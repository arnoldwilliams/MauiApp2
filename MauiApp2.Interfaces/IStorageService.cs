using System.Collections.Generic;
using MauiApp2.Dto;

namespace MauiApp2.Interfaces
{
    /// <summary>
    /// Contract for local storage of application data.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets the locally cached form configurations.
        /// </summary>
        /// <returns>The cached form configurations.</returns>
        List<FormConfiguration> GetFormConfigurations();
    }
}
