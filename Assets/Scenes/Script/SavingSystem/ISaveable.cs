using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable
{
    string GetUniqueID();
    string CaptureAsJson();
    void RestoreFromJson(string json);
}