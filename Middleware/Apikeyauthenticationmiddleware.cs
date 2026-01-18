namespace FullstackHA.Middleware
{
    public class ApiKeyAuthMiddleware
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for Swagger endpoints
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.Value == "/")
            {
                await _next(context);
                return;
            }

            // Check if API key is provided
            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Unauthorized",
                    message = "API Key is missing. Please provide X-Api-Key header."
                });
                return;
            }

            var apiKey = extractedApiKey.ToString();
            var readWriteKey = _configuration["ApiKeys:FS_ReadWrite"];
            var readKey = _configuration["ApiKeys:FS_Read"];

            // Get the endpoint path
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method;

            // Determine required permission
            bool requiresWriteAccess = path.Contains("setmap") && method == "POST";

            // Validate API key and permissions
            if (requiresWriteAccess)
            {
                // SetMap requires FS_ReadWrite key
                if (apiKey != readWriteKey)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "Invalid API Key or insufficient permissions. FS_ReadWrite key required."
                    });
                    return;
                }
            }
            else
            {
                // Other endpoints require at least FS_Read key
                if (apiKey != readKey && apiKey != readWriteKey)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "Unauthorized",
                        message = "Invalid API Key. FS_Read or FS_ReadWrite key required."
                    });
                    return;
                }
            }

            // API key is valid, proceed to next middleware
            await _next(context);
        }
    }

}
