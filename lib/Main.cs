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
     * It is the individual mod's responsibility to set this reference. i.e.:
     *
     * public override void Entry(IModHelper helper) {
     *     ichortower.TowerCore.Main.Mod = this;
     *     ...
     * }
     *
     * You may experience NREs if you fail to do this before using the lib.
     */
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
