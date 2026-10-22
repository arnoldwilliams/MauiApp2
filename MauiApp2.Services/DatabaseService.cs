using System.Collections;
using SQLite;
using MauiApp2.Dto;
using MauiApp2.Database.Models;
using MauiApp2.Interfaces;
using Microsoft.Maui.Storage;
using Newtonsoft.Json;

namespace MauiApp2.Services
{
    public class DatabaseService : IDatabaseService
    {
        public const string DatabaseFilename = "WiltshireMyMobileDB.db3";

        private readonly IFormConfigurationService _formConfigurationService;

        private List<FormConfiguration> _formConfigurations;

        public DatabaseService(IFormConfigurationService formConfigurationService)
        {
            _formConfigurationService = formConfigurationService;

            var tasks = new List<Task>();
            var _formConfigurationTask = Task.Run(() => _formConfigurationService.LoadFormConfigurationsAsync());

            tasks.Add(_formConfigurationTask);

            Task.WhenAll();

            var result = _formConfigurationTask.Result;

            if (!string.IsNullOrEmpty(result))
            {
                _formConfigurations = JsonConvert.DeserializeObject<List<FormConfiguration>>(result);
            }
        }

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create |
            // enable multi-threaded database access
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        SQLiteAsyncConnection db;

        public async Task Init()
        {
            try
            {
                if (db is not null)
                    return;

                db = new SQLiteAsyncConnection(DatabasePath, Flags);

                if (!IsTableExists("MenuModel"))
                    await db.CreateTableAsync<MenuModel>();

                if (!IsTableExists("CaseModel"))
                    await db.CreateTableAsync<CaseModel>();

                if (!IsTableExists("CaseSummaryModel"))
                    await db.CreateTableAsync<CaseSummaryModel>();

                if (!IsTableExists("FormConfigurationModel"))
                    await db.CreateTableAsync<FormConfigurationModel>();

                if (!IsTableExists("BannedExtensionListModel"))
                    await db.CreateTableAsync<BannedExtensionListModel>();

                if (!IsTableExists("LogModel"))
                    await db.CreateTableAsync<LogModel>();

                if (!IsTableExists("BaseMapModel"))
                    await db.CreateTableAsync<BaseMapModel>();
            }
            catch { }
        }

        public bool IsTableExists(string tableName)
        {
            try
            {
                var tableInfo = db.GetConnection().GetTableInfo(tableName);
                if (tableInfo.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> Add(object model)
        {
            await Init();
            var result = await db.InsertAsync(model);
            return result;
        }

        public async Task<int> AddOrReplace(object model)
        {
            await Init();
            var result = await db.InsertOrReplaceAsync(model);
            return result;
        }

        public async Task<int> Update(object model)
        {
            await Init();
            var result = await db.UpdateAsync(model);
            return result;
        }

        public async Task<int> Delete(object model)
        {
            await Init();

            return await db.DeleteAsync(model);
        }

        public async Task<int> DeleteAllMenuModels()
        {
            var recordsDeleted = 0;

            try
            {
                await Init();

                var items = await db.Table<MenuModel>().ToListAsync();

                recordsDeleted = items.Count;

                foreach (var item in items)
                {
                    await db.DeleteAsync(item);
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> DeleteAllCases()
        {
            var recordsDeleted = 0;

            try
            {
                await Init();

                var items = await db.Table<CaseModel>().ToListAsync();

                recordsDeleted = items.Count;

                foreach (var item in items)
                {
                    await db.DeleteAsync(item);
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> DeleteAllCaseSummaries()
        {
            var recordsDeleted = 0;

            try
            {
                await Init();

                var items = await db.Table<CaseSummaryModel>().ToListAsync();

                recordsDeleted = items.Count;

                foreach (var item in items)
                {
                    await db.DeleteAsync(item);
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> DeleteAllFormConfigurations()
        {
            var recordsDeleted = 0;

            try
            {
                await Init();

                var items = await db.Table<FormConfigurationModel>().ToListAsync();

                recordsDeleted = items.Count;

                foreach (var item in items)
                {
                    await db.DeleteAsync(item);
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> DeleteAllBannedExtensions()
        {
            var recordsDeleted = 0;

            try
            {
                await Init();

                var items = await db.Table<BannedExtensionListModel>().ToListAsync();

                recordsDeleted = items.Count;

                foreach (var item in items)
                {
                    await db.DeleteAsync(item);
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> DeleteAllLogs()
        {
            var recordsDeleted = 0;

            try
            {
                await Init();

                var items = await db.Table<LogModel>().ToListAsync();

                recordsDeleted = items.Count;

                foreach (var item in items)
                {
                    await db.DeleteAsync(item);
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> DeleteAllBaseMaps()
        {
            var recordsDeleted = 0;

            try
            {
                await Init();

                var items = await db.Table<BaseMapModel>().ToListAsync();

                recordsDeleted = items.Count;

                foreach (var item in items)
                {
                    await db.DeleteAsync(item);
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> GetMenuModelCount()
        {
            try
            {
                await Init();

                return await db.Table<MenuModel>().CountAsync();
            }
            catch { }

            return 0;
        }

        public async Task<int> GetCaseCount()
        {
            try
            {
                await Init();

                return await db.Table<CaseModel>().CountAsync();
            }
            catch { }

            return 0;
        }

        public async Task<int> GetCaseSummaryCount()
        {
            try
            {
                await Init();

                return await db.Table<CaseSummaryModel>().CountAsync();
            }
            catch { }

            return 0;
        }

        public async Task<int> GetFormConfigurationCount()
        {
            try
            {
                await Init();

                return await db.Table<FormConfigurationModel>().CountAsync();
            }
            catch { }

            return 0;
        }

        public async Task<int> GetBannedExtensionCount()
        {
            try
            {
                await Init();

                return await db.Table<BannedExtensionListModel>().CountAsync();
            }
            catch { }

            return 0;
        }

        public async Task<int> GetLogCount()
        {
            try
            {
                await Init();

                return await db.Table<LogModel>().CountAsync();
            }
            catch { }

            return 0;
        }

        public async Task<int> GetBaseMapCount()
        {
            try
            {
                await Init();

                return await db.Table<BaseMapModel>().CountAsync();
            }
            catch { }

            return 0;
        }

        public async Task<List<MenuModel>?> GetAllMenuModels()
        {
            try
            {
                await Init();

                return await db.Table<MenuModel>().ToListAsync();
            }
            catch { }

            return null;
        }

        public async Task<List<CaseModel>?> GetAllCases()
        {
            try
            {
                await Init();

                return await db.Table<CaseModel>().ToListAsync();
            }
            catch { }

            return null;
        }

        public async Task<List<CaseSummaryModel>?> GetAllCaseSummaries()
        {
            try
            {
                await Init();

                return await db.Table<CaseSummaryModel>().ToListAsync();
            }
            catch { }

            return null;
        }

        public async Task<List<FormConfigurationModel>?> GetAllFormConfigurations()
        {
            try
            {
                await Init();

                return await db.Table<FormConfigurationModel>().ToListAsync();
            }
            catch { }

            return null;
        }

        public async Task<List<BannedExtensionListModel>?> GetAllBannedExtensions()
        {
            try
            {
                await Init();

                return await db.Table<BannedExtensionListModel>().ToListAsync();
            }
            catch { }

            return null;
        }

        public async Task<List<LogModel>?> GetAllLogs()
        {
            try
            {
                await Init();

                return await db.Table<LogModel>().ToListAsync();
            }
            catch { }

            return null;
        }

        public async Task<List<BaseMapModel>?> GetAllBaseMaps()
        {
            try
            {
                await Init();

                return await db.Table<BaseMapModel>().ToListAsync();
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Modification implemented due to generics is iOS ObjectiveC
        /// </summary>
        /// <returns></returns>
        public async Task<List<CaseSummaryModel>> GetDraftCases()
        {
            var results = await db.Table<CaseSummaryModel>().ToListAsync();

            if (results is null || results.Count == 0)
            {
                return new List<CaseSummaryModel>();
            }

            return results;
        }

        public async Task<MenuModel?> GetMenuModelById(string menuId)
        {
            try
            {
                await Init();

                return await db.Table<MenuModel>().FirstOrDefaultAsync(x => x.MenuId.Equals(menuId));
            }
            catch { }

            return null;
        }

        public async Task<CaseModel?> GetCaseById(int id)
        {
            try
            {
                await Init();

                return await db.Table<CaseModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            }
            catch { }

            return null;
        }

        public async Task<CaseSummaryModel?> GetCaseSummaryById(int id)
        {
            try
            {
                await Init();

                return await db.Table<CaseSummaryModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            }
            catch { }

            return null;
        }

        public async Task<FormConfigurationModel?> GetFormConfigurationById(int id)
        {
            try
            {
                await Init();

                return await db.Table<FormConfigurationModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            }
            catch { }

            return null;
        }

        public async Task<BannedExtensionListModel?> GetBannedExtensionById(int id)
        {
            try
            {
                await Init();

                return await db.Table<BannedExtensionListModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            }
            catch { }

            return null;
        }

        public async Task<LogModel?> GetLogById(int id)
        {
            try
            {
                await Init();

                return await db.Table<LogModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            }
            catch { }

            return null;
        }

        public async Task<BaseMapModel?> GetBaseMapById(int id)
        {
            try
            {
                await Init();

                return await db.Table<BaseMapModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
            }
            catch { }

            return null;
        }

        public async Task<FormConfiguration?> GetByFormId(FormConfigurationModel model)
        {
            if (model == null) return null;

            try
            {
                await Init();

                return _formConfigurations?.FirstOrDefault(x => x.PortalFormId == model.FormID);
            }
            catch { }

            return null;
        }

        public FormConfiguration? GetConfigByFormId(FormConfigurationModel model)
        {
            if (model == null) return null;

            try
            {
                Task.Run(async () => await Init());

                //temp bypass

                return _formConfigurations?.FirstOrDefault(x => x.PortalFormId == model.FormID);
            }
            catch { }

            return null;
        }
    }
}
