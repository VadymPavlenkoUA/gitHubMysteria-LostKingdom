public class SaveSlotInfo
{
    public int slot;
    public bool exists;
    public string saveTime;
    public string path;

    public SaveSlotInfo(int slot, bool exists, string saveTime, string path)
    {
        this.slot = slot;
        this.exists = exists;
        this.saveTime = saveTime;
        this.path = path;
    }
}
