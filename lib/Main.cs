using StardewModdingAPI;

namespace ichortower.TowerCore;

/*
 * A bucket for holding references to useful things:
 *     - IMod reference, if you need it
 *     - mod unique id
 *     - mod.Helper
 *     - mod.Monitor
 *     - mod.ModManifest
 */
public class Main
{
    /*
     * It is the client's responsibility to call this function. i.e.:
     *
     * public override void Entry(IModHelper helper) {
     *     ichortower.TowerCore.Main.Init(this);
     * }
     *
     * This will both set the mod reference (critical for most support functions
     * to work) and call the assorted attribute-based registry functions. If you
     * aren't using some of those features and you are very concerned about
     * performance, you can manually do just the ones you need:
     *
     * public override void Entry(IModHelper helper) {
     *     ichortower.TowerCore.Main.Mod = this;
     *     ichortower.TowerCore.HarmonyPatches.Apply();
     * }
     *
     */
    public static void Init(IMod modref)
    {
        Mod = modref;
        SmapiEvents.Register();
        //ConsoleCommands.Register();
        HarmonyPatches.Apply();
    }


    public static IMod Mod = null;

    public static IModHelper Helper {
        get {
            _Helper ??= Mod.Helper;
            return _Helper;
        }
    }

    public static IMonitor Monitor {
        get {
            _Monitor ??= Mod.Monitor;
            return _Monitor;
        }
    }

    public static IManifest Manifest {
        get {
            _Manifest ??= Mod.ModManifest;
            return _Manifest;
        }
    }

    public static string ModId {
        get {
            return Manifest.UniqueID;
        }
    }

    private static IModHelper _Helper = null;
    private static IMonitor _Monitor = null;
    private static IManifest _Manifest = null;
}
