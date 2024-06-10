using UnityEngine;
using System.IO;

public class CustomLogger : MonoBehaviour
{
    private string logFilePath;

    void Awake()
    {
        // Define the log file path relative to the executable
        string directoryPath = Path.Combine(Application.dataPath, "../logs");
        Directory.CreateDirectory(directoryPath);
        logFilePath = Path.Combine(directoryPath, "game_log.txt");

        // Subscribe to the log message received event
        Application.logMessageReceived += HandleLog;
    }

    void OnDestroy()
    {
        // Unsubscribe from the log message received event
        Application.logMessageReceived -= HandleLog;
    }

    // Handle log messages
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string logEntry = $"{System.DateTime.Now} [{type}] {logString}\n{stackTrace}\n";
        File.AppendAllText(logFilePath, logEntry);
    }
}
