using System;

namespace StandForFH5Revival.Interfaces
{
    public interface IRevertBase
    {
        bool Revert();
        bool CanRevert { get; }
        bool IsReverted { get; }
        void StoreOriginalBytes(UIntPtr address, byte[] originalBytes);
    }
}