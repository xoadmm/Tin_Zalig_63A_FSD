using FullstackHA.Middleware;
using FullstackHA.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the MapService as Singleton (to maintain state in memory)
builder.Services.AddSingleton<IMapService, MapService>();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shortest Path API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");

// Add custom API Key authentication middleware
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();