namespace DynamicContainerManager.WebApi.Extensions;

public static class GenerateConnectionStringExtensions
{
    public static string GetMssqlConnectionString(string host, string port, string databaseName, string username, string password)
    {
        return $"Server={host},{port};Database={databaseName};User Id={username};Password={password};TrustServerCertificate=true";
    }
}