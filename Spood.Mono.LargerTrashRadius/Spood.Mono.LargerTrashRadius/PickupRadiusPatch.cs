using HarmonyLib;
using MelonLoader;
using ScheduleOne.ObjectScripts;

namespace Spood.Mono.LargerTrashRadius
{
    [HarmonyPatch(typeof(TrashContainerItem), "Awake")]
    class PickupRadiusPatch
    {
        private static void Prefix(TrashContainerItem __instance)
        {
            // get preferences from file
            var cat = MelonPreferences.GetCategory(Mod.PreferencesCategory);
            var radius = cat.GetEntry<float>(Mod.PreferencesRadiusEntryName).Value;

            // patch trash can radius
            __instance.PickupRadius = radius;
        }
    }
}
