using StardewValley;

namespace ichortower.TowerCore;

public class Game
{
    public static bool IsActive() {
        return Game1.game1.IsActive || ((!Game1.options?.pauseWhenOutOfFocus) ?? false);
    }
}
