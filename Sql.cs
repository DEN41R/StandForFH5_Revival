using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace StandForFH5Revival
{
    public class Sql : CheatsUtilities
    {
        private UIntPtr _cDatabaseAddress;
        private UIntPtr _ptr;

        public override string CheatName => "SQL Database";
        public bool WereScansSuccessful { get; private set; }

        public async Task SqlExecAobScan()
        {
            WereScansSuccessful = false;
            _cDatabaseAddress = UIntPtr.Zero;
            _ptr = UIntPtr.Zero;

            try
            {
                var memory = Memory.GetInstance();
                if (memory?.MProc?.Handle == IntPtr.Zero)
                {
                    return;
                }

                const string sig = "0F 84 ? ? ? ? 48 8B 35 ? ? ? ? 48 85 F6 74";
                _cDatabaseAddress = await SmartAobScan(sig);

                if (_cDatabaseAddress.ToUInt64() > 0)
                {
                    var relativeAddress = new UIntPtr(_cDatabaseAddress.ToUInt64() + 0x6 + 0x3);
                    if (!ValidateAddress(relativeAddress))
                    {
                        return;
                    }
                    
                    var relative = memory.ReadInt32(relativeAddress);
                    var pCDataBaseAddress = new UIntPtr(_cDatabaseAddress.ToUInt64() + (ulong)relative + 0x6 + 0x7);
                    
                    if (!ValidateAddress(pCDataBaseAddress))
                    {
                        return;
                    }
                    
                    _ptr = memory.ReadUIntPtr(pCDataBaseAddress);
                    if (_ptr.ToUInt64() == 0 || !ValidateAddress(_ptr))
                    {
                        return;
                    }

                    var testVFunc = GetVirtualFunctionPtr(_ptr, 9);
                    if (testVFunc == UIntPtr.Zero)
                    {
                        return;
                    }

                    WereScansSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                LogError("SQL", $"SqlExecAobScan failed: {ex.Message}");
            }
        }

        public async void Query(string command)
        {
            try
            {
                var memory = Memory.GetInstance();
                var procHandle = memory?.MProc?.Handle ?? IntPtr.Zero;

                if (procHandle == IntPtr.Zero)
                {
                    return;
                }

                if (_ptr == UIntPtr.Zero)
                {
                    await SqlExecAobScan();
                }

                if (_ptr == UIntPtr.Zero)
                {
                    return;
                }

                var rcx = _ptr;
                var callFunction = GetVirtualFunctionPtr(_ptr, 9);
                if (callFunction == UIntPtr.Zero)
                {
                    return;
                }

                var mainMod = memory.MProc?.Process?.MainModule;
                if (mainMod == null)
                {
                    return;
                }

                await ExecuteSqlCommandSimplified(procHandle, rcx, callFunction, command, mainMod);
            }
            catch (Exception ex)
            {
                LogError("SQL", $"Query failed: {ex.Message}");
            }
        }

        private async Task<bool> ExecuteSqlCommandSimplified(IntPtr procHandle, UIntPtr rcx, UIntPtr callFunction, string command, System.Diagnostics.ProcessModule mainMod)
        {
            UIntPtr jmpShellcodeaddr = UIntPtr.Zero;
            UIntPtr rdx = UIntPtr.Zero;
            UIntPtr r8 = UIntPtr.Zero;
            IntPtr thread = IntPtr.Zero;

            try
            {
                var memory = Memory.GetInstance();
                var shellCodeAddress = new UIntPtr((ulong)mainMod.BaseAddress + 0x1000);
                
                jmpShellcodeaddr = VirtualAllocEx(procHandle, UIntPtr.Zero, 0x1000, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
                rdx = VirtualAllocEx(procHandle, UIntPtr.Zero, 0x1000, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
                r8 = VirtualAllocEx(procHandle, UIntPtr.Zero, 0x1000, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

                if (jmpShellcodeaddr == UIntPtr.Zero || rdx == UIntPtr.Zero || r8 == UIntPtr.Zero)
                {
                    return false;
                }

                var rcxBytes = BitConverter.GetBytes(rcx.ToUInt64());
                var rdxBytes = BitConverter.GetBytes(rdx.ToUInt64());
                var r8Bytes = BitConverter.GetBytes(r8.ToUInt64());
                var callBytes = BitConverter.GetBytes(callFunction.ToUInt64());

                byte[] shellCode = new byte[]
                {
                    0x51, 0x52, 0x41, 0x50, 0x41, 0x51, 0x48, 0x83, 0xEC, 0x28, 0x48, 0xB9, 
                    rcxBytes[0], rcxBytes[1], rcxBytes[2], rcxBytes[3], rcxBytes[4], rcxBytes[5], rcxBytes[6], rcxBytes[7], 
                    0x48, 0xBA, rdxBytes[0], rdxBytes[1], rdxBytes[2], rdxBytes[3], rdxBytes[4], rdxBytes[5], rdxBytes[6], rdxBytes[7], 
                    0x49, 0xB8, r8Bytes[0], r8Bytes[1], r8Bytes[2], r8Bytes[3], r8Bytes[4], r8Bytes[5], r8Bytes[6], r8Bytes[7], 
                    0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, 
                    callBytes[0], callBytes[1], callBytes[2], callBytes[3], callBytes[4], callBytes[5], callBytes[6], callBytes[7], 
                    0x48, 0x83, 0xC4, 0x28, 0x41, 0x59, 0x41, 0x58, 0x5A, 0x59, 0xC3
                };

                shellCodeAddress = new UIntPtr(shellCodeAddress.ToUInt64() - (ulong)shellCode.Length);
                
                if (!memory.ChangeProtection(shellCodeAddress, MemoryProtection.ExecuteReadWrite, out var oldProtection))
                {
                    return false;
                }

                try
                {
                    memory.WriteStringMemory(r8, command + "\0");
                    memory.WriteArrayMemory(shellCodeAddress, shellCode);

                    var jmpBytes = BitConverter.GetBytes(shellCodeAddress.ToUInt64());
                    byte[] jmpShellcode = new byte[]
                    {
                        0xFF, 0x25, 0x00, 0x00, 0x00, 0x00, 
                        jmpBytes[0], jmpBytes[1], jmpBytes[2], jmpBytes[3], jmpBytes[4], jmpBytes[5], jmpBytes[6], jmpBytes[7]
                    };

                    memory.WriteArrayMemory(jmpShellcodeaddr, jmpShellcode);

                    thread = CreateRemoteThread(procHandle, IntPtr.Zero, UIntPtr.Zero, jmpShellcodeaddr, IntPtr.Zero, 0, out _);
                    if (thread == IntPtr.Zero || thread == new IntPtr(-1))
                    {
                        return false;
                    }

                    WaitForSingleObject(thread, int.MaxValue);
                    return true;
                }
                finally
                {
                    memory.ChangeProtection(shellCodeAddress, oldProtection, out _);
                }
            }
            catch (Exception ex)
            {
                LogError("SQL", $"ExecuteSqlCommandSimplified failed: {ex.Message}");
                return false;
            }
            finally
            {
                if (thread != IntPtr.Zero && thread != new IntPtr(-1))
                {
                    CloseHandle(thread);
                }
                
                if (jmpShellcodeaddr != UIntPtr.Zero)
                    Free(jmpShellcodeaddr);
                if (r8 != UIntPtr.Zero)
                    Free(r8);
                if (rdx != UIntPtr.Zero)
                    Free(rdx);
            }
        }

        public void Reset()
        {
            WereScansSuccessful = false;
            var fields = typeof(Sql).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                                   .Where(f => f.FieldType == typeof(UIntPtr));
            
            foreach (var field in fields)
            {
                field.SetValue(this, UIntPtr.Zero);
            }
        }

        private static UIntPtr GetVirtualFunctionPtr(UIntPtr ptr, int index)
        {
            try
            {
                if (ptr == UIntPtr.Zero)
                    return UIntPtr.Zero;

                var memory = Memory.GetInstance();
                if (memory?.MProc?.Handle == IntPtr.Zero)
                    return UIntPtr.Zero;

                var pVTable = memory.ReadUIntPtr(ptr);
                if (pVTable == UIntPtr.Zero)
                    return UIntPtr.Zero;

                var lpBaseAddress = new UIntPtr(pVTable.ToUInt64() + (ulong)UIntPtr.Size * (ulong)index);
                return memory.ReadUIntPtr(lpBaseAddress);
            }
            catch
            {
                return UIntPtr.Zero;
            }
        }

        private static new bool ValidateAddress(UIntPtr address)
        {
            if (address == UIntPtr.Zero)
                return false;

            try
            {
                var memory = Memory.GetInstance();
                var handle = memory?.MProc?.Handle ?? IntPtr.Zero;
                
                if (handle == IntPtr.Zero)
                    return false;

                var addressValue = address.ToUInt64();
                if (addressValue < 0x10000 || addressValue > 0x7FFFFFFFFFFF)
                    return false;

                var testBuffer = new byte[8];
                return NativeMethods.ReadProcessMemory(handle, address, testBuffer, (UIntPtr)8, UIntPtr.Zero);
            }
            catch
            {
                return false;
            }
        }

        private static new void Free(UIntPtr address)
        {
            if (address == UIntPtr.Zero) 
                return;

            try
            {
                var memory = Memory.GetInstance();
                var handle = memory?.MProc?.Handle ?? IntPtr.Zero;
                
                if (handle != IntPtr.Zero)
                {
                    VirtualFreeEx(handle, address, UIntPtr.Zero, MEM_RELEASE);
                }
            }
            catch { }
        }

        public override void Cleanup()
        {
            WereScansSuccessful = false;
            _cDatabaseAddress = UIntPtr.Zero;
            _ptr = UIntPtr.Zero;

            base.Cleanup();
        }

        public override bool Revert()
        {
            try
            {
                Reset();
                IsReverted = true;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("SQL Revert", ex.Message);
                return false;
            }
        }

        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RELEASE = 0x8000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr VirtualAllocEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr VirtualFreeEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            UIntPtr dwStackSize,
            UIntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }

    public enum MemoryProtection : uint
    {
        ExecuteReadWrite = 0x40
    }
}