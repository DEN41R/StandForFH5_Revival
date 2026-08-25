using System;
using System.Threading.Tasks;

namespace StandForFH5Revival
{
    public class Bypass : CheatsUtilities
    {
        public UIntPtr CallAddress;
        private bool m_applied;
        private bool m_scanning;
        private static readonly object s_Lock = new object();

        public override string CheatName => "CRC Bypass";

        public async Task DisableCrcChecks()
        {
            lock (s_Lock)
            {
                if (m_scanning || m_applied)
                {
                    return;
                }

                m_scanning = true;
            }

            try
            {
                const string sig = "4C 3B ? 0F 95 ? 0F 94";
                var callAddress = await SmartAobScan(sig);
                
                if (callAddress == UIntPtr.Zero)
                {
                    throw new InvalidOperationException($"CRC bypass address not found with signature: {sig}");
                }

                lock (s_Lock)
                {
                    CallAddress = callAddress;
                    byte[] patch = { 0x48, 0x39, 0xFF };
                    var memory = Memory.GetInstance();
                    memory.WriteArrayMemory(CallAddress, patch);
                    m_scanning = false;
                    m_applied = true;
                }
            }
            catch
            {
                lock (s_Lock)
                {
                    m_scanning = false;
                }
                throw;
            }
        }

        public override void Cleanup()
        {
            try
            {
                lock (s_Lock)
                {
                    if (CallAddress != UIntPtr.Zero && m_applied)
                    {
                        byte[] orig = { 0x4C, 0x3B, 0xEF };
                        var memory = Memory.GetInstance();
                        memory.WriteArrayMemory(CallAddress, orig);
                    }
                }

                Reset();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during Bypass cleanup: {ex.Message}");
            }

            base.Cleanup();
        }

        public void Reset()
        {
            lock (s_Lock)
            {
                m_scanning = false;
                m_applied = false;
                CallAddress = UIntPtr.Zero;
            }
        }
    }
}