using MelonLoader;
using Spood.Mono.Core;

[assembly: MelonInfo(typeof(Spood.Mono.LargerTrashRadius.Mod), "Spood.Mono.LargerTrashRadius", "1.1.0", "Spood", null)]
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
    }

    private void SetupPreferences()
    {
        var cat = MelonPreferences.CreateCategory(PreferencesCategory);
        cat.SetDefault(PreferencesRadiusEntryName, 5.5f);
        cat.SaveToFile();
    }
}