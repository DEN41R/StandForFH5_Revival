using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StandForFH5Revival
{
    public static class Memory
    {
        private static MemoryInstance _instance;
        private static readonly object _lock = new object();

        public static MemoryInstance GetInstance()
        {
            if (_instance != null) 
                return _instance;

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new MemoryInstance();
                }
            }

            return _instance;
        }

        public static void ResetInstance()
        {
            lock (_lock)
            {
                _instance?.Dispose();
                _instance = null;
            }
        }

        public static bool IsInitialized()
        {
            return _instance?.MProc?.Process != null && !_instance.MProc.Process.HasExited;
        }
    }

    public class MemoryInstance : IDisposable
    {
        public ProcessInfo MProc { get; private set; }

        public MemoryInstance()
        {
            MProc = new ProcessInfo();
        }

        public int ReadInt32(UIntPtr address)
        {
            var buffer = new byte[4];
            if (ProcessNativeMethods.ReadProcessMemory(MProc.Handle, address, buffer, (UIntPtr)buffer.Length, out _))
            {
                return BitConverter.ToInt32(buffer, 0);
            }
            throw new InvalidOperationException($"Failed to read int32 from address 0x{address.ToUInt64():X}");
        }

        public T ReadMemory<T>(UIntPtr address) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];
            if (ProcessNativeMethods.ReadProcessMemory(MProc.Handle, address, buffer, (UIntPtr)buffer.Length, out _))
            {
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                try
                {
                    return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                }
                finally
                {
                    handle.Free();
                }
            }
            throw new InvalidOperationException($"Failed to read {typeof(T).Name} from address 0x{address.ToUInt64():X}");
        }

        public UIntPtr CreateDetour(UIntPtr address, byte[] detourCode, int replaceCount)
        {
            try
            {
                var detourAddress = ProcessNativeMethods.VirtualAllocEx(
                    MProc.Handle, 
                    UIntPtr.Zero, 
                    (UIntPtr)detourCode.Length,
                    0x3000,
                    0x40);

                if (detourAddress == UIntPtr.Zero)
                    return UIntPtr.Zero;

                if (!ProcessNativeMethods.WriteProcessMemory(MProc.Handle, detourAddress, detourCode, (UIntPtr)detourCode.Length, out _))
                {
                    ProcessNativeMethods.VirtualFreeEx(MProc.Handle, detourAddress, UIntPtr.Zero, 0x8000);
                    return UIntPtr.Zero;
                }

                var jumpBytes = CalculateJump(address, detourAddress, replaceCount);

                if (!ProcessNativeMethods.WriteProcessMemory(MProc.Handle, address, jumpBytes, (UIntPtr)jumpBytes.Length, out _))
                {
                    ProcessNativeMethods.VirtualFreeEx(MProc.Handle, detourAddress, UIntPtr.Zero, 0x8000);
                    return UIntPtr.Zero;
                }

                return detourAddress;
            }
            catch
            {
                return UIntPtr.Zero;
            }
        }

        private byte[] CalculateJump(UIntPtr from, UIntPtr to, int replaceCount)
        {
            var jumpBytes = new byte[replaceCount];
            jumpBytes[0] = 0xE9;
            var offset = (int)((long)to.ToUInt64() - (long)from.ToUInt64() - 5);
            BitConverter.GetBytes(offset).CopyTo(jumpBytes, 1);
            for (var i = 5; i < jumpBytes.Length; i++)
            {
                jumpBytes[i] = 0x90;
            }
            return jumpBytes;
        }

        public UIntPtr ReadUIntPtr(UIntPtr address)
        {
            var buffer = new byte[UIntPtr.Size];
            if (ProcessNativeMethods.ReadProcessMemory(MProc.Handle, address, buffer, (UIntPtr)buffer.Length, out _))
            {
                return UIntPtr.Size == 8
                    ? new UIntPtr(BitConverter.ToUInt64(buffer, 0))
                    : new UIntPtr(BitConverter.ToUInt32(buffer, 0));
            }
            throw new InvalidOperationException($"Failed to read UIntPtr from address 0x{address.ToUInt64():X}");
        }

        public bool ChangeProtection(UIntPtr address, MemoryProtection newProtection, out MemoryProtection oldProtection)
        {
            oldProtection = MemoryProtection.ExecuteReadWrite;
            if (ProcessNativeMethods.VirtualProtectEx(MProc.Handle, address, (UIntPtr)1, (uint)newProtection, out var old))
            {
                oldProtection = (MemoryProtection)old;
                return true;
            }
            return false;
        }

        public void WriteStringMemory(UIntPtr address, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (!ProcessNativeMethods.WriteProcessMemory(MProc.Handle, address, bytes, (UIntPtr)bytes.Length, out _))
            {
                throw new InvalidOperationException($"Failed to write string to address 0x{address.ToUInt64():X}");
            }
        }

        public void WriteArrayMemory(UIntPtr address, byte[] data)
        {
            if (!ProcessNativeMethods.WriteProcessMemory(MProc.Handle, address, data, (UIntPtr)data.Length, out _))
            {
                throw new InvalidOperationException($"Failed to write array to address 0x{address.ToUInt64():X}");
            }
        }

        public void Dispose()
        {
            MProc?.Dispose();
        }
    }

    public class ProcessInfo : IDisposable
    {
        public Process Process { get; set; }
        public IntPtr Handle { get; set; }
        public int ProcessId { get; set; }
        public bool Is64Bit { get; set; } = true;

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
            {
                try { ProcessNativeMethods.CloseHandle(Handle); } catch { }
                Handle = IntPtr.Zero;
            }

            Process?.Dispose();
            Process = null;
        }
    }

    internal static class ProcessNativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            UIntPtr lpBaseAddress,
            byte[] lpBuffer,
            UIntPtr nSize,
            out UIntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            UIntPtr lpBaseAddress,
            byte[] lpBuffer,
            UIntPtr nSize,
            out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtectEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UIntPtr VirtualAllocEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint dwFreeType);
    }
}