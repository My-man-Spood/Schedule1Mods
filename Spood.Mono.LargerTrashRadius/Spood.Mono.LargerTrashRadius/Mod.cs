using MelonLoader;

[assembly: MelonInfo(typeof(Spood.Mono.LargerTrashRadius.Mod), "Spood.Mono.LargerTrashRadius", "1.0.0", "Spood", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
namespace Spood.Mono.LargerTrashRadius;

public class Mod : MelonMod
{
    public const string PreferencesCategory = "Spood_LargerTrashRadius";
    public const string PreferencesRadiusEntryName = "radius";
    
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Spood.Mono.LargerTrashRadius Initialized");
        SetupPreferences();
        HarmonyInstance.PatchAll();
    }

    private void SetupPreferences()
    {
        var cat = MelonPreferences.CreateCategory(PreferencesCategory);
        if(cat.GetEntry(PreferencesRadiusEntryName) == null)
        {
            cat.CreateEntry<float>(PreferencesRadiusEntryName, 5.5f);
            cat.SaveToFile();
        }
    }
}