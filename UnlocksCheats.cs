using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace StandForFH5Revival
{
    public class UnlocksCheats : CheatsUtilities
    {
        private UIntPtr _getRewardsAddress;
        private UIntPtr _getPerkPrizeAddress;
        private UIntPtr _getWheelspinsAddress;
        private UIntPtr _getSuperWheelspinsAddress;
        private float _creditsValue = 999999999f;

        public override string CheatName => "Credits Cheat";

        public float CreditsValue 
        { 
            get => _creditsValue; 
            set => _creditsValue = value; 
        }

        public enum EPerkType : byte
        {
            Unknown = 0,
            XP = 1,
            FP = 2,
            Credits = 3
        }

        public async Task<bool> CheatPerkPrize(float value, EPerkType type)
        {
            try
            {
                if (_getRewardsAddress == UIntPtr.Zero)
                {
                    _getRewardsAddress = await SmartAobScan("48 89 ? ? ? 57 48 83 EC ? 33 FF E8 ? ? ? ? 48 8B ? 48 8B ? FF 52");
                    if (_getRewardsAddress == UIntPtr.Zero)
                    {
                        ShowError("CheatPerkPrize", "Failed to find GetRewards address with signature");
                        return false;
                    }
                }

                if (_getPerkPrizeAddress == UIntPtr.Zero)
                {
                    _getPerkPrizeAddress = await SmartAobScan("48 89 ? ? ? 57 48 83 EC ? 48 8B ? 48 8B ? E8 ? ? ? ? 83 E8");
                    if (_getPerkPrizeAddress == UIntPtr.Zero)
                    {
                        ShowError("CheatPerkPrize", "Failed to find GetPerkPrize address with signature");
                        return false;
                    }
                }

                if (_getRewardsAddress == UIntPtr.Zero || _getPerkPrizeAddress == UIntPtr.Zero)
                {
                    ShowError("CheatPerkPrize", "_getRewardsAddress == 0 || _getPerkPrizeAddress == 0");
                    return false;
                }

                byte[] bVal = BitConverter.GetBytes(value);
                byte[] bRewards = BitConverter.GetBytes(_getRewardsAddress.ToUInt64());
                byte[] bPerkPrize = BitConverter.GetBytes(_getPerkPrizeAddress.ToUInt64());
                
                byte[] asm = new byte[]
                {
                    0x51, 0x52, 0x41, 0x50, 0x41, 0x51, 0x48, 0x83, 0xEC, 0x28, 0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08,
                    bRewards[0], bRewards[1], bRewards[2], bRewards[3], bRewards[4], bRewards[5], bRewards[6], bRewards[7],
                    0x48, 0x8B, 0xD0, 0x48, 0x8D, 0x05, 0x51, 0x00, 0x00, 0x00, 0xC7, 0x40, 0x40, bVal[0], bVal[1], bVal[2],
                    bVal[3], 0xC6, 0x40, 0x50, (byte)type, 0x48, 0x8D, 0x0D, 0x1F, 0x00, 0x00, 0x00, 0x48, 0x89, 0x41, 0x18,
                    0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, bPerkPrize[0], bPerkPrize[1], bPerkPrize[2], bPerkPrize[3],
                    bPerkPrize[4], bPerkPrize[5], bPerkPrize[6], bPerkPrize[7], 0x48, 0x83, 0xC4, 0x28, 0x41, 0x59, 0x41, 0x58,
                    0x5A, 0x59, 0xC3
                };

                var memory = Memory.GetInstance();
                IntPtr procHandle = memory.MProc?.Handle ?? IntPtr.Zero;
                
                if (procHandle == IntPtr.Zero)
                {
                    ShowError("CheatPerkPrize", "Process handle is null");
                    return false;
                }

                if (memory.MProc?.Process?.HasExited == true)
                {
                    ShowError("CheatPerkPrize", "Game process has exited");
                    return false;
                }

                UIntPtr asmAddress = VirtualAllocEx(procHandle, UIntPtr.Zero, (UIntPtr)0x1000, 0x3000, 0x40);
                if (asmAddress == UIntPtr.Zero)
                {
                    ShowError("CheatPerkPrize", "Failed to allocate memory for shellcode");
                    return false;
                }

                try
                {
                    memory.WriteArrayMemory(asmAddress, asm);
                    
                    IntPtr thread = CreateRemoteThread(procHandle, IntPtr.Zero, UIntPtr.Zero, asmAddress, IntPtr.Zero, 0, out _);
                    if (thread == IntPtr.Zero || thread == new IntPtr(-1))
                    {
                        ShowError("CheatPerkPrize", "Failed to create remote thread");
                        return false;
                    }

                    try
                    {
                        uint wait = WaitForSingleObject(thread, 5000);
                        if (wait == 0xFFFFFFFF)
                        {
                            ShowError("CheatPerkPrize", "WaitForSingleObject failed");
                            return false;
                        }
                        else if (wait == 0x00000102)
                        {
                            ShowError("CheatPerkPrize", "Thread execution timed out");
                            return false;
                        }
                        
                        return true;
                    }
                    finally
                    {
                        CloseHandle(thread);
                    }
                }
                finally
                {
                    Free(asmAddress);
                }
            }
            catch (Exception ex)
            {
                ShowError("CheatPerkPrize", ex.Message);
                return false;
            }
        }

        public async Task<bool> AddCredits(float value)
        {
            return await CheatPerkPrize(value, EPerkType.Credits);
        }

        public async Task<bool> CheatWheelspins(float value)
        {
            try
            {
                if (_getWheelspinsAddress == UIntPtr.Zero)
                {
                    _getWheelspinsAddress = await SmartAobScan("48 89 ? ? ? 57 48 83 EC ? E8 ? ? ? ? F3 48 ? ? ? 48 8D ? ? ? 48 8B ? ? ? ? ? E8 ? ? ? ? 90 44 8B ? 33 D2");
                }

                if (_getWheelspinsAddress == UIntPtr.Zero)
                {
                    ShowError("CheatWheelspins", "_getWheelspinsAddress == 0");
                    return false;
                }

                byte[] bVal = BitConverter.GetBytes(value);
                byte[] bSpins = BitConverter.GetBytes(_getWheelspinsAddress.ToUInt64());

                byte[] asm = new byte[]
                {
                    0x51, 0x52, 0x41, 0x50, 0x41, 0x51, 0x48, 0x83, 0xEC, 0x28, 0x48, 0x8D, 0x05, 0x4D, 0x00, 0x00, 0x00, 0xC7,
                    0x40, 0x40, bVal[0], bVal[1], bVal[2], bVal[3], 0x48, 0x8D, 0x0D, 0x1F, 0x00, 0x00, 0x00, 0x48, 0x89, 0x41,
                    0x18, 0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, bSpins[0], bSpins[1], bSpins[2], bSpins[3], bSpins[4],
                    bSpins[5], bSpins[6], bSpins[7], 0x48, 0x83, 0xC4, 0x28, 0x41, 0x59, 0x41, 0x58, 0x5A, 0x59, 0xC3
                };

                var memory = Memory.GetInstance();
                IntPtr procHandle = memory.MProc?.Handle ?? IntPtr.Zero;
                
                if (procHandle == IntPtr.Zero)
                {
                    ShowError("CheatWheelspins", "Process handle is null");
                    return false;
                }

                UIntPtr asmAddress = VirtualAllocEx(procHandle, UIntPtr.Zero, (UIntPtr)0x1000, 0x3000, 0x40);
                if (asmAddress == UIntPtr.Zero)
                {
                    ShowError("CheatWheelspins", "_asmAddress == 0");
                    return false;
                }

                memory.WriteArrayMemory(asmAddress, asm);
                IntPtr thread = CreateRemoteThread(procHandle, IntPtr.Zero, UIntPtr.Zero, asmAddress, IntPtr.Zero, 0, out _);
                if (thread == IntPtr.Zero || thread == new IntPtr(-1))
                {
                    ShowError("CheatWheelspins", "thread == 0 || thread == -1");
                    Free(asmAddress);
                    return false;
                }

                uint wait = WaitForSingleObject(thread, int.MaxValue);
                if (wait == 0xFFFFFFFF || wait == 0x00000102)
                {
                    ShowError("CheatWheelspins", "wait == -1 || wait == 0x00000102L");
                    CloseHandle(thread);
                    Free(asmAddress);
                    return false;
                }

                CloseHandle(thread);
                Free(asmAddress);
                return true;
            }
            catch (Exception ex)
            {
                ShowError("CheatWheelspins", ex.Message);
                return false;
            }
        }

        public async Task<bool> CheatSuperWheelspins(float value)
        {
            try
            {
                if (_getSuperWheelspinsAddress == UIntPtr.Zero)
                {
                    _getSuperWheelspinsAddress = await SmartAobScan("48 89 ? ? ? 57 48 83 EC ? E8 ? ? ? ? F3 48 ? ? ? 48 8D ? ? ? 48 8B ? ? ? ? ? E8 ? ? ? ? 90 44 8B ? BA 01");
                }

                if (_getSuperWheelspinsAddress == UIntPtr.Zero)
                {
                    ShowError("CheatSuperWheelspins", "_getSuperWheelspinsAddress == 0");
                    return false;
                }

                byte[] bVal = BitConverter.GetBytes(value);
                byte[] bSpins = BitConverter.GetBytes(_getSuperWheelspinsAddress.ToUInt64());

                byte[] asm = new byte[]
                {
                    0x51, 0x52, 0x41, 0x50, 0x41, 0x51, 0x48, 0x83, 0xEC, 0x28, 0x48, 0x8D, 0x05, 0x4D, 0x00, 0x00, 0x00, 0xC7,
                    0x40, 0x40, bVal[0], bVal[1], bVal[2], bVal[3], 0x48, 0x8D, 0x0D, 0x1F, 0x00, 0x00, 0x00, 0x48, 0x89, 0x41,
                    0x18, 0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, bSpins[0], bSpins[1], bSpins[2], bSpins[3], bSpins[4],
                    bSpins[5], bSpins[6], bSpins[7], 0x48, 0x83, 0xC4, 0x28, 0x41, 0x59, 0x41, 0x58, 0x5A, 0x59, 0xC3
                };

                var memory = Memory.GetInstance();
                IntPtr procHandle = memory.MProc?.Handle ?? IntPtr.Zero;
                
                if (procHandle == IntPtr.Zero)
                {
                    ShowError("CheatSuperWheelspins", "Process handle is null");
                    return false;
                }

                UIntPtr asmAddress = VirtualAllocEx(procHandle, UIntPtr.Zero, (UIntPtr)0x1000, 0x3000, 0x40);
                if (asmAddress == UIntPtr.Zero)
                {
                    ShowError("CheatSuperWheelspins", "_asmAddress == 0");
                    return false;
                }

                memory.WriteArrayMemory(asmAddress, asm);
                IntPtr thread = CreateRemoteThread(procHandle, IntPtr.Zero, UIntPtr.Zero, asmAddress, IntPtr.Zero, 0, out _);
                if (thread == IntPtr.Zero || thread == new IntPtr(-1))
                {
                    ShowError("CheatSuperWheelspins", "thread == 0 || thread == -1");
                    Free(asmAddress);
                    return false;
                }

                uint wait = WaitForSingleObject(thread, int.MaxValue);
                if (wait == 0xFFFFFFFF || wait == 0x00000102)
                {
                    ShowError("CheatSuperWheelspins", "wait == -1 || wait == 0x00000102L");
                    CloseHandle(thread);
                    Free(asmAddress);
                    return false;
                }

                CloseHandle(thread);
                Free(asmAddress);
                return true;
            }
            catch (Exception ex)
            {
                ShowError("CheatSuperWheelspins", ex.Message);
                return false;
            }
        }

        public async Task<bool> CheatXP(float value)
        {
            return await CheatPerkPrize(value, EPerkType.XP);
        }

        public override void Cleanup()
        {
            _getRewardsAddress = UIntPtr.Zero;
            _getPerkPrizeAddress = UIntPtr.Zero;
            _getWheelspinsAddress = UIntPtr.Zero;
            _getSuperWheelspinsAddress = UIntPtr.Zero;

            base.Cleanup();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr VirtualAllocEx(
            IntPtr hProcess,
            UIntPtr lpAddress,
            UIntPtr dwSize,
            uint flAllocationType,
            uint flProtect);

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
        private static extern uint WaitForSingleObject(
            IntPtr hHandle,
            int dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}