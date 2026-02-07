// Assets/Editor/DialogueImporterMultiEditor.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueImporterMultiEditor : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Resources/DialogueData";
    private bool overwriteExisting = true;

    [UnityEditor.MenuItem("Tools/Dialogue/Import CSV → Multi DialogueData")]
    public static void ShowWindow()
    {
        GetWindow<DialogueImporterMultiEditor>("Dialogue Importer (Multi)");
    }

    void OnGUI()
    {
        GUILayout.Label("📘 匯入多章節對話 (CSV)", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV 檔案 (UTF-8)", csvFile, typeof(TextAsset), false);
        outputFolder = EditorGUILayout.TextField("輸出資料夾", outputFolder);
        overwriteExisting = EditorGUILayout.Toggle("覆蓋既有檔案", overwriteExisting);

        if (GUILayout.Button("🚀 匯入 CSV 並生成多個 DialogueData"))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("錯誤", "請先選擇 CSV 檔案(TextAsset)", "OK");
                return;
            }
            ImportCSVMulti();
        }
    }

    void ImportCSVMulti()
    {
        string[] rawLines = csvFile.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        if (rawLines.Length <= 1)
        {
            Debug.LogError("CSV 內容不足或格式錯誤。");
            return;
        }

        // parse header -> find indices
        string headerLine = rawLines[0];
        List<string> headers = ParseCSVLine(headerLine);
        int idxChapter = headers.IndexOf("Chapter");
        int idxID = headers.IndexOf("ID");
        int idxName = headers.IndexOf("CharacterName");
        int idxPortrait = headers.IndexOf("Portrait");
        int idxText = headers.IndexOf("Text");
        int idxSide = headers.IndexOf("Side");
        int idxVoice = headers.IndexOf("Voice");

        if (idxChapter < 0 || idxName < 0 || idxText < 0)
        {
            EditorUtility.DisplayDialog("錯誤", "CSV 必須包含 Chapter, CharacterName, Text 等欄位。", "OK");
            return;
        }

        // group by chapter
        Dictionary<string, List<string>> chapterRows = new Dictionary<string, List<string>>();

        for (int i = 1; i < rawLines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(rawLines[i])) continue;
            List<string> cols = ParseCSVLine(rawLines[i]);
            if (cols.Count == 0) continue;

            string chapter = idxChapter >= 0 && idxChapter < cols.Count ? cols[idxChapter] : "Default";
            if (string.IsNullOrWhiteSpace(chapter)) chapter = "Default";

            if (!chapterRows.ContainsKey(chapter)) chapterRows[chapter] = new List<string>();
            chapterRows[chapter].Add(rawLines[i]);
        }

        // ensure output folder
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        int createdCount = 0;
        foreach (var kv in chapterRows)
        {
            string chapterName = SanitizeFileName(kv.Key);
            string assetPath = $"{outputFolder}/Dialogue_{chapterName}.asset";

            // build DialogueLine list
            List<DialogueLine> lines = new List<DialogueLine>();

            foreach (string row in kv.Value)
            {
                List<string> cols = ParseCSVLine(row);
                // safe read
                string name = GetCol(cols, idxName);
                string portraitName = GetCol(cols, idxPortrait);
                string text = GetCol(cols, idxText);
                string side = GetCol(cols, idxSide);
                string voiceName = GetCol(cols, idxVoice);
                string boxName = GetCol(cols, headers.IndexOf("DialogueBox"));

                DialogueLine line = new DialogueLine();
                line.characterName = name;
                line.text = text;
                line.isLeftSide = (side.Trim().ToLower() == "left" || side.Trim().ToLower() == "l");

                if (!string.IsNullOrEmpty(portraitName))
                    line.portrait = Resources.Load<Sprite>($"Sprites/Portraits/{portraitName}");

                if (!string.IsNullOrEmpty(voiceName))
                    line.voiceClip = Resources.Load<AudioClip>($"Audio/Voices/{voiceName}");

                if (!string.IsNullOrEmpty(boxName))
                    line.dialogueBox = Resources.Load<Sprite>($"Sprites/DialogueBoxes/{boxName}"); // ✅ 新增

                lines.Add(line);
            }

            // create or overwrite asset
            DialogueData dialogueData;
            if (File.Exists(assetPath) && !overwriteExisting)
            {
                dialogueData = AssetDatabase.LoadAssetAtPath<DialogueData>(assetPath);
                if (dialogueData == null) dialogueData = ScriptableObject.CreateInstance<DialogueData>();
            }
            else
            {
                dialogueData = ScriptableObject.CreateInstance<DialogueData>();
            }

            dialogueData.lines = lines.ToArray();
            AssetDatabase.CreateAsset(dialogueData, assetPath);
            // If asset existed and was loaded, we need to copy data then Save
            EditorUtility.SetDirty(dialogueData);

            createdCount++;
            Debug.Log($"✅ 產生章節: {chapterName} -> {assetPath} (lines: {lines.Count})");
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("完成", $"匯入完成，共產生 {createdCount} 個 DialogueData 資產。", "OK");
    }

    // robust CSV line parser (supports quoted fields with commas)
    List<string> ParseCSVLine(string line)
    {
        List<string> cols = new List<string>();
        if (string.IsNullOrEmpty(line)) return cols;

        // regex: either "quoted string" or unquoted token (no commas)
        Regex csvRegex = new Regex("(?:^|,)(?:(?:\"([^\"]*(?:\"\"[^\"]*)*)\")|([^\",]*))", RegexOptions.Compiled);
        MatchCollection matches = csvRegex.Matches(line);
        foreach (Match m in matches)
        {
            string quoted = m.Groups[1].Value;
            string unquoted = m.Groups[2].Value;
            string val = quoted != "" ? quoted.Replace("\"\"", "\"") : unquoted;
            cols.Add(val);
        }
        return cols;
    }

    string GetCol(List<string> cols, int idx)
    {
        if (idx < 0 || idx >= cols.Count) return "";
        return cols[idx].Trim();
    }

    string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        name = name.Replace(" ", "_");
        return name;
    }
}
