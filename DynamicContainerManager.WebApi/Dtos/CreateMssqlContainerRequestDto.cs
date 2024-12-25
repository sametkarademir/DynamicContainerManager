namespace DynamicContainerManager.WebApi.Dtos;

public class CreateMssqlContainerRequestDto
{
    public string? ImageName { get; set; }
    public string? HostPort { get; set; }
    public string? SaPassword { get; set; }
    public string? DatabaseName { get; set; }
}