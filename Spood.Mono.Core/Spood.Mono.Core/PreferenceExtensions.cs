using MelonLoader;

namespace Spood.Mono.Core;

public static class PreferenceExtensions
{
    public static void AddEntry<T>(this MelonPreferences_Category category, string name, T defaultValue, string description)
    {
        category.SetOverwrite($"{name}Description", description);
        category.SetDefault(name, defaultValue);
    }

    public static void SetEntry<T>(this MelonPreferences_Category category, string name, T defaultValue, string description)
    {
        category.AddEntry(name, defaultValue, description);
    }

    public static void SetDefault<T>(this MelonPreferences_Category category, string name, T defaultValue)
    {
        if (category.GetEntry(name) == null)
        {
            category.CreateEntry<T>(name, defaultValue);
        }
    }

    public static void SetOverwrite<T>(this MelonPreferences_Category category, string name, T value)
    {
        var entry = category.GetEntry(name);
        if (entry == null)
        {
            category.CreateEntry<T>(name, value);
            return;
        }

        category.DeleteEntry(name);
        category.CreateEntry<T>(name, value);
    }
}
