using MelonLoader;

namespace Spood.Mono.CartelRebalance;

internal static class CartelInfluenceConfig
{
    private const string CategoryName = "Spood_CartelRebalance";

    private const string RemoveCartelGraffitiPlayerKey = "remove_cartel_graffiti_player_delta";
    private const string RemoveCartelGraffitiNpcInterruptedKey = "remove_cartel_graffiti_npc_interrupted_delta";
    private const string AmbushClearedKey = "ambush_cleared_delta";
    private const string CartelDealerDefeatedKey = "cartel_dealer_defeated_delta";
    private const string NewCustomerUnlockedKey = "new_customer_unlocked_delta";

    private static MelonPreferences_Category _category = null!;

    private static MelonPreferences_Entry<float> _removeCartelGraffitiPlayerDelta = null!;
    private static MelonPreferences_Entry<float> _removeCartelGraffitiNpcInterruptedDelta = null!;
    private static MelonPreferences_Entry<float> _ambushClearedDelta = null!;
    private static MelonPreferences_Entry<float> _cartelDealerDefeatedDelta = null!;
    private static MelonPreferences_Entry<float> _newCustomerUnlockedDelta = null!;

    public static float RemoveCartelGraffitiPlayerDelta => _removeCartelGraffitiPlayerDelta.Value;
    public static float RemoveCartelGraffitiNpcInterruptedDelta => _removeCartelGraffitiNpcInterruptedDelta.Value;
    public static float AmbushClearedDelta => _ambushClearedDelta.Value;
    public static float CartelDealerDefeatedDelta => _cartelDealerDefeatedDelta.Value;
    public static float NewCustomerUnlockedDelta => _newCustomerUnlockedDelta.Value;

    public static void Initialize()
    {
        _category = MelonPreferences.CreateCategory(CategoryName);

        _removeCartelGraffitiPlayerDelta = GetOrCreateEntry(RemoveCartelGraffitiPlayerKey, -0.05f);
        _removeCartelGraffitiNpcInterruptedDelta = GetOrCreateEntry(RemoveCartelGraffitiNpcInterruptedKey, -0.1f);
        _ambushClearedDelta = GetOrCreateEntry(AmbushClearedKey, -0.1f);
        _cartelDealerDefeatedDelta = GetOrCreateEntry(CartelDealerDefeatedKey, -0.1f);
        _newCustomerUnlockedDelta = GetOrCreateEntry(NewCustomerUnlockedKey, -0.075f);

        _category.SaveToFile();
    }

    private static MelonPreferences_Entry<float> GetOrCreateEntry(string entryName, float defaultValue)
    {
        var existing = _category.GetEntry<float>(entryName);
        if (existing != null)
        {
            return existing;
        }

        return _category.CreateEntry(entryName, defaultValue);
    }
}
