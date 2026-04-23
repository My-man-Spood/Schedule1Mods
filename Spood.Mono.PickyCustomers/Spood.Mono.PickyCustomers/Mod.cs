using System.Diagnostics;
using System.Globalization;
using AeLa.EasyFeedback.APIs;
using MelonLoader;
using Spood.Mono.Core;

[assembly: MelonInfo(typeof(Spood.Mono.PickyCustomers.Mod), "Spood.Mono.PickyCustomers", "1.0.0", "Spood", null)]
[assembly: MelonGame("TVGS", "Schedule I")]
namespace Spood.Mono.PickyCustomers;

public class Mod : MelonMod
{
    public const string PreferencesCategory = "Spood_PickyCustomers";
    public const string PropertyPenaltyPercent = nameof(PropertyPenaltyPercent);
    public const string PropertyBonusPercent = nameof(PropertyBonusPercent);
    public const string DrugTypePenaltyPercent = nameof(DrugTypePenaltyPercent);
    public const string QualityPenaltyPercent = nameof(QualityPenaltyPercent);
    public const string QualityBonusPercent = nameof(QualityBonusPercent);
    public const string RedColor = "\x1B[255;0;0m";
    public const string GreenColor = "\x1B[0;255;0m";
    public override void OnInitializeMelon()
    {
        SetupPreferences();
        LoggerInstance.Msg("Initialized.");
    }

    private void SetupPreferences()
    {
        var cat = MelonPreferences.CreateCategory(PreferencesCategory);
        cat.AddEntry(PropertyPenaltyPercent, 0.4f, "How much to penalize enjoyment if the product is missing a property the customer wants (0 = no penalty, 1 = maximum penalty).");
        cat.AddEntry(PropertyBonusPercent, 0.1f, "Adds extra enjoyment for each additional liked property on the product, beyond what the customer requires (applies per extra property, usually up to 2). (0 = no bonus, 1 = maximum bonus).");
        cat.AddEntry(DrugTypePenaltyPercent, 0.3f, "How much to penalize enjoyment if the product is a drug type the customer dislikes (0 = no penalty, 1 = maximum penalty).");
        cat.AddEntry(QualityPenaltyPercent, 0.3f, "How much to penalize enjoyment if the product quality is lower than the customer expects (0 = no penalty, 1 = maximum penalty).");
        cat.AddEntry(QualityBonusPercent, 0.1f, "Adds extra enjoyment for each quality tier the product is above what the customer expects (0 = no bonus, 1 = maximum bonus).");

        cat.SaveToFile();
    }
}