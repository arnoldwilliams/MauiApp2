using MauiApp2.Interfaces;
using Microsoft.Maui.Storage;

namespace MauiApp2.Services
{
    /// <inheritdoc />
    public class PreferencesService : IPreferencesService
    {
        /// <inheritdoc />
        public string GetPreference(string key, string defaultValue)
        {
            return Preferences.Default.Get(key, defaultValue);
        }

        /// <inheritdoc />
        public void SetPreference(string key, string value)
        {
            Preferences.Default.Set(key, value);
        }
    }
}
