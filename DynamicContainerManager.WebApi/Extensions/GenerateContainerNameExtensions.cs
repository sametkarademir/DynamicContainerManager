namespace DynamicContainerManager.WebApi.Extensions;

public static class GenerateContainerNameExtensions
{
    public static string GenerateContainerName(string baseName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        return $"{baseName}-{timestamp}";
    }
}