using System.Collections.Generic;
using MauiApp2.Dto;
using MauiApp2.Interfaces;

namespace MauiApp2.Services
{
    /// <inheritdoc />
    public class StorageService : IStorageService
    {
        /// <inheritdoc />
        public List<FormConfiguration> GetFormConfigurations()
        {
            return new List<FormConfiguration>();
        }
    }
}
