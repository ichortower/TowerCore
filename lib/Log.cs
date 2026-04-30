using StardewModdingAPI;
using System.Diagnostics;

namespace ichortower.TowerCore;

/*
 * Just short aliases for SMAPI's log functions, since I find
 *    Log.Warn("message");
 * much more pleasant to both read and type than
 *    Mod.Monitor.Log("message", LogLevel.Warn);
 *
 * As a bonus, Debug and DebugWarn are conditional on the DEBUG build level, so
 * they get optimized out when compiling for release.
 */
public class Log
{
    public static void Trace(string text) {
        Main.Monitor?.Log(text, LogLevel.Trace);
    }
    [Conditional("DEBUG")]
    public static void Debug(string text) {
        Main.Monitor?.Log(text, LogLevel.Debug);
    }
    [Conditional("DEBUG")]
    public static void DebugWarn(string text) {
        Main.Monitor?.Log(text, LogLevel.Warn);
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
