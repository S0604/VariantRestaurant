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
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("自動存檔")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f;

    [Header("手動存檔槽數量")]
    [SerializeField] private int manualSlotCount = 3;

    private const string AUTO_SAVE_FILE = "autosave.json";

    private Coroutine autoSaveCoroutine;
    private SaveFileData pendingLoadData;
    private bool isWaitingSceneLoad = false;

    public int ManualSlotCount => manualSlotCount;

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

            if (enableAutoSave && !isWaitingSceneLoad)
            {
                SaveAuto();
            }
        }
    }

    public void SaveSlot(int slotIndex)
    {
        if (!IsValidManualSlot(slotIndex))
        {
            Debug.LogWarning($"[SaveManager] 無效的手動存檔槽位: {slotIndex}");
            return;
        }

        string fileName = GetManualSlotFileName(slotIndex);
        SaveToFile(fileName, false, slotIndex);
    }

    public void LoadSlot(int slotIndex)
    {
        if (!IsValidManualSlot(slotIndex))
        {
            Debug.LogWarning($"[SaveManager] 無效的手動讀檔槽位: {slotIndex}");
            return;
        }

        string fileName = GetManualSlotFileName(slotIndex);
        LoadFromFile(fileName);
    }

    public void SaveAuto()
    {
        SaveToFile(AUTO_SAVE_FILE, true, 0);
    }

    public void LoadAuto()
    {
        LoadFromFile(AUTO_SAVE_FILE);
    }

    public bool HasManualSave(int slotIndex)
    {
        if (!IsValidManualSlot(slotIndex))
            return false;

        return File.Exists(GetFullPath(GetManualSlotFileName(slotIndex)));
    }

    public bool HasAutoSave()
    {
        return File.Exists(GetFullPath(AUTO_SAVE_FILE));
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
            displayName = $"存檔槽 {slotIndex}",
            fileName = GetManualSlotFileName(slotIndex),
            fullPath = GetFullPath(GetManualSlotFileName(slotIndex)),
            saveTime = "空槽位",
            sceneName = "-"
        };

        if (!File.Exists(meta.fullPath))
            return meta;

        SaveFileData data = ReadSaveFile(meta.fullPath);
        if (data != null)
        {
            meta.hasData = true;
            meta.saveTime = string.IsNullOrEmpty(data.saveTime) ? "未知時間" : data.saveTime;
            meta.sceneName = string.IsNullOrEmpty(data.sceneName) ? "-" : data.sceneName;
        }

        return meta;
    }

    public SaveSlotMetaData GetAutoSaveMetaData()
    {
        SaveSlotMetaData meta = new SaveSlotMetaData
        {
            slotIndex = 0,
            isAutoSave = true,
            hasData = false,
            displayName = "自動存檔",
            fileName = AUTO_SAVE_FILE,
            fullPath = GetFullPath(AUTO_SAVE_FILE),
            saveTime = "無自動存檔",
            sceneName = "-"
        };

        if (!File.Exists(meta.fullPath))
            return meta;

        SaveFileData data = ReadSaveFile(meta.fullPath);
        if (data != null)
        {
            meta.hasData = true;
            meta.saveTime = string.IsNullOrEmpty(data.saveTime) ? "未知時間" : data.saveTime;
            meta.sceneName = string.IsNullOrEmpty(data.sceneName) ? "-" : data.sceneName;
        }

        return meta;
    }

    public List<SaveSlotMetaData> GetAllLoadableSlotsSorted(bool includeAutoSave = true)
    {
        List<SaveSlotMetaData> list = new List<SaveSlotMetaData>();

        if (includeAutoSave)
        {
            list.Add(GetAutoSaveMetaData());
        }

        for (int i = 1; i <= manualSlotCount; i++)
        {
            list.Add(GetManualSlotMetaData(i));
        }

        list.Sort((a, b) =>
        {
            DateTime ta = ParseTime(a.saveTime);
            DateTime tb = ParseTime(b.saveTime);
            return tb.CompareTo(ta);
        });

        return list;
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

    private void SaveToFile(string fileName, bool isAutoSave, int slotIndex)
    {
        SaveFileData fileData = new SaveFileData
        {
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            sceneName = SceneManager.GetActiveScene().name,
            isAutoSave = isAutoSave,
            slotIndex = slotIndex
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
                Debug.LogWarning($"[SaveManager] 發現空的 UniqueID，已略過: {saveable}");
                continue;
            }

            if (usedIDs.Contains(id))
            {
                Debug.LogWarning($"[SaveManager] 發現重複 UniqueID: {id}，後面的資料已略過");
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
        Debug.Log($"[SaveManager] 存檔完成: {path}");
    }

    private void LoadFromFile(string fileName)
    {
        string path = GetFullPath(fileName);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] 找不到存檔檔案: {path}");
            return;
        }

        SaveFileData fileData = ReadSaveFile(path);

        if (fileData == null)
        {
            Debug.LogError("[SaveManager] 讀檔失敗，JSON 解析為 null");
            return;
        }

        if (string.IsNullOrEmpty(fileData.sceneName))
        {
            Debug.LogWarning("[SaveManager] 存檔中沒有 sceneName，將直接嘗試還原目前場景物件");
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
            Debug.LogWarning("[SaveManager] ApplyLoadedData 收到 null");
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
                Debug.LogWarning($"[SaveManager] 場景中有重複 UniqueID: {id}");
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

        Debug.Log($"[SaveManager] 讀檔完成，場景: {fileData.sceneName}，時間: {fileData.saveTime}");
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
        return $"save_slot_{slotIndex}.json";
    }

    private string GetFullPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}