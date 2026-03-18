using Dapper;
using NetReactProjectBackEnd.Data;
using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public class DataRepository : IDataRepository
{
    private readonly DapperContext _context;

    public DataRepository(DapperContext context) => _context = context;

    public async Task<IEnumerable<DataItem>> GetAllAsync()
    {
        using var con = _context.CreateConnection();
        return await con.QueryAsync<DataItem>("SELECT * FROM dbo.DataItems");
    }
}