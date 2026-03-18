using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public interface IDataRepository
{
    Task<IEnumerable<DataItem>> GetAllAsync();
}