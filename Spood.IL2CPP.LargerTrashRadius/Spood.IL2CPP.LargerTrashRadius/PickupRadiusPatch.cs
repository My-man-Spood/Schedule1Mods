using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne.ObjectScripts;
using MelonLoader;
using System.Reflection;

namespace Spood.IL2CPP.LargerTrashRadius;

[HarmonyPatch(typeof(TrashContainerItem), "Awake")]
public class PickupRadiusPatch
{
    static void Prefix(Il2CppObjectBase __instance)
    {
        // get preferences from file
        var cat = MelonPreferences.GetCategory(Mod.PreferencesCategory);
        var radius = cat.GetEntry<float>(Mod.PreferencesRadiusEntryName).Value;

        // patch trash can radius
        var propertyInfo = __instance.GetType().GetProperty("PickupRadius", BindingFlags.Public | BindingFlags.Instance);
        propertyInfo.SetValue(__instance, radius);
    }
}
