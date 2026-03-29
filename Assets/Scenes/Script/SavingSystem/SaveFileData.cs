using System;
using System.Collections.Generic;

[Serializable]
public class SaveRecord
{
    public string uniqueID;
    public string jsonData;
}

[Serializable]
public class SaveFileData
{
    public string saveTime;
    public string sceneName;
    public bool isAutoSave;
    public int slotIndex;
    public List<SaveRecord> records = new List<SaveRecord>();
}