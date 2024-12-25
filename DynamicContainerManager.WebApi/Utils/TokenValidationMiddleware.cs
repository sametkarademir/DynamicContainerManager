namespace DynamicContainerManager.WebApi.Utils;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public TokenValidationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
    {
        var expectedToken = _configuration["AppSettings:Token"];
        context.Request.Headers.TryGetValue("X-Access-Token", out var token);
        
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token is required");
            return;
        }
        
        if (token != expectedToken)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid Token");
            return;
        }
        
        await _next(context);
    }
}