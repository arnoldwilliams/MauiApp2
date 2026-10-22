namespace MauiApp2.Interfaces
{
    /// <summary>
    /// Contract for application configuration values.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the endpoint used to retrieve form configurations.
        /// </summary>
        string FormConfigurationUrl { get; }
    }
}
