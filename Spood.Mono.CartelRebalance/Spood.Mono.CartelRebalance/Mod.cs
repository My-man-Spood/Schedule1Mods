using MelonLoader;

[assembly: MelonInfo(typeof(Spood.Mono.CartelRebalance.Mod), "Spood.Mono.CartelRebalance", "1.0.0", "Spood", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
namespace Spood.Mono.CartelRebalance;

public class Mod : MelonMod
{
    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Spood.Mono.CartelRebalance Initialized");
        CartelInfluenceConfig.Initialize();
    }
}
