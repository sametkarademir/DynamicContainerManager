using DynamicContainerManager.WebApi.Dtos;

namespace DynamicContainerManager.WebApi.Services.Interfaces;

public interface IContainerService
{
    Task<string> CreateMssqlContainerAsync(CreateMssqlContainerRequestDto request);
    Task DeleteContainerAsync(string containerId);
}