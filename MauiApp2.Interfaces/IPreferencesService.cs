namespace MauiApp2.Interfaces
{
    /// <summary>
    /// Contract for reading and writing application preferences.
    /// </summary>
    public interface IPreferencesService
    {
        /// <summary>
        /// Gets the preference value for the supplied key.
        /// </summary>
        /// <param name="key">The preference key.</param>
        /// <param name="defaultValue">The value returned when the key does not exist.</param>
        /// <returns>The preference value.</returns>
        string GetPreference(string key, string defaultValue);

        /// <summary>
        /// Sets the preference value for the supplied key.
        /// </summary>
        /// <param name="key">The preference key.</param>
        /// <param name="value">The value to store.</param>
        void SetPreference(string key, string value);
    }
}
