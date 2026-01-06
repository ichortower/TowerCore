using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ichortower.TowerCore;

public class ConsoleCommands
{
    /*
     * Search for and apply all declared console command handlers in the given class.
     * Like with the Harmony system, this relies on an attribute I defined for this
     * purpose.
     */
    public static bool Register(Type t, out int count)
    {
        MethodInfo[] funcs = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
        count = 0;
        bool ret = true;
        foreach (var func in funcs) {
            if (!TryRegisterCommand(func, out string err)) {
                if (err is not null) {
                    Log.Error($"Console command registry failed ({func.ReflectedType.FullName}.{func.Name}): " +
                            err.ToString());
                    ret = false;
                }
                continue;
            }
            ++count;
        }
        if (!ret) {
            Log.Error("Some console commands failed to register. Please report this to " +
                    "ichortower, along with this SMAPI log.");
        }
        return ret;
    }

    internal static bool TryRegisterCommand(MethodInfo func, out string err)
    {
        err = null;
        ConsoleCommand command = (ConsoleCommand)Attribute.GetCustomAttribute(func,
                typeof(ConsoleCommand));
        // not having the attribute isn't an error
        if (command is null) {
            return false;
        }

        try {
            Main.Helper.ConsoleCommands.Add(command.CommandWord, command.HelpText,
                    (Action<string, string[]>) func.CreateDelegate(typeof(Action<string, string[]>)));
        }
        catch (Exception e) {
            err = e.Message;//$"Method signature not compatible (expected <string, string[]>)";
            return false;
        }
        Log.Trace($"Registered command '{command.CommandWord}' " +
                $"({func.ReflectedType.FullName}.{func.Name})");
        return true;
    }

    /*
     * Call a console command by hooking into SMAPI's command parser and queueing it directly.
     * e.g.:   ichortower.TowerCore.ConsoleCommands.QueueCommand.Value("patch update");
     *
     * Big thanks to Shockah for this. It's wizardry.
     */
    public static Lazy<Action<string>> QueueCommand = new(() => {
        var sCoreType = Type.GetType(
                "StardewModdingAPI.Framework.SCore,StardewModdingAPI")!;
        var commandQueueType = Type.GetType(
                "StardewModdingAPI.Framework.CommandQueue,StardewModdingAPI")!;
        var sCoreGetter = sCoreType.GetProperty("Instance",
                BindingFlags.NonPublic | BindingFlags.Static).GetGetMethod(true);
        var rawCommandQueueField = sCoreType.GetField("RawCommandQueue",
                BindingFlags.NonPublic | BindingFlags.Instance);
        var queueAddMethod = commandQueueType.GetMethod("Add",
                BindingFlags.Public | BindingFlags.Instance);

        var method = new DynamicMethod("QueueConsoleCommand",
                null, new Type[] {typeof(string)});
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Call, sCoreGetter);
        il.Emit(OpCodes.Ldfld, rawCommandQueueField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, queueAddMethod);
        il.Emit(OpCodes.Ret);
        return method.CreateDelegate<Action<string>>();
    });
}

[AttributeUsage(AttributeTargets.Method)]
public class ConsoleCommand : Attribute
{
    public string CommandWord { get; }
    public string HelpText { get; }

    public ConsoleCommand(string commandWord, string helpText) {
        CommandWord = commandWord;
        HelpText = helpText;
    }
}
