using MauiApp2.Database.Models;
using MauiApp2.Dto;

namespace MauiApp2.Interfaces
{
    /// <summary>
    /// Contract for the local SQLite database service.
    /// Each table is exposed through explicit, strongly typed members instead of
    /// generic or runtime type dispatched ones.
    /// </summary>
    public interface IDatabaseService
    {
        Task Init();

        bool IsTableExists(string tableName);

        Task<int> Add(object model);

        Task<int> AddOrReplace(object model);

        Task<int> Update(object model);

        Task<int> Delete(object model);

        Task<int> DeleteAllMenuModels();

        Task<int> DeleteAllCases();

        Task<int> DeleteAllCaseSummaries();

        Task<int> DeleteAllFormConfigurations();

        Task<int> DeleteAllBannedExtensions();

        Task<int> DeleteAllLogs();

        Task<int> DeleteAllBaseMaps();

        Task<int> GetMenuModelCount();

        Task<int> GetCaseCount();

        Task<int> GetCaseSummaryCount();

        Task<int> GetFormConfigurationCount();

        Task<int> GetBannedExtensionCount();

        Task<int> GetLogCount();

        Task<int> GetBaseMapCount();

        Task<List<MenuModel>?> GetAllMenuModels();

        Task<List<CaseModel>?> GetAllCases();

        Task<List<CaseSummaryModel>?> GetAllCaseSummaries();

        Task<List<FormConfigurationModel>?> GetAllFormConfigurations();

        Task<List<BannedExtensionListModel>?> GetAllBannedExtensions();

        Task<List<LogModel>?> GetAllLogs();

        Task<List<BaseMapModel>?> GetAllBaseMaps();

        Task<List<CaseSummaryModel>> GetDraftCases();

        Task<MenuModel?> GetMenuModelById(string menuId);

        Task<CaseModel?> GetCaseById(int id);

        Task<CaseSummaryModel?> GetCaseSummaryById(int id);

        Task<FormConfigurationModel?> GetFormConfigurationById(int id);

        Task<BannedExtensionListModel?> GetBannedExtensionById(int id);

        Task<LogModel?> GetLogById(int id);

        Task<BaseMapModel?> GetBaseMapById(int id);

        Task<FormConfiguration?> GetByFormId(FormConfigurationModel model);

        FormConfiguration? GetConfigByFormId(FormConfigurationModel model);
    }
}
