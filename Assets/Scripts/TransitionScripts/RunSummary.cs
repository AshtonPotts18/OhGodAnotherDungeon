using System.IO;
using UnityEngine;

public class RunSummary
{
    public string playerName;
    public int level;
    public float elapsedSeconds;
    public int goldCollected;
}
public static class RunSummarySave
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "playerData.json");

    public static void Save(RunSummary summary)
    {
        string json = JsonUtility.ToJson(summary, true);
        File.WriteAllText(SavePath, json);
    }
    public static RunSummary Load()
    {
        if (!File.Exists(SavePath))
            return new RunSummary();

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<RunSummary>(json);
    }
}
