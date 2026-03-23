using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetReactProjectBackEnd.Repositories;

namespace NetReactProjectBackEnd.Controllers;

[ApiController]
[Route("api/data")]
[Authorize]
public class DataController : Controller
{
    private readonly IDataRepository _dataRepository;

    public DataController(IDataRepository dataRepository) => _dataRepository = dataRepository;

    [HttpGet("data")]
    public async Task<IActionResult> GetData()
    {
        var data = await _dataRepository.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("data/{userId}")]
    public async Task<IActionResult> GetDataByUser(int userId)
    {
        var data = await _dataRepository.GetByUserIdAsync(userId);
        if (data == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(data);
        }
    }
}