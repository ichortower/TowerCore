using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#if TOWERCORE_USING_NEWTONSOFT
using Newtonsoft.Json.Linq;
using System.IO;
#endif

namespace ichortower.TowerCore;

public class Compatibility
{
    /*
     * Attempt to parse a token from a loaded Content Patcher pack and return
     * it as a string.
     * This is generally not ideal and involves multiple reflection crimes, but
     * you do what you have to do.
     */
    public static string GetCPToken(string modId, string key)
    {
        try {
            object lpb = Activator.CreateInstance(LogPathBuilderType, new object[] {"none"});
            // DoNotWrapExceptions here lets us catch different types
            object context = GetContextFor.Invoke(TokenManagerRef,
                    BindingFlags.DoNotWrapExceptions, null, new object[] {modId}, null);
            object tokstr = Activator.CreateInstance(TokenStringType, new object[] {"{{" + key + "}}", context, lpb});
            return tokstr.ToString();
        }
        // this can be thrown by GetContextFor when CP hasn't finished initializing
        // (e.g. in GameLaunched). ignore and return null; in our use case it will
        // be refreshed later, so it's harmless
        catch (KeyNotFoundException) {
        }
        catch (Exception e) {
            Log.Warn(e.ToString());
        }
        return null;
    }


#if TOWERCORE_USING_NEWTONSOFT

    /*
     * Parse a config.json from another mod.
     * Returns true if the mod is installed and its config was successfully
     * parsed, and false otherwise.
     *
     * NOTE this relies on knowing the standard location for the file and uses
     * System APIs to read it, which may become disallowed in the future.
     * But there's no official channel for this, and SMAPI mods and content
     * packs have different stuff to reflect into (and it's gnarly), so this way
     * remains for now.
     */
    internal static bool TryGetConfig(string modId, out JObject config)
    {
        config = null;
        var modInfo = Main.Helper.ModRegistry.Get(modId);
        if (modInfo is null) {
            return false;
        }
        try {
            string baseDir = modInfo.GetType().GetProperty("DirectoryPath")
                    .GetValue(modInfo) as string;
            config = JObject.Parse(File.ReadAllText(Path.Combine(baseDir, "config.json")));
            return true;
        }
        catch (FileNotFoundException) {
        }
        catch (Exception e) {
            Log.Warn($"Caught while trying to read config for {modId}: {e}");
        }
        return false;
    }

#endif // TOWERCORE_USING_NEWTONSOFT


    /*
     * This is the implementation stuff that drives GetCPToken.
     */
    private static Mod _cpModRef = null;
    private static Type _tokenStringType = null;
    private static Type _logPathBuilderType = null;
    private static MethodInfo _getContextFor = null;

    private static object TokenManagerRef = null;

    private static Mod CpModRef {
        get {
            if (_cpModRef is null) {
                var mi = Main.Helper.ModRegistry.Get("Pathoschild.ContentPatcher");
                _cpModRef = mi.GetType().GetProperty("Mod",
                        BindingFlags.Public | BindingFlags.Instance)
                        .GetValue(mi) as Mod;
            }
            return _cpModRef;
        }
    }

    private static Type TokenStringType {
        get {
            _tokenStringType ??= CpModRef.GetType().Assembly.GetType(
                    "ContentPatcher.Framework.Conditions.TokenString");
            return _tokenStringType;
        }
    }

    private static Type LogPathBuilderType {
        get {
            _logPathBuilderType ??= CpModRef.GetType().Assembly.GetType(
                    "ContentPatcher.Framework.LogPathBuilder");
            return _logPathBuilderType;
        }
    }

    private static MethodInfo GetContextFor {
        get {
            if (_getContextFor is null) {
                object screenManField = CpModRef.GetType().GetField("ScreenManager",
                        BindingFlags.NonPublic | BindingFlags.Instance).GetValue(CpModRef);
                object screenManValue = screenManField.GetType().GetProperty("Value",
                        BindingFlags.Public | BindingFlags.Instance).GetValue(screenManField);
                TokenManagerRef = screenManValue.GetType().GetProperty("TokenManager",
                        BindingFlags.Public | BindingFlags.Instance).GetValue(screenManValue);
                _getContextFor = TokenManagerRef.GetType().GetMethod("GetContextFor",
                        BindingFlags.Public | BindingFlags.Instance);
            }
            return _getContextFor;
        }
    }
}
