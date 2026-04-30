using StardewModdingAPI.Events;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ichortower.TowerCore;

public class SmapiEvents
{

    public static bool Register()
    {
        return Register(Assembly.GetExecutingAssembly(), out var _);
    }

    public static bool Register(Assembly assembly, out int count)
    {
        count = 0;
        bool ret = true;
        if (assembly is null) {
            Log.Warn($"SmapiEvents.Register was called with a null assembly, so no work was done.");
            return false;
        }
        foreach (Type type in assembly.GetTypes()) {
            bool allFine = Register(type, out int thisCount, reportOverall: false);
            if (!allFine) {
                ret = false;
            }
            count += thisCount;
        }
        if (!ret) {
            Log.Error("Some SMAPI events failed to register. Please report this to ichortower," +
                    " along with this SMAPI log.");
        }
        return ret;
    }

    internal static List<object> EventClasses = new() {
        Main.Helper.Events.Content,
        Main.Helper.Events.Display,
        Main.Helper.Events.GameLoop,
        Main.Helper.Events.Input,
        Main.Helper.Events.Multiplayer,
        Main.Helper.Events.Player,
        Main.Helper.Events.World,
        Main.Helper.Events.Specialized,
    };

    private static Dictionary<Type, (object, EventInfo)> _Handlers = null;
    internal static Dictionary<Type, (object, EventInfo)> Handlers {
        get {
            _Handlers ??= BuildHandlerDict();
            return _Handlers;
        }
    }

    internal static Dictionary<Type, (object, EventInfo)> BuildHandlerDict()
    {
        Dictionary<Type, (object, EventInfo)> ret = new();
        foreach (object obj in EventClasses) {
            foreach (EventInfo evt in obj.GetType().GetEvents()) {
                Type argsType = evt.EventHandlerType?.GetMethod("Invoke")?
                        .GetParameters()[1]?.ParameterType;
                if (argsType is null) {
                    continue;
                }
                ret[argsType] = (obj, evt);
            }
        }
        return ret;
    }


    public static bool Register(Type type, out int count, bool reportOverall = true)
    {
        MethodInfo[] funcs = type.GetMethods(AllAccess);
        count = 0;
        bool ret = true;
        foreach (MethodInfo func in funcs) {
            SmapiEventAttribute attr = (SmapiEventAttribute)
                    Attribute.GetCustomAttribute(func, typeof(SmapiEventAttribute));
            if (attr is null) {
                continue;
            }
            ParameterInfo[] args = func.GetParameters();
            if (args.Length < 2) {
                Log.Warn($"Method '{func.ReflectedType.FullName}.{func.Name}' was flagged " +
                        "for SMAPI event registry, but did not have enough parameters.");
                ret = false;
                continue;
            }
            // only static methods allowed (see below)
            if (!func.IsStatic) {
                Log.Warn($"Non-static method '{func.ReflectedType.FullName}.{func.Name}' " +
                        "was flagged for SMAPI event registry. Only static methods are supported.");
                ret = false;
                continue;
            }
            Type which = args[1].ParameterType;
            if (!Handlers.TryGetValue(which, out (object obj, EventInfo evt) tuple)) {
                Log.Error($"Unsupported event using args type '{which.Name}'");
                ret = false;
                continue;
            }
            try {
                // This particular version of CreateDelegate won't bind to an instance method;
                // hence the try/catch (although we early exit on non-statics above).
                // So why use it instead of one that allows instance methods? When registering
                // event handlers this way, we don't have access to a class instance to provide.
                // We could allow it anyway, with `null` as the instance, but that invites
                // NREs when invoking, since `this` would be null. I think it's better to
                // prevent non-statics.
                var deleg = func.CreateDelegate(typeof(EventHandler<>).MakeGenericType(which));
                tuple.evt.AddEventHandler(tuple.obj, deleg);
                Log.Debug($"Registered handler '{func.ReflectedType.FullName}.{func.Name}'" +
                        $" for event '{tuple.evt.Name}'");
                ++count;
            }
            catch (Exception e) {
                Log.Error(e.ToString());
                ret = false;
            }
        }
        if (reportOverall && !ret) {
            Log.Error("Some SMAPI events failed to register. Please report this to ichortower," +
                    " along with this SMAPI log.");
        }
        return ret;
    }


    internal static BindingFlags AllAccess = BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

}

[AttributeUsage(AttributeTargets.Method)]
public class SmapiEventAttribute : Attribute
{
}
