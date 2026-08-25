using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StandForFH5Revival.Interfaces;

namespace StandForFH5Revival
{
    public abstract class CheatsUtilities : ICheatsBase, IRevertBase
    {
        public abstract string CheatName { get; }
        public virtual bool IsActive { get; protected set; }

        public virtual bool Initialize()
        {
            IsActive = true;
            return true;
        }

        public virtual void Cleanup()
        {
            if (CanRevert && !IsReverted)
            {
                Revert();
            }
            IsActive = false;
        }

        public virtual bool CanRevert { get; protected set; }
        public virtual bool IsReverted { get; protected set; }

        protected readonly Dictionary<UIntPtr, byte[]> _originalBytes = new Dictionary<UIntPtr, byte[]>();

        public virtual void StoreOriginalBytes(UIntPtr address, byte[] originalBytes)
        {
            if (address == UIntPtr.Zero || originalBytes == null || originalBytes.Length == 0)
                return;

            _originalBytes[address] = new byte[originalBytes.Length];
            Array.Copy(originalBytes, _originalBytes[address], originalBytes.Length);
            CanRevert = true;
        }

        public virtual bool Revert()
        {
            if (!CanRevert || IsReverted)
                return false;

            try
            {
                var memory = Memory.GetInstance();
                foreach (var kvp in _originalBytes)
                {
                    var success = NativeMethods.WriteProcessMemory(
                        memory.MProc.Handle, 
                        kvp.Key, 
                        kvp.Value, 
                        (UIntPtr)kvp.Value.Length, 
                        UIntPtr.Zero);
                    
                    if (!success)
                    {
                        ShowError("Memory Revert", $"Failed to restore bytes at 0x{kvp.Key.ToUInt64():X}");
                        return false;
                    }
                }
                
                _originalBytes.Clear();
                IsReverted = true;
                return true;
            }
            catch (Exception ex)
            {
                ShowError("Memory Revert", ex.Message);
                return false;
            }
        }

        protected static bool ValidateAddress(UIntPtr address)
        {
            if (address == UIntPtr.Zero)
                return false;

            try
            {
                var memory = Memory.GetInstance();
                var process = memory.MProc?.Process;
                
                if (process?.MainModule == null)
                    return false;

                var minAddress = (ulong)process.MainModule.BaseAddress;
                var maxAddress = minAddress + (ulong)process.MainModule.ModuleMemorySize;
                var addressValue = address.ToUInt64();

                if (addressValue < minAddress || addressValue > maxAddress)
                {
                    return IsAddressInValidMemoryRegion(address);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected static bool ValidateAddresses(params UIntPtr[] addresses)
        {
            if (addresses == null || addresses.Length == 0)
                return false;

            foreach (var address in addresses)
            {
                if (!ValidateAddress(address))
                    return false;
            }

            return true;
        }

        private static bool IsAddressInValidMemoryRegion(UIntPtr address)
        {
            try
            {
                var memory = Memory.GetInstance();
                var handle = memory.MProc?.Handle ?? IntPtr.Zero;
                
                if (handle == IntPtr.Zero)
                    return false;

                var result = NativeMethods.VirtualQueryEx(handle, address, out var mbi, (UIntPtr)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                
                if (result == UIntPtr.Zero)
                    return false;

                return (mbi.State & NativeMethods.MEM_COMMIT) != 0 && 
                       (mbi.Protect & (NativeMethods.PAGE_NOACCESS | NativeMethods.PAGE_GUARD)) == 0;
            }
            catch
            {
                return false;
            }
        }

        protected static bool ValidateAddressRange(UIntPtr address, uint size)
        {
            if (address == UIntPtr.Zero || size == 0)
                return false;

            try
            {
                if (!ValidateAddress(address))
                    return false;

                var endAddress = new UIntPtr(address.ToUInt64() + size - 1);
                if (!ValidateAddress(endAddress))
                    return false;

                if (size > 0x1000)
                {
                    var midAddress = new UIntPtr(address.ToUInt64() + size / 2);
                    if (!ValidateAddress(midAddress))
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected static async Task<UIntPtr> SmartAobScan(string search, UIntPtr? start = null, UIntPtr? end = null)
        {
            try
            {
                var memory = Memory.GetInstance();
                var process = memory.MProc?.Process;
                
                if (process?.MainModule == null)
                    return UIntPtr.Zero;

                var minRange = (long)process.MainModule.BaseAddress;
                var maxRange = minRange + process.MainModule.ModuleMemorySize;

                if (start != null)
                {
                    minRange = (long)start.Value.ToUInt64();
                }
                
                if (end != null)
                {
                    maxRange = (long)end.Value.ToUInt64();
                }

                return await ScanMemoryRegions(minRange, maxRange, search);
            }
            catch (Exception ex)
            {
                ShowError("Pattern Scanning", $"Pattern: {search}, Error: {ex.Message}");
                return UIntPtr.Zero;
            }
        }

        private static async Task<UIntPtr> ScanMemoryRegions(long startAddress, long endAddress, string pattern)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var memory = Memory.GetInstance();
                    var handle = memory.MProc?.Handle ?? IntPtr.Zero;

                    if (handle == IntPtr.Zero)
                        return UIntPtr.Zero;

                    var patternBytes = ParsePatternString(pattern, out var mask);
                    if (patternBytes == null || patternBytes.Length == 0)
                        return UIntPtr.Zero;

                    var process = memory.MProc.Process;
                    if (process?.MainModule != null)
                    {
                        var moduleBase = (long)process.MainModule.BaseAddress;
                        var moduleSize = process.MainModule.ModuleMemorySize;
                        
                        var result = ScanModuleMemory(handle, new UIntPtr((ulong)moduleBase), moduleSize, patternBytes, mask);
                        if (result != UIntPtr.Zero)
                            return result;
                    }

                    var currentAddress = startAddress;
                    var mbi = new MEMORY_BASIC_INFORMATION();

                    while (currentAddress < endAddress)
                    {
                        var result = NativeMethods.VirtualQueryEx(handle, new UIntPtr((ulong)currentAddress), out mbi, (UIntPtr)Marshal.SizeOf(mbi));
                        
                        if (result == UIntPtr.Zero)
                            break;

                        if ((mbi.State & NativeMethods.MEM_COMMIT) != 0 && 
                            (mbi.Protect & (NativeMethods.PAGE_NOACCESS | NativeMethods.PAGE_GUARD)) == 0 &&
                            (mbi.Type & 0x1000000) != 0)
                        {
                            var regionStart = Math.Max(currentAddress, (long)mbi.BaseAddress.ToUInt64());
                            var regionEnd = Math.Min(endAddress, (long)(mbi.BaseAddress.ToUInt64() + mbi.RegionSize.ToUInt64()));
                            var regionSize = regionEnd - regionStart;

                            if (regionSize > 0 && regionSize <= int.MaxValue)
                            {
                                var foundAddress = ScanRegion(handle, new UIntPtr((ulong)regionStart), (int)regionSize, patternBytes, mask);
                                if (foundAddress != UIntPtr.Zero)
                                    return foundAddress;
                            }
                        }

                        currentAddress = (long)(mbi.BaseAddress.ToUInt64() + mbi.RegionSize.ToUInt64());
                    }

                    return UIntPtr.Zero;
                }
                catch
                {
                    return UIntPtr.Zero;
                }
            });
        }

        private static UIntPtr ScanModuleMemory(IntPtr processHandle, UIntPtr baseAddress, int moduleSize, byte[] pattern, string mask)
        {
            try
            {
                const int chunkSize = 0x100000;
                var currentOffset = 0;

                while (currentOffset < moduleSize)
                {
                    var remainingSize = Math.Min(chunkSize, moduleSize - currentOffset);
                    var currentAddress = new UIntPtr(baseAddress.ToUInt64() + (ulong)currentOffset);
                    
                    var result = ScanRegion(processHandle, currentAddress, remainingSize, pattern, mask);
                    if (result != UIntPtr.Zero)
                        return result;
                    
                    currentOffset += remainingSize;
                }

                return UIntPtr.Zero;
            }
            catch
            {
                return UIntPtr.Zero;
            }
        }

        private static byte[] ParsePatternString(string pattern, out string mask)
        {
            var parts = pattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var bytes = new List<byte>();
            var maskBuilder = new StringBuilder();

            foreach (var part in parts)
            {
                if (part == "?" || part == "??" || part.Contains("?"))
                {
                    bytes.Add(0x00);
                    maskBuilder.Append('?');
                }
                else
                {
                    if (byte.TryParse(part, NumberStyles.HexNumber, null, out var b))
                    {
                        bytes.Add(b);
                        maskBuilder.Append('x');
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid hex byte: {part}");
                    }
                }
            }

            mask = maskBuilder.ToString();
            return bytes.ToArray();
        }

        private static UIntPtr ScanRegion(IntPtr processHandle, UIntPtr baseAddress, int regionSize, byte[] pattern, string mask)
        {
            try
            {
                if (regionSize > 0x1000000)
                    regionSize = 0x1000000;

                var buffer = new byte[regionSize];
                if (!NativeMethods.ReadProcessMemory(processHandle, baseAddress, buffer, (UIntPtr)regionSize, UIntPtr.Zero))
                {
                    return ScanRegionInChunks(processHandle, baseAddress, regionSize, pattern, mask);
                }

                for (int i = 0; i <= buffer.Length - pattern.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (mask[j] == 'x' && buffer[i + j] != pattern[j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        return new UIntPtr(baseAddress.ToUInt64() + (ulong)i);
                    }
                }

                return UIntPtr.Zero;
            }
            catch
            {
                return UIntPtr.Zero;
            }
        }

        private static UIntPtr ScanRegionInChunks(IntPtr processHandle, UIntPtr baseAddress, int regionSize, byte[] pattern, string mask)
        {
            try
            {
                const int chunkSize = 0x10000;
                var overlap = pattern.Length - 1;
                
                for (int offset = 0; offset < regionSize; offset += chunkSize - overlap)
                {
                    var currentSize = Math.Min(chunkSize, regionSize - offset);
                    if (currentSize <= 0) break;

                    var currentAddress = new UIntPtr(baseAddress.ToUInt64() + (ulong)offset);
                    var buffer = new byte[currentSize];

                    if (NativeMethods.ReadProcessMemory(processHandle, currentAddress, buffer, (UIntPtr)currentSize, UIntPtr.Zero))
                    {
                        for (int i = 0; i <= buffer.Length - pattern.Length; i++)
                        {
                            bool found = true;
                            for (int j = 0; j < pattern.Length; j++)
                            {
                                if (mask[j] == 'x' && buffer[i + j] != pattern[j])
                                {
                                    found = false;
                                    break;
                                }
                            }

                            if (found)
                            {
                                return new UIntPtr(currentAddress.ToUInt64() + (ulong)i);
                            }
                        }
                    }
                }

                return UIntPtr.Zero;
            }
            catch
            {
                return UIntPtr.Zero;
            }
        }

        protected static void ShowError(string feature, string details)
        {
            try
            {
                LogError(feature, details);
                MessageBox.Show($"Error in {feature}!\n\nDetails: {details}", "Stand for FH5 Revival - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }

        protected static void LogError(string feature, string details)
        {
            try
            {
                Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in {feature}: {details}");
            }
            catch { }
        }

        protected static bool ExecuteWithErrorHandling(Action action, string featureName, Action fallbackAction = null)
        {
            try
            {
                action?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                ShowError(featureName, ex.Message);
                try { fallbackAction?.Invoke(); } catch { }
                return false;
            }
        }

        protected static T ExecuteWithErrorHandling<T>(Func<T> func, string featureName, T defaultValue = default, Func<T> fallbackFunc = null)
        {
            try
            {
                return func != null ? func.Invoke() : defaultValue;
            }
            catch (Exception ex)
            {
                ShowError(featureName, ex.Message);
                if (fallbackFunc != null)
                {
                    try { return fallbackFunc.Invoke(); } catch { }
                }
                return defaultValue;
            }
        }

        protected static async Task<T> ExecuteWithErrorHandling<T>(Func<Task<T>> func, string featureName, T defaultValue = default, Func<Task<T>> fallbackFunc = null)
        {
            try
            {
                return func != null ? await func.Invoke() : defaultValue;
            }
            catch (Exception ex)
            {
                ShowError(featureName, ex.Message);
                if (fallbackFunc != null)
                {
                    try { return await fallbackFunc.Invoke(); } catch { }
                }
                return defaultValue;
            }
        }

        protected static void Free(UIntPtr address)
        {
            if (address == UIntPtr.Zero) 
                return;

            try
            {
                var memory = Memory.GetInstance();
                var handle = memory.MProc?.Handle ?? IntPtr.Zero;
                
                if (handle != IntPtr.Zero)
                {
                    NativeMethods.VirtualFreeEx(handle, address, UIntPtr.Zero, NativeMethods.MEM_RELEASE);
                }
            }
            catch { }
        }

        protected static byte[] CalculateDetour(UIntPtr address, UIntPtr target, int replaceCount)
        {
            if (replaceCount < 5)
                throw new ArgumentException("Replace count must be at least 5 bytes for JMP instruction");

            var detourBytes = new byte[replaceCount];
            detourBytes[0] = 0xE9;
            var offset = (int)((long)target.ToUInt64() - (long)address.ToUInt64() - 5);
            BitConverter.GetBytes(offset).CopyTo(detourBytes, 1);
            for (var i = 5; i < detourBytes.Length; i++)
            {
                detourBytes[i] = 0x90;
            }
            return detourBytes;
        }

        protected bool CreateDetour(UIntPtr address, UIntPtr target, int replaceCount)
        {
            return ExecuteWithErrorHandling(() =>
            {
                if (!ValidateAddress(address) || !ValidateAddress(target))
                {
                    throw new ArgumentException("Invalid address provided for detour");
                }

                var memory = Memory.GetInstance();
                var handle = memory.MProc?.Handle ?? IntPtr.Zero;

                if (handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Process handle is null");
                }

                var originalBytes = new byte[replaceCount];
                if (!NativeMethods.ReadProcessMemory(handle, address, originalBytes, (UIntPtr)replaceCount, UIntPtr.Zero))
                {
                    throw new InvalidOperationException("Failed to read original bytes");
                }

                StoreOriginalBytes(address, originalBytes);

                var detourBytes = CalculateDetour(address, target, replaceCount);

                if (!NativeMethods.WriteProcessMemory(handle, address, detourBytes, (UIntPtr)detourBytes.Length, UIntPtr.Zero))
                {
                    throw new InvalidOperationException("Failed to write detour bytes");
                }

                return true;
            }, "CreateDetour", false);
        }

        protected UIntPtr CreateCodeCave(uint size)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var memory = Memory.GetInstance();
                var handle = memory.MProc?.Handle ?? IntPtr.Zero;

                if (handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Process handle is null");
                }

                var address = NativeMethods.VirtualAllocEx(handle, UIntPtr.Zero, (UIntPtr)size,
                    NativeMethods.MEM_COMMIT | NativeMethods.MEM_RESERVE,
                    NativeMethods.PAGE_EXECUTE_READWRITE);

                if (address == UIntPtr.Zero)
                {
                    throw new OutOfMemoryException("Failed to allocate code cave");
                }

                if (!ValidateAddress(address))
                {
                    NativeMethods.VirtualFreeEx(handle, address, UIntPtr.Zero, NativeMethods.MEM_RELEASE);
                    throw new InvalidOperationException("Allocated code cave address is invalid");
                }

                return address;
            }, "CreateCodeCave", UIntPtr.Zero);
        }
    }

    internal static class NativeMethods
    {
        public const uint MEM_RELEASE = 0x8000;
        public const uint MEM_COMMIT = 0x1000;
        public const uint MEM_RESERVE = 0x2000;
        public const uint PAGE_NOACCESS = 0x01;
        public const uint PAGE_GUARD = 0x100;
        public const uint PAGE_EXECUTE_READWRITE = 0x40;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UIntPtr VirtualFreeEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint dwFreeType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            UIntPtr lpBaseAddress,
            byte[] lpBuffer,
            UIntPtr nSize,
            UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            UIntPtr lpBaseAddress,
            byte[] lpBuffer,
            UIntPtr nSize,
            UIntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UIntPtr VirtualAllocEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UIntPtr VirtualQueryEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer,
            UIntPtr dwLength);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MEMORY_BASIC_INFORMATION
    {
        public UIntPtr BaseAddress;
        public UIntPtr AllocationBase;
        public uint AllocationProtect;
        public UIntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }
}