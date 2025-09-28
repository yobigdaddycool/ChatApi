using Microsoft.EntityFrameworkCore;
using ChatApi.Data;
using ChatApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                      "Data Source=chatapi.db"));

// Register application services
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IKeyPointService, KeyPointService>();
builder.Services.AddScoped<ISummarizationService, SummarizationService>();

// Add CORS policy for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("DevPolicy");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

// Add a simple root endpoint with API information
app.MapGet("/", () => new { 
    Name = "ChatApi", 
    Version = "1.0.0",
    Description = "API for managing chat conversations, key points, and summaries",
    Endpoints = new[] {
        "/api/conversations - Chat conversation management",
        "/api/keypoints - Key point management",
        "/health - Health check"
    }
});

app.Run();
