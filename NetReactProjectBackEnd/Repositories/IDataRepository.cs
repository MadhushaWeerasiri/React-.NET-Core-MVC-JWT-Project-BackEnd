using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public interface IDataRepository
{
    Task<IEnumerable<DataItem>> GetAllAsync();
    Task<IEnumerable<DataItem>> GetByUserIdAsync(int userId);
}