namespace Manager.Contract;

public interface IScrappeManager
{
    Task ScrappeSearchAndPersistAsync(string searchText);
}