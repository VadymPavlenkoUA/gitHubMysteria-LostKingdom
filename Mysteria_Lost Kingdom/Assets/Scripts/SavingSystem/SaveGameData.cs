using System;
using System.Collections.Generic;

[Serializable]
public class SaveGameData
{
    public int saveVersion = 1;
    public string saveTime;
    public Dictionary<string, object> savedObjects = new();
}

[Serializable]
public class SaveGameWrapper
{
    public int saveVersion;
    public string saveTime;
    public List<SaveObjectEntry> objects = new();
}

[Serializable]
public class SaveObjectEntry
{
    public string id;
    public string json;
    public string type;
}
