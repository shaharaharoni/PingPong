namespace Web1
{
    public interface IStorageService
    {
        Task<Item> GetAsync(string id);
        Task AddAsync(Item item);
        Task DeleteAsync(string id);
        Task UpdateAsync(string id, Item item);
        Task<IEnumerable<Item>> GetAllAsync();
    }
}
