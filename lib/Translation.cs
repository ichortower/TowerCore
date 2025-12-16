namespace ichortower.TowerCore;

public class Translation
{
    /*
     * Just a more-convenient alias for the i18n API endpoint. Typically:
     *     using TR = ichortower.TowerCore.Translation;
     *     string foo = TR.Get("foo");
     */
    public static string Get(string key) {
        return Main.Helper.Translation.Get(key);
    }
}
