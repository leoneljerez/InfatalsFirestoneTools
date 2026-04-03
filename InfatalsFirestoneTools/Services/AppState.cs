namespace InfatalsFirestoneTools.Services
{
    public class AppState
    {
        public event Action? OnProfileChanged;

        public void NotifyProfileChanged()
        {
            OnProfileChanged?.Invoke();
        }
    }
}
