using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace StandForFH5Revival
{
    public class MemoryManager : IDisposable
    {
        private const string GAME_PROCESS_NAME = "ForzaHorizon5";
        private Process _gameProcess;
        private bool _isAttached;
        private bool _privilegesEnabled;

        public Process GameProcess => _gameProcess;
        public bool IsAttached => _isAttached && IsGameRunning();

        public bool AttachToGame()
        {
            try
            {
                if (!_privilegesEnabled && !EnableSeDebugPrivilege())
                {
                    return false;
                }

                var processes = Process.GetProcessesByName(GAME_PROCESS_NAME);
                if (processes.Length == 0)
                {
                    _isAttached = false;
                    return false;
                }

                var newProcess = processes[0];
                
                if (_gameProcess == null || _gameProcess.Id != newProcess.Id || _gameProcess.HasExited)
                {
                    _gameProcess?.Dispose();
                    _gameProcess = newProcess;
                    
                    bool gameIs64Bit = Is64BitProcess(_gameProcess);
                    bool appIs64Bit = Environment.Is64BitProcess;
                    
                    if (gameIs64Bit != appIs64Bit)
                    {
                        return false;
                    }
                    
                    var memory = Memory.GetInstance();
                    memory.MProc.Process = _gameProcess;
                    memory.MProc.ProcessId = _gameProcess.Id;
                    memory.MProc.Handle = _gameProcess.Handle;
                    memory.MProc.Is64Bit = gameIs64Bit;
                }
                else
                {
                    newProcess.Dispose();
                }

                _isAttached = true;
                return true;
            }
            catch
            {
                _isAttached = false;
                return false;
            }
        }

        public bool IsGameRunning()
        {
            try
            {
                if (_gameProcess != null && !_gameProcess.HasExited)
                {
                    return true;
                }

                var processes = Process.GetProcessesByName(GAME_PROCESS_NAME);
                var isRunning = processes.Length > 0;
                
                foreach (var process in processes)
                {
                    process.Dispose();
                }

                if (!isRunning)
                {
                    _isAttached = false;
                    if (_gameProcess != null)
                    {
                        _gameProcess.Dispose();
                        _gameProcess = null;
                    }
                }

                return isRunning;
            }
            catch
            {
                return false;
            }
        }

        public bool EnableSeDebugPrivilege()
        {
            try
            {
                using (var identity = WindowsIdentity.GetCurrent())
                {
                    var principal = new WindowsPrincipal(identity);
                    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        return false;
                    }
                }

                if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out IntPtr tokenHandle))
                {
                    return false;
                }

                try
                {
                    if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, out LUID luid))
                    {
                        return false;
                    }

                    var tokenPrivileges = new TOKEN_PRIVILEGES
                    {
                        PrivilegeCount = 1,
                        Privileges = new LUID_AND_ATTRIBUTES[1]
                    };
                    
                    tokenPrivileges.Privileges[0].Luid = luid;
                    tokenPrivileges.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

                    if (!AdjustTokenPrivileges(tokenHandle, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero))
                    {
                        return false;
                    }

                    if (Marshal.GetLastWin32Error() == ERROR_NOT_ALL_ASSIGNED)
                    {
                        return false;
                    }

                    _privilegesEnabled = true;
                    return true;
                }
                finally
                {
                    CloseHandle(tokenHandle);
                }
            }
            catch
            {
                return false;
            }
        }

        public void Detach()
        {
            _isAttached = false;
            
            if (_gameProcess != null)
            {
                _gameProcess.Dispose();
                _gameProcess = null;
            }

            Memory.ResetInstance();
        }

        public void Dispose()
        {
            Detach();
        }

        private static bool Is64BitProcess(Process process)
        {
            try
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    if (IsWow64Process(process.Handle, out bool isWow64))
                    {
                        return !isWow64;
                    }
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        private const int TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const int TOKEN_QUERY = 0x0008;
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const string SE_DEBUG_NAME = "SeDebugPrivilege";
        private const int ERROR_NOT_ALL_ASSIGNED = 1300;

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public uint Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);
    }
}