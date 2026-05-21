using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveSlotMetaData
{
    public int slotIndex;
    public bool isAutoSave;
    public bool hasData;
    public string saveTime;
    public string sceneName;
    public string displayName;
    public string fileName;
    public string fullPath;
    public string fileID;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("自動存檔")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f;
    [SerializeField] private int maxAutoSaveCount = 10;

    [Header("手動存檔槽數")]
    [SerializeField] private int manualSlotCount = 3;

    private const string MANUAL_FILE_PREFIX = "save_slot_";
    private const string AUTO_FILE_PREFIX = "autosave_";
    private const string FILE_EXTENSION = ".json";

    private Coroutine autoSaveCoroutine;
    private SaveFileData pendingLoadData;
    private bool isWaitingSceneLoad = false;

    public int ManualSlotCount => manualSlotCount;
    public int MaxAutoSaveCount => maxAutoSaveCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Start()
    {
        if (enableAutoSave)
        {
            StartAutoSave();
        }
    }

    public void StartAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
        }

        autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
    }

    public void StopAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(autoSaveInterval);

            if (enableAutoSave && !isWaitingSceneLoad && CanSaveNow())
            {
                SaveAuto();
            }
        }
    }

    public bool CanSaveNow()
    {
        if (FreeModeToggleManager.Instance == null)
            return true;

        return !FreeModeToggleManager.Instance.IsBusinessMode;
    }

    public bool CanLoadNow()
    {
        return true;
    }

    public string GetSaveBlockedReason()
    {
        if (FreeModeToggleManager.Instance != null && FreeModeToggleManager.Instance.IsBusinessMode)
        {
            return "Àç·~¼Ò¦¡¤¤¤£¥i¦sÀÉ";
        }

        return string.Empty;
    }

    public void SaveSlot(int slotIndex)
    {
        if (!IsValidManualSlot(slotIndex))
        {
            Debug.LogWarning($"[SaveManager] µL®Äªº¤â°Ê¦sÀÉ¼Ñ¦ì: {slotIndex}");
            return;
        }

        if (!CanSaveNow())
        {
            Debug.Log("[SaveManager] ¥Ø«e¬°Àç·~¼Ò¦¡¡A¸T¤î¤â°Ê¦sÀÉ");
            return;
        }

        string fileName = GetManualSlotFileName(slotIndex);
        SaveToFile(fileName, false, slotIndex);
    }

    public void LoadSlot(int slotIndex)
    {
        if (!IsValidManualSlot(slotIndex))
        {
            Debug.LogWarning($"[SaveManager] µL®Äªº¤â°ÊÅªÀÉ¼Ñ¦ì: {slotIndex}");
            return;
        }

        string fileName = GetManualSlotFileName(slotIndex);
        LoadFromFile(fileName);
    }

    public void SaveAuto()
    {
        if (!CanSaveNow())
        {
            Debug.Log("[SaveManager] ¥Ø«e¬°Àç·~¼Ò¦¡¡A¸T¤î¦Û°Ê¦sÀÉ");
            return;
        }

        string fileID = GenerateAutoSaveFileID();
        string fileName = fileID + FILE_EXTENSION;

        SaveToFile(fileName, true, 0, fileID);
        CleanupOldAutoSaves();
    }

    public void LoadAutoByFileName(string fileName)
    {
        LoadFromFile(fileName);
    }

    public bool HasManualSave(int slotIndex)
    {
        if (!IsValidManualSlot(slotIndex))
            return false;

        return File.Exists(GetFullPath(GetManualSlotFileName(slotIndex)));
    }

    public bool HasAnyLoadableData()
    {
        for (int i = 1; i <= manualSlotCount; i++)
        {
            if (HasManualSave(i))
                return true;
        }

        return GetAutoSaveMetaDataList().Count > 0;
    }

    public string GetSavePathInfo()
    {
        return Application.persistentDataPath;
    }

    public SaveSlotMetaData GetManualSlotMetaData(int slotIndex)
    {
        SaveSlotMetaData meta = new SaveSlotMetaData
        {
            slotIndex = slotIndex,
            isAutoSave = false,
            hasData = false,
            displayName = $"¤â°Ê¦sÀÉ {slotIndex}",
            fileName = GetManualSlotFileName(slotIndex),
            fullPath = GetFullPath(GetManualSlotFileName(slotIndex)),
            saveTime = "ªÅ¼Ñ¦ì",
            sceneName = "-",
            fileID = $"manual_{slotIndex}"
        };

        if (!File.Exists(meta.fullPath))
            return meta;

        SaveFileData data = ReadSaveFile(meta.fullPath);
        if (data != null)
        {
            meta.hasData = true;
            meta.saveTime = string.IsNullOrEmpty(data.saveTime) ? "¥¼ª¾®É¶¡" : data.saveTime;
            meta.sceneName = string.IsNullOrEmpty(data.sceneName) ? "-" : data.sceneName;
            meta.fileID = string.IsNullOrEmpty(data.fileID) ? $"manual_{slotIndex}" : data.fileID;
        }

        return meta;
    }

    public List<SaveSlotMetaData> GetAutoSaveMetaDataList()
    {
        List<SaveSlotMetaData> list = new List<SaveSlotMetaData>();

        if (!Directory.Exists(Application.persistentDataPath))
            return list;

        string[] files = Directory.GetFiles(Application.persistentDataPath, $"{AUTO_FILE_PREFIX}*{FILE_EXTENSION}");

        foreach (string fullPath in files)
        {
            SaveFileData data = ReadSaveFile(fullPath);

            SaveSlotMetaData meta = new SaveSlotMetaData
            {
                slotIndex = 0,
                isAutoSave = true,
                hasData = data != null,
                displayName = "¦Û°Ê¦sÀÉ",
                fileName = Path.GetFileName(fullPath),
                fullPath = fullPath,
                saveTime = data != null && !string.IsNullOrEmpty(data.saveTime) ? data.saveTime : "¥¼ª¾®É¶¡",
                sceneName = data != null && !string.IsNullOrEmpty(data.sceneName) ? data.sceneName : "-",
                fileID = data != null && !string.IsNullOrEmpty(data.fileID) ? data.fileID : Path.GetFileNameWithoutExtension(fullPath)
            };

            list.Add(meta);
        }

        list.Sort((a, b) =>
        {
            DateTime ta = ParseTime(a.saveTime);
            DateTime tb = ParseTime(b.saveTime);
            return tb.CompareTo(ta);
        });

        return list;
    }

    public List<SaveSlotMetaData> GetAllLoadableSlotsSorted(bool includeAutoSaves = true)
    {
        List<SaveSlotMetaData> list = new List<SaveSlotMetaData>();

        // ¤â°Ê¦sÀÉ©T©w¶¶§Ç 1 -> 2 -> 3
        for (int i = 1; i <= manualSlotCount; i++)
        {
            list.Add(GetManualSlotMetaData(i));
        }

        // ¦Û°Ê¦sÀÉ¥t¥~¨Ì®É¶¡±Æ§Ç«á±µ¦b«á­±
        if (includeAutoSaves)
        {
            list.AddRange(GetAutoSaveMetaDataList());
        }

        return list;
    }

    public void DeleteAutoSaveByFileName(string fileName)
    {
        string fullPath = GetFullPath(fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log($"[SaveManager] ¤w§R°£¦Û°Ê¦sÀÉ: {fileName}");
        }
    }

    private void CleanupOldAutoSaves()
    {
        List<SaveSlotMetaData> autoSaves = GetAutoSaveMetaDataList();

        if (maxAutoSaveCount <= 0)
        {
            foreach (var save in autoSaves)
            {
                if (File.Exists(save.fullPath))
                {
                    File.Delete(save.fullPath);
                }
            }
            return;
        }

        if (autoSaves.Count <= maxAutoSaveCount)
            return;

        for (int i = maxAutoSaveCount; i < autoSaves.Count; i++)
        {
            if (File.Exists(autoSaves[i].fullPath))
            {
                File.Delete(autoSaves[i].fullPath);
                Debug.Log($"[SaveManager] ¦Û°Ê§R°£³ÌÂÂ¦Û°Ê¦sÀÉ: {autoSaves[i].fileName}");
            }
        }
    }

    private string GenerateAutoSaveFileID()
    {
        return $"{AUTO_FILE_PREFIX}{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private DateTime ParseTime(string timeString)
    {
        if (DateTime.TryParse(timeString, out DateTime result))
            return result;

        return DateTime.MinValue;
    }

    private bool IsValidManualSlot(int slotIndex)
    {
        return slotIndex >= 1 && slotIndex <= manualSlotCount;
    }

    private void SaveToFile(string fileName, bool isAutoSave, int slotIndex, string fileID = "")
    {
        SaveFileData fileData = new SaveFileData
        {
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            sceneName = SceneManager.GetActiveScene().name,
            isAutoSave = isAutoSave,
            slotIndex = slotIndex,
            fileID = string.IsNullOrEmpty(fileID) ? Path.GetFileNameWithoutExtension(fileName) : fileID
        };

        ISaveable[] saveables = FindAllSaveables();
        HashSet<string> usedIDs = new HashSet<string>();

        foreach (ISaveable saveable in saveables)
        {
            if (saveable == null)
                continue;

            string id = saveable.GetUniqueID();

            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[SaveManager] 發現空的 UniqueID，已跳過: {saveable}");
                continue;
            }

            if (usedIDs.Contains(id))
            {
                Debug.LogWarning($"[SaveManager] 發現重複 UniqueID: {id}，後面的資料已跳過");
                continue;
            }
            usedIDs.Add(id);

            SaveRecord record = new SaveRecord
            {
                uniqueID = id,
                jsonData = saveable.CaptureAsJson()
            };

            fileData.records.Add(record);
        }

        string json = JsonUtility.ToJson(fileData, true);
        string path = GetFullPath(fileName);

        File.WriteAllText(path, json);
        Debug.Log($"[SaveManager] ¦sÀÉ§¹¦¨: {path}");
    }

    private void LoadFromFile(string fileName)
    {
        string path = GetFullPath(fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] §ä¤£¨ì¦sÀÉÀÉ®×: {path}");
            return;
        }

        SaveFileData fileData = ReadSaveFile(path);

        if (fileData == null)
        {
            Debug.LogError("[SaveManager] ÅªÀÉ¥¢±Ñ¡AJSON ¸ÑªR¬° null");
            return;
        }

        if (string.IsNullOrEmpty(fileData.sceneName))
        {
            Debug.LogWarning("[SaveManager] ¦sÀÉ¤¤¨S¦³ sceneName¡A±Nª½±µ¹Á¸ÕÁÙ­ì¥Ø«e³õ´ºª«¥ó");
            ApplyLoadedData(fileData);
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == fileData.sceneName)
        {
            ApplyLoadedData(fileData);
        }
        else
        {
            pendingLoadData = fileData;
            isWaitingSceneLoad = true;
            SceneManager.LoadScene(fileData.sceneName);
        }
    }

    private SaveFileData ReadSaveFile(string fullPath)
    {
        if (!File.Exists(fullPath))
            return null;

        string json = File.ReadAllText(fullPath);

        if (string.IsNullOrEmpty(json))
            return null;

        return JsonUtility.FromJson<SaveFileData>(json);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isWaitingSceneLoad || pendingLoadData == null)
            return;

        if (scene.name != pendingLoadData.sceneName)
            return;

        ApplyLoadedData(pendingLoadData);
        pendingLoadData = null;
        isWaitingSceneLoad = false;
    }

    private void ApplyLoadedData(SaveFileData fileData)
    {
        if (fileData == null)
        {
            Debug.LogWarning("[SaveManager] ApplyLoadedData ¦¬¨ì null");
            return;
        }

        ISaveable[] saveables = FindAllSaveables();
        Dictionary<string, ISaveable> saveableDict = new Dictionary<string, ISaveable>();

        foreach (ISaveable saveable in saveables)
        {
            if (saveable == null)
                continue;

            string id = saveable.GetUniqueID();

            if (string.IsNullOrEmpty(id))
                continue;

            if (!saveableDict.ContainsKey(id))
            {
                saveableDict.Add(id, saveable);
            }
            else
            {
                Debug.LogWarning($"[SaveManager] ³õ´º¤¤¦³­«½Æ UniqueID: {id}");
            }
        }

        foreach (SaveRecord record in fileData.records)
        {
            if (record == null || string.IsNullOrEmpty(record.uniqueID))
                continue;

            if (saveableDict.TryGetValue(record.uniqueID, out ISaveable saveable))
            {
                saveable.RestoreFromJson(record.jsonData);
            }
        }

        Debug.Log($"[SaveManager] ÅªÀÉ§¹¦¨¡A³õ´º: {fileData.sceneName}¡A®É¶¡: {fileData.saveTime}");
    }

    private ISaveable[] FindAllSaveables()
    {
        MonoBehaviour[] allBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        List<ISaveable> results = new List<ISaveable>();

        foreach (MonoBehaviour mono in allBehaviours)
        {
            if (mono is ISaveable saveable)
            {
                results.Add(saveable);
            }
        }

        return results.ToArray();
    }

    private string GetManualSlotFileName(int slotIndex)
    {
        return $"{MANUAL_FILE_PREFIX}{slotIndex}{FILE_EXTENSION}";
    }

    private string GetFullPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public void DeleteAllSaveFiles()
    {
        // §R°£¤â°Ê¦sÀÉ
        for (int i = 1; i <= manualSlotCount; i++)
        {
            string manualPath = GetFullPath(GetManualSlotFileName(i));
            if (File.Exists(manualPath))
            {
                File.Delete(manualPath);
                Debug.Log($"[SaveManager] ¤w§R°£¤â°Ê¦sÀÉ: {Path.GetFileName(manualPath)}");
            }
        }

        // §R°£©Ò¦³¦Û°Ê¦sÀÉ
        if (Directory.Exists(Application.persistentDataPath))
        {
            string[] autoFiles = Directory.GetFiles(Application.persistentDataPath, $"{AUTO_FILE_PREFIX}*{FILE_EXTENSION}");

            foreach (string autoPath in autoFiles)
            {
                if (File.Exists(autoPath))
                {
                    File.Delete(autoPath);
                    Debug.Log($"[SaveManager] ¤w§R°£¦Û°Ê¦sÀÉ: {Path.GetFileName(autoPath)}");
                }
            }
        }

        Debug.Log("[SaveManager] ©Ò¦³¦sÀÉ¸ê®Æ¤w§R°£");
    }

    public bool HasAnySaveFiles()
    {
        for (int i = 1; i <= manualSlotCount; i++)
        {
            if (File.Exists(GetFullPath(GetManualSlotFileName(i))))
                return true;
        }

        if (Directory.Exists(Application.persistentDataPath))
        {
            string[] autoFiles = Directory.GetFiles(Application.persistentDataPath, $"{AUTO_FILE_PREFIX}*{FILE_EXTENSION}");
            if (autoFiles.Length > 0)
                return true;
        }

        return false;
    }
}