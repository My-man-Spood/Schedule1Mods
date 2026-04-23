using System;
using MelonLoader;
using Spood.Mono.Core;

namespace Spood.Mono.CartelRebalance;

internal static class CartelInfluenceConfig
{
    private const string CategoryName = "Spood_CartelRebalance";

    private const string RemoveCartelGraffitiPlayer = nameof(RemoveCartelGraffitiPlayer);
    private const string RemoveCartelGraffitiNpcInterrupted = nameof(RemoveCartelGraffitiNpcInterrupted);
    private const string AmbushCleared = nameof(AmbushCleared);
    private const string CartelDealerDefeated = nameof(CartelDealerDefeated);
    private const string NewCustomerUnlocked = nameof(NewCustomerUnlocked);

    private static MelonPreferences_Category _category = null!;

    private static MelonPreferences_Entry<int> _removeCartelGraffitiPlayerPoints = null!;
    private static MelonPreferences_Entry<int> _removeCartelGraffitiNpcInterruptedPoints = null!;
    private static MelonPreferences_Entry<int> _ambushClearedPoints = null!;
    private static MelonPreferences_Entry<int> _cartelDealerDefeatedPoints = null!;
    private static MelonPreferences_Entry<int> _newCustomerUnlockedPoints = null!;

    public static float RemoveCartelGraffitiPlayerDelta => ConvertPointsToDelta(_removeCartelGraffitiPlayerPoints.Value);
    public static float RemoveCartelGraffitiNpcInterruptedDelta => ConvertPointsToDelta(_removeCartelGraffitiNpcInterruptedPoints.Value);
    public static float AmbushClearedDelta => ConvertPointsToDelta(_ambushClearedPoints.Value);
    public static float CartelDealerDefeatedDelta => ConvertPointsToDelta(_cartelDealerDefeatedPoints.Value);
    public static float NewCustomerUnlockedDelta => ConvertPointsToDelta(_newCustomerUnlockedPoints.Value);

    public static void Initialize()
    {
        _category = MelonPreferences.CreateCategory(CategoryName);

        _category.SetEntry(
            RemoveCartelGraffitiPlayer,
            50,
            "Influence points (out of 1000) removed when you (the player) remove cartel graffiti.");
        _category.SetEntry(
            RemoveCartelGraffitiNpcInterrupted,
            100,
            "Influence points (out of 1000) removed when you interrupt an NPC doing cartel graffiti.");
        _category.SetEntry(
            AmbushCleared,
            100,
            "Influence points (out of 1000) removed when an ambush is defeated/cleared.");
        _category.SetEntry(
            CartelDealerDefeated,
            100,
            "Influence points (out of 1000) removed when a cartel dealer is defeated.");
        _category.SetEntry(
            NewCustomerUnlocked,
            50,
            "Influence points (out of 1000) removed when a new customer is unlocked while the cartel is hostile.");

        _removeCartelGraffitiPlayerPoints = _category.GetEntry<int>(RemoveCartelGraffitiPlayer)!;
        _removeCartelGraffitiNpcInterruptedPoints = _category.GetEntry<int>(RemoveCartelGraffitiNpcInterrupted)!;
        _ambushClearedPoints = _category.GetEntry<int>(AmbushCleared)!;
        _cartelDealerDefeatedPoints = _category.GetEntry<int>(CartelDealerDefeated)!;
        _newCustomerUnlockedPoints = _category.GetEntry<int>(NewCustomerUnlocked)!;

        _category.SaveToFile();
    }

    private static float ConvertPointsToDelta(int points)
    {
        return -(points / 1000f);
    }
}
