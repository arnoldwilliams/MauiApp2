using MauiApp2.Interfaces;

namespace MauiApp2.Services
{
    /// <inheritdoc />
    public class ConfigurationService : IConfigurationService
    {
        /// <inheritdoc />
        public string FormConfigurationUrl { get; set; } = string.Empty;
    }
}
