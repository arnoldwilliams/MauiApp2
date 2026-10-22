using MauiApp2.Dto;
using MauiApp2.Interfaces;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace MauiApp2.Services
{
    public class FormConfigurationService : IFormConfigurationService
    {
        /// <summary>
        /// Last Synced Date time.
        /// </summary>
        private const string LastSyncedFormConfigurationDateTime = "LAST_SYNC_FRM_CNFG_DT";
        public const string FormConfigs = "FormConfigs";
        public const string FormConfigsAppVersion = "FormConfigsAppVersion";

        /// <summary>
        /// The Communication Service.
        /// </summary>
        private readonly IHttpCommunication communicationService;

        /// <summary>
        /// The configuration service.
        /// </summary>
        private readonly IConfigurationService configurationService;

        /// <summary>
        /// The Preference Service.
        /// </summary>
        private readonly IPreferencesService preferencesService;

        private readonly IStorageService _storageService;

        /// <summary>
        /// List of Form Configuration.
        /// </summary>
        private List<FormConfiguration> formConfigurations = new List<FormConfiguration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FormConfigurationService"/> class.
        /// </summary>
        /// <param name="communicationService">The communication service.</param>
        /// <param name="storageService">The storage service.</param>
        /// <param name="preferencesService">The Preference service.</param>
        public FormConfigurationService(IHttpCommunication communicationService,
                                        IPreferencesService preferencesService, IConfigurationService configurationService, IStorageService storageService)
        {
            this.communicationService = communicationService;
            this.preferencesService = preferencesService;
            this.configurationService = configurationService;
            _storageService = storageService;

            // Preload from local storage in launch of application
            formConfigurations = _storageService.GetFormConfigurations();
        }

        /// <summary>
        /// Gets the configuration text.
        /// </summary>
        /// <param name="configurationType">The configuration identifier from PortalFormType constants.</param>
        /// <returns>Form configuration object</returns>
        public async Task<FormConfiguration> GetConfigurationAsync(string configurationType)
        {
            // todo
            await Task.Delay(100);
            if (this.formConfigurations.Count == 0)
            {
                //this.formConfigurations = await this._StorageService.GetFormConfigurations().ConfigureAwait(true);

            }

            return formConfigurations.FirstOrDefault(item => item.PortalFormId.Equals(configurationType)) ?? new();
        }

        /// <summary>
        /// Gets the configuration text.
        /// </summary>
        /// <param name="configurationType">The configuration identifier from PortalFormType constants.</param>
        /// <returns>Form configuration object</returns>
        public FormConfiguration GetConfiguration(string configurationType)
        {
            if (formConfigurations.Count == 0)
            {
                //this.formConfigurations = await this._StorageService.GetFormConfigurations().ConfigureAwait(true);

            }

            return formConfigurations?.FirstOrDefault(item => item.PortalFormId.Equals(configurationType)) ?? new();
        }

        /// <summary>
        /// Loads all Form Configurations.
        /// </summary>
        /// <returns>Success or Failure</returns>
        public async Task<string> LoadFormConfigurationsAsync()
        {
            // get the last sync date time and get the delta form configurations
            var lastSyncedDateTimeString = preferencesService.GetPreference(LastSyncedFormConfigurationDateTime, new DateTime(2000, 01, 01).ToString("o")); // todo

            string currentAppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})";

            var result = preferencesService.GetPreference(FormConfigs, string.Empty);
            var configsAppVersion = preferencesService.GetPreference(FormConfigsAppVersion, string.Empty);

            if (string.IsNullOrEmpty(result) || currentAppVersion != configsAppVersion)
            {
                result = await this.communicationService.GetStringAsync($"{configurationService.FormConfigurationUrl}/{lastSyncedDateTimeString}");
                preferencesService.SetPreference(FormConfigs, result);
                preferencesService.SetPreference(FormConfigsAppVersion, currentAppVersion);
            }

            return result;
        }
    }

}
