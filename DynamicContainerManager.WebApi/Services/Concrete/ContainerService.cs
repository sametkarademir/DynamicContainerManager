using Docker.DotNet;
using Docker.DotNet.Models;
using DynamicContainerManager.WebApi.Dtos;
using DynamicContainerManager.WebApi.Extensions;
using DynamicContainerManager.WebApi.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DynamicContainerManager.WebApi.Services.Concrete;

public class ContainerService(ILogger<ContainerService> logger, IConfiguration configuration, IMemoryCache memoryCache) : IContainerService
{
    public async Task<string> CreateMssqlContainerAsync(CreateMssqlContainerRequestDto request)
    {
        var imageName = request.ImageName ?? configuration["Docker:Mssql:DefaultMssqlImage"]!;
        var saPassword = request.SaPassword ?? PasswordGeneratorExtensions.GeneratePassword(length: 12, includeUpperCase: true, includeNumbers: true, includeSpecialChars: true);
        
        using var dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        
        await PullImageIfNotExists(dockerClient, imageName);
        
        var containerId = await CreateAndStartContainer(
            dockerClient,
            imageName,
            configuration["Docker:Mssql:DefaultMssqlContainerName"]!,
            saPassword,
            request.HostPort
            );

        memoryCache.Set("mssql_container", containerId);
        
        var connectionString = GenerateConnectionStringExtensions.GetMssqlConnectionString(
            configuration["AppSettings:Host"]!,
            request.HostPort ?? configuration["Docker:Mssql:DefaultMssqlPort"]!,
            request.DatabaseName ?? configuration["Docker:Mssql:DefaultMssqlDatabaseName"]!,
            "sa",
            request.SaPassword ?? saPassword
        );
        
        return connectionString;
    }

    public async Task DeleteContainerAsync(string containerId)
    {
        logger.LogInformation($"Container is being deleted: {containerId}");
        
        using var dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        
        await dockerClient.Containers.StopContainerAsync(containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });

        logger.LogInformation("Container is being removed...");
        await dockerClient.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters { Force = true });
    }
    
    
    private async Task PullImageIfNotExists(DockerClient client, string imageName)
    {
        logger.LogInformation($"Docker image is being checked: {imageName}");
        
        var imageParts = imageName.Split(':');
        var imageRepo = imageParts[0];
        var imageTag = imageParts.Length > 1 ? imageParts[1] : "latest";
        
        var filters = new Dictionary<string, IDictionary<string, bool>>
        {
            { "reference", new Dictionary<string, bool> { { $"{imageRepo}:{imageTag}", true } } }
        };
        
        var images = await client.Images.ListImagesAsync(new ImagesListParameters
        {
            Filters = filters,
        });

        if (images.Count == 0)
        {
            logger.LogInformation($"Docker image not found. Pulling: {imageName}");
            
            await client.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName },
                null,
                new Progress<JSONMessage>(message => Console.WriteLine(message.Status))
            );
        }
        else
        {
            logger.LogInformation("Docker image exists.");
        }
    }
    private async Task<string> CreateAndStartContainer(DockerClient client, string imageName, string containerName, string saPassword, string? hostPort = null)
    {
        logger.LogInformation("Container is being created...");
        
        var createParams = new CreateContainerParameters
        {
            Name = GenerateContainerNameExtensions.GenerateContainerName(containerName),
            Image = imageName,
            Env = new List<string>
            {
                "ACCEPT_EULA=Y",
                "MSSQL_SA_PASSWORD=" + saPassword
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { "1433/tcp", new List<PortBinding> { new PortBinding { HostPort = hostPort ?? "1433" } } }
                }
            }
        };
        
        var container = await client.Containers.CreateContainerAsync(createParams);
        
        logger.LogInformation("Container is being started...");
        
        await client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
        return container.ID;
    }
    
    
}