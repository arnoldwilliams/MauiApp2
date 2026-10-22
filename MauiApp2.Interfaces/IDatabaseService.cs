using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MauiApp2.Database.Models;

namespace MauiApp2.Interfaces
{
    public interface IDatabaseService
    {
        public Task Init();

        public bool IsTableExists(string tableName);

        public Task<int> Add(object model);

        public Task<int> Update(object model);

        public Task<int> Delete(object model);

        public Task<int> DeleteAll(object model);

        public Task<int> DeleteAllAsync<T>();

        public Task<int> GetCount(object model);

        public Task<IEnumerable> GetAll(object model);

        public Task<List<T>?> GetAllOld<T>() where T : new();

        // public Task<List<T>?> Get<T>();
        public Task<List<CaseSummaryModel>> GetDraftCases();

        public Task<object> GetById<T>(int id);

        public Task<object> GetById(object model);

        public Task<object> GetByFormId(object model);

        public object GetConfigByFormId(object model);
        public Task<int> AddOrReplace(object model);

    }

}
