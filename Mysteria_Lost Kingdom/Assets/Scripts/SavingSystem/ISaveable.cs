public interface ISaveable
{
    string GetSaveID();
    object CaptureState();
    void RestoreState(object state);
}
