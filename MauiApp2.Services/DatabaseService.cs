using System;
using SQLite;
using MauiApp2.Dto;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;
using MauiApp2.Database.Models;
using MauiApp2.Interfaces;
using Newtonsoft.Json;
using Microsoft.Maui.Storage;

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

        public async Task<int> DeleteAll(object model)
        {
            int recordsDeleted = 0;

            try
            {

                if (model == null)
                    return -1;

                await Init();

                Type tp = model.GetType();

                if (tp.Equals(typeof(MenuModel)))
                {
                    var items = await db.Table<MenuModel>().ToListAsync();

                    recordsDeleted = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (tp.Equals(typeof(CaseModel)))
                {
                    var items = await db.Table<CaseModel>().ToListAsync();

                    recordsDeleted = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (tp.Equals(typeof(CaseSummaryModel)))
                {
                    var items = await db.Table<CaseSummaryModel>().ToListAsync();

                    recordsDeleted = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (tp.Equals(typeof(FormConfigurationModel)))
                {
                    var items = await db.Table<FormConfigurationModel>().ToListAsync();

                    recordsDeleted = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (tp.Equals(typeof(BannedExtensionListModel)))
                {
                    var items = await db.Table<BannedExtensionListModel>().ToListAsync();

                    recordsDeleted = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (tp.Equals(typeof(LogModel)))
                {
                    var items = await db.Table<LogModel>().ToListAsync();

                    recordsDeleted = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (tp.Equals(typeof(BaseMapModel)))
                {
                    var items = await db.Table<BaseMapModel>().ToListAsync();

                    recordsDeleted = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }
            }
            catch { }

            return recordsDeleted;
        }

        public async Task<int> DeleteAllAsync<T>()
        {
            int result = 0;

            try
            {
                await Init();

                if (typeof(T) == typeof(MenuModel))
                {
                    var items = await db.Table<MenuModel>().ToListAsync();

                    result = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (typeof(T) == typeof(CaseModel))
                {
                    var items = await db.Table<CaseModel>().ToListAsync();

                    result = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (typeof(T) == typeof(CaseSummaryModel))
                {
                    var items = await db.Table<CaseSummaryModel>().ToListAsync();

                    result = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (typeof(T) == typeof(FormConfigurationModel))
                {
                    var items = await db.Table<FormConfigurationModel>().ToListAsync();

                    result = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (typeof(T) == typeof(BannedExtensionListModel))
                {
                    var items = await db.Table<BannedExtensionListModel>().ToListAsync();

                    result = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (typeof(T) == typeof(LogModel))
                {
                    var items = await db.Table<LogModel>().ToListAsync();

                    result = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }

                if (typeof(T) == typeof(BaseMapModel))
                {
                    var items = await db.Table<BaseMapModel>().ToListAsync();

                    result = items.Count;

                    foreach (var item in items)
                    {
                        await db.DeleteAsync(item);
                    }
                }
            }
            catch { }

            return result;
        }

        public async Task<int> GetCount(object model)
        {
            if (model == null) return -1;

            try
            {

                await Init();

                Type tp = model.GetType();

                if (tp.Equals(typeof(MenuModel)))
                {
                    return await db.Table<MenuModel>().CountAsync();
                }

                if (tp.Equals(typeof(CaseModel)))
                {
                    return await db.Table<CaseModel>().CountAsync();
                }

                if (tp.Equals(typeof(CaseSummaryModel)))
                {
                    return await db.Table<CaseSummaryModel>().CountAsync();
                }

                if (tp.Equals(typeof(FormConfigurationModel)))
                {
                    return await db.Table<FormConfigurationModel>().CountAsync();
                }

                if (tp.Equals(typeof(BannedExtensionListModel)))
                {
                    return await db.Table<BannedExtensionListModel>().CountAsync();
                }

                if (tp.Equals(typeof(LogModel)))
                {
                    return await db.Table<LogModel>().CountAsync();
                }

                if (tp.Equals(typeof(BaseMapModel)))
                {
                    return await db.Table<BaseMapModel>().CountAsync();
                }
            }
            catch { }

            return 0;
        }

        public async Task<IEnumerable> GetAll(object model)
        {

            if (model == null) return null;

            try
            {

                await Init();

                Type tp = model.GetType();

                if (tp.Equals(typeof(MenuModel)))
                {
                    return await db.Table<MenuModel>().ToListAsync();
                }

                if (tp.Equals(typeof(CaseModel)))
                {
                    return await db.Table<CaseModel>().ToListAsync();
                }

                if (tp.Equals(typeof(CaseSummaryModel)))
                {
                    return await db.Table<CaseSummaryModel>().ToListAsync();
                }

                if (tp.Equals(typeof(FormConfigurationModel)))
                {
                    return await db.Table<FormConfigurationModel>().ToListAsync();
                }

                if (tp.Equals(typeof(BannedExtensionListModel)))
                {
                    return await db.Table<BannedExtensionListModel>().ToListAsync();
                }

                if (tp.Equals(typeof(LogModel)))
                {
                    return await db.Table<LogModel>().ToListAsync();
                }

                if (tp.Equals(typeof(BaseMapModel)))
                {
                    return await db.Table<BaseMapModel>().ToListAsync();
                }
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

        /// <summary>
        /// Method is now redundant
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<List<T>?> GetAllOld<T>() where T : new()
        {
            try
            {
                List<T>? tbRecords = null;

                await Init();

                if (typeof(T) == typeof(MenuModel))
                {
                    tbRecords = await db.Table<T>().ToListAsync();
                    return tbRecords;
                }

                if (typeof(T) == typeof(CaseModel))
                {
                    tbRecords = await db.Table<T>().ToListAsync();
                    return tbRecords;
                }

                if (typeof(T) == typeof(CaseSummaryModel))
                {
                    tbRecords = await db.Table<T>().ToListAsync();
                    return tbRecords;
                }

                if (typeof(T) == typeof(FormConfigurationModel))
                {
                    tbRecords = await db.Table<T>().ToListAsync();
                    return tbRecords;
                }

                if (typeof(T) == typeof(BannedExtensionListModel))
                {
                    tbRecords = await db.Table<T>().ToListAsync();
                    return tbRecords;
                }

                if (typeof(T) == typeof(LogModel))
                {
                    tbRecords = await db.Table<T>().ToListAsync();
                    return tbRecords;
                }

                if (typeof(T) == typeof(BaseMapModel))
                {
                    tbRecords = await db.Table<T>().ToListAsync();
                    return tbRecords;
                }

                return tbRecords;
            }
            catch 
            {
                throw;
            }
        }

        public async Task<object> GetById<T>(int id)
        {
            object result = null;

            try
            {
                await Init();

                if (typeof(T) == typeof(MenuModel))
                {
                    result = await db.Table<MenuModel>().FirstOrDefaultAsync(x => x.MenuId.Equals(id));
                }

                if (typeof(T) == typeof(CaseModel))
                {
                    result = await db.Table<CaseModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
                }

                if (typeof(T) == typeof(CaseSummaryModel))
                {
                    result = await db.Table<CaseSummaryModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
                }

                if (typeof(T) == typeof(FormConfigurationModel))
                {
                    result = await db.Table<FormConfigurationModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
                }

                if (typeof(T) == typeof(BannedExtensionListModel))
                {
                    result = await db.Table<BannedExtensionListModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
                }

                if (typeof(T) == typeof(LogModel))
                {
                    result = await db.Table<LogModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
                }

                if (typeof(T) == typeof(BaseMapModel))
                {
                    result = await db.Table<BaseMapModel>().FirstOrDefaultAsync(x => x.Id.Equals(id));
                }
            }
            catch { }

            return result;
        }

        public async Task<object> GetById(object model)
        {
            if (model == null) return null;

            try
            {
                await Init();
                Type tp = model.GetType();

                if (tp.Equals(typeof(MenuModel)))
                {
                    var seekModel = model as MenuModel;
                    var result = await db.Table<MenuModel>().FirstOrDefaultAsync(x => x.MenuId.Equals(seekModel.MenuId));
                    return result;
                }

                if (tp.Equals(typeof(CaseModel)))
                {
                    var seekModel = model as CaseModel;
                    var result = await db.Table<CaseModel>().FirstOrDefaultAsync(x => x.Id.Equals(seekModel.Id));
                    return result;
                }

                if (tp.Equals(typeof(FormConfigurationModel)))
                {
                    var seekModel = model as FormConfigurationModel;
                    var result = await db.Table<FormConfigurationModel>().FirstOrDefaultAsync(x => x.Id.Equals(seekModel.Id));
                    return result;
                }

                if (tp.Equals(typeof(BannedExtensionListModel)))
                {
                    var seekModel = model as BannedExtensionListModel;
                    var result = await db.Table<BannedExtensionListModel>().FirstOrDefaultAsync(x => x.Id.Equals(seekModel.Id));
                    return result;
                }

                if (tp.Equals(typeof(LogModel)))
                {
                    var seekModel = model as LogModel;
                    var result = await db.Table<LogModel>().FirstOrDefaultAsync(x => x.Id.Equals(seekModel.Id));
                    return result;
                }

                if (tp.Equals(typeof(BaseMapModel)))
                {
                    var seekModel = model as BaseMapModel;
                    var result = await db.Table<BaseMapModel>().FirstOrDefaultAsync(x => x.Id.Equals(seekModel.Id));
                    return result;
                }
            }
            catch { }

            return null;
        }

        public async Task<object> GetByFormId(object model)
        {
            if (model == null) return null;

            try
            {
                await Init();

                Type tp = model.GetType();

                if (tp.Equals(typeof(FormConfigurationModel)))
                {
                    var seekModel = model as FormConfigurationModel;
                    var formConfig = _formConfigurations?.FirstOrDefault(x => x.PortalFormId == seekModel.FormID);

                    var result = _formConfigurations?.FirstOrDefault(x => x.PortalFormId == seekModel.FormID);
                    return result;
                }
            }
            catch { }

            return null;
        }

        public object GetConfigByFormId(object model)
        {
            if (model == null) return null;

            try
            {
                Task.Run(async () => await Init());

                Type tp = model.GetType();

                if (tp.Equals(typeof(FormConfigurationModel)))
                {
                    //temp bypass

                    var seekModel = model as FormConfigurationModel;
                    var result = _formConfigurations?.FirstOrDefault(x => x.PortalFormId == seekModel.FormID);
                    return result;
                }
            }
            catch { }

            return null;
        }
    }


}
