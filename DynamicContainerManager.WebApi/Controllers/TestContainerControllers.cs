using DynamicContainerManager.WebApi.Dtos;
using DynamicContainerManager.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DynamicContainerManager.WebApi.Controllers;

[ApiController]
[Route("api/v1/test-containers")]
public class TestContainerControllers(IContainerService containerService, IMemoryCache memoryCache) : ControllerBase
{

    [HttpGet("get-mssql")]
    public async Task<IActionResult> GetMssqlConnectionStringAsync([FromQuery] CreateMssqlContainerRequestDto request)
    {
        var result = await containerService.CreateMssqlContainerAsync(request);
        
        return Ok(result);
    }
    
    [HttpDelete("delete-mssql")]
    public async Task<IActionResult> DeleteMssqlConnectionStringAsync()
    {
        memoryCache.TryGetValue("mssql_container", out string? containerId);

        await containerService.DeleteContainerAsync(containerId ?? "");
        return Ok(containerId);
    }
}