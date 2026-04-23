using MelonLoader;

namespace Spood.Mono.PickyCustomers;

public static class PreferenceExtensions
{
    public static void AddEntry<T>(this MelonPreferences_Category cat, string name, T defaultValue, string description)
    {
        cat.SetOverwrite($"{name}Description", description);
        cat.SetDefault(name, defaultValue);

    }
    /// <summary>
    /// Sets the default value for a preference entry if it does not already exist.
    /// Will not overwrite an existing entry.
    /// </summary>
    public static void SetDefault<T>(this MelonPreferences_Category cat, string name, T defaultValue)
    {
        if (cat.GetEntry(name) == null)
        {
            cat.CreateEntry<T>(name, defaultValue);
        }
    }

    /// <summary>
    /// Set a value for a preference entry. Will overwrite an existing entry.
    /// </summary>
    public static void SetOverwrite<T>(this MelonPreferences_Category cat, string name, T value)
    {
        var entry = cat.GetEntry(name);
        if (entry == null)
        {
            cat.CreateEntry<T>(name, value);
        }
        else
        {
            cat.DeleteEntry(name);
            cat.CreateEntry<T>(name, value);
        }
    }
}