namespace OmniCore.Hybrid.Interfaces
{
    public interface ILayoutStateService
    {
        event Action? OnStateChanged;
        void NotifyStateChanged();
    }
}
