using System;
using System.Collections.Generic;
using System.Text;
using MauiApp2.Dto;

namespace MauiApp2.Services
{
    /// <summary>
    /// Contract for Form congiguration servcie
    /// </summary>
    public interface IFormConfigurationService
    {
        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>The configuration.</returns>
        /// <param name="configurationType">Configuration type.</param>
        Task<FormConfiguration> GetConfigurationAsync(string configurationType);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <returns>The configuration.</returns>
        /// <param name="configurationType">Configuration type.</param>
        FormConfiguration GetConfiguration(string configurationType);

        /// <summary>
        /// Loads the form configurations async.
        /// </summary>
        /// <returns>The form configurations async.</returns>
        Task<string> LoadFormConfigurationsAsync();
    }

}
