namespace StandForFH5Revival.Interfaces
{
    public interface ICheatsBase
    {
        bool Initialize();
        void Cleanup();
        string CheatName { get; }
        bool IsActive { get; }
    }
}