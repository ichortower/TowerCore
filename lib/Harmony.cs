#if TOWERCORE_USING_HARMONY
using HarmonyLib;
using System;
using System.Reflection;

namespace ichortower.TowerCore;

public class Patches
{
    /*
     * Search for and apply all declared Harmony patches in the given class. This isn't really
     * a good idea, since Harmony already has attributes and a PatchAll function, but I have an
     * unfounded hope that doing this will let me avoid the SMAPI rewriting problems, and it
     * should get me a few features (like being able to continue if one patch fails).
     *
     * I implemented my own attributes for this, since Harmony's are slightly awkward for my
     * intended setup. As a result, don't expect this to work for other mods (or other authors).
     * 
     * Returns true if no problems were encountered (all found patches were applied successfully,
     * or no patches were found so no work could be done), or false if at least one patch failed
     * to apply. In either case, `out int count` will be set to the number of patches that
     * succeeded.
     */
    public static bool Apply(Type t, out int count)
    {
        MethodInfo[] funcs = t.GetMethods(BindingFlags.Static | BindingFlags.Public);
        count = 0;
        bool ret = true;
        foreach (var func in funcs) {
            if (!TryApplyPatch(func, out string err)) {
                if (err is not null) {
                    Log.Error($"Patch failed ({func.ReflectedType.FullName}.{func.Name}): " +
                            err.ToString());
                    ret = false;
                }
                continue;
            }
            ++count;
        }
        if (!ret) {
            Log.Error("Some Harmony patches failed to apply. Please report this to ichortower," +
                    " along with this SMAPI log.");
        }
        return ret;
    }

    internal static bool TryApplyPatch(MethodInfo func, out string err)
    {
        err = null;
        TargetMethod[] targets = (TargetMethod[])Attribute.GetCustomAttributes(
                func, typeof(TargetMethod));
        // not having any targets just means it isn't a patch, which is fine
        if (targets.Length == 0) {
            return false;
        }
        PatchType patchType = (PatchType)Attribute.GetCustomAttribute(func, typeof(PatchType));
        if (patchType is null) {
            err = $"missing PatchType attribute";
            return false;
        }

        bool ret = true;
        MethodInfo orig;
        foreach (TargetMethod attr in targets) {
            try {
                // TODO method type
                if ((attr.ArgumentTypes?.Length ?? 0) > 0) {
                    orig = attr.TargetType.GetMethod(attr.MethodName,
                            AllAccess, null, attr.ArgumentTypes, null);
                }
                else {
                    orig = attr.TargetType.GetMethod(attr.MethodName, AllAccess);
                }
                if (orig is null) {
                    err = $"TargetMethod not found: {attr.TargetType}.{attr.MethodName}";
                    ret = false;
                    continue;
                }
                HarmonyMethod patch = new(func.ReflectedType, func.Name);
                if (patchType.Value == PatchTypes.Prefix) {
                    Harmony.Patch(original: orig, prefix: patch);
                }
                else if (patchType.Value == PatchTypes.Postfix) {
                    Harmony.Patch(original: orig, postfix: patch);
                }
                else if (patchType.Value == PatchTypes.Transpiler) {
                    Harmony.Patch(original: orig, transpiler: patch);
                }
                else if (patchType.Value == PatchTypes.Finalizer) {
                    Harmony.Patch(original: orig, finalizer: patch);
                }
                Log.Trace($"Patched '{orig.ReflectedType.FullName}.{orig.Name}'" +
                        $" ({patchType.Value.ToString()})");
            }
            catch (Exception e) {
                err = e.ToString();
                ret = false;
                continue;
            }
        }
        return ret;
    }

    internal static BindingFlags AllAccess = BindingFlags.Static | BindingFlags.Instance |
            BindingFlags.Public | BindingFlags.NonPublic;

    internal static Harmony Harmony {
        get {
            _Harmony ??= new Harmony(Main.ModId);
            return _Harmony;
        }
    }
    private static Harmony _Harmony = null;
}


public enum PatchTypes {
    Prefix,
    Postfix,
    Transpiler,
    Finalizer,
}


/*
 * Attribute the first: TargetMethod.
 * Use this one to declare a method to target with a patch. Use it more than once to patch
 * multiple methods.
 * Each instance should be fully defined, so this has fewer constructors/overloads than
 * Harmony's HarmonyPatch.
 */
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class TargetMethod : Attribute
{
    public Type TargetType { get; }
    public string MethodName { get; }
    // TODO MethodType proxy for constructors/getters/etc.
    public Type[] ArgumentTypes { get; }

    public TargetMethod(Type type, string methodName) {
        TargetType = type;
        MethodName = methodName;
    }

    public TargetMethod(Type type, string methodName, Type[] argumentTypes) {
        TargetType = type;
        MethodName = methodName;
        ArgumentTypes = argumentTypes;
    }
}

/*
 * Attribute the second: PatchType.
 * Use this to set the type of patch to apply: one of Prefix, Postfix, Transpiler, or Finalizer.
 */
[AttributeUsage(AttributeTargets.Method)]
public class PatchType : Attribute
{
    public PatchTypes Value { get; }

    public PatchType(PatchTypes type) {
        Value = type;
    }
}

#endif // TOWERCORE_USING_HARMONY
