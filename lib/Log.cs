using StardewModdingAPI;

namespace ichortower.TowerCore;

public class Log
{
    public static void Trace(string text) {
        Main.Monitor?.Log(text, LogLevel.Trace);
    }
    public static void Debug(string text) {
        Main.Monitor?.Log(text, LogLevel.Debug);
    }
    public static void Info(string text) {
        Main.Monitor?.Log(text, LogLevel.Info);
    }
    public static void Warn(string text) {
        Main.Monitor?.Log(text, LogLevel.Warn);
    }
    public static void Error(string text) {
        Main.Monitor?.Log(text, LogLevel.Error);
    }
    public static void Alert(string text) {
        Main.Monitor?.Log(text, LogLevel.Alert);
    }
    public static void Verbose(string text) {
        Main.Monitor?.VerboseLog(text);
    }
}
