
using System;

namespace SkyeMusicCompanion.Services;

public static class Log
{
    public static string LogPath => Path.Combine(FileSystem.AppDataDirectory, "companion_log.txt");

    public static string GetLogPath() => LogPath;
    public static void Write(string message)
    {
        var line =
            $"[{DateTime.Now:yyyyMMdd HHmmss}]{Environment.NewLine}" + $"{message}{Environment.NewLine}{Environment.NewLine}";
        try
        {
            File.AppendAllText(LogPath, line);
            System.Diagnostics.Debug.WriteLine("LOG WRITE OK: " + message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("LOG ERROR: " + ex.Message);
        }
    }
    public static string ReadAll()
    {
        if (!File.Exists(LogPath))
            return "(log file not found)";

        return File.ReadAllText(LogPath);
    }
    public static void Clear()
    {
        try
        {
            if (File.Exists(LogPath))
                File.WriteAllText(LogPath, string.Empty);

            System.Diagnostics.Debug.WriteLine("LOG CLEARED");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("LOG CLEAR ERROR: " + ex.Message);
        }
    }
}
