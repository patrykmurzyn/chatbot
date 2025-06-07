using ChatbotAI.API.Hubs;
using ChatbotAI.API.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Register services using extension methods
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddChatSettings(builder.Configuration);
builder.Services.AddStreamingServices(builder.Configuration);
builder.Services.AddChatServices();
builder.Services.AddApiFeatures(builder.Configuration);

var app = builder.Build();

// Apply database migrations
app.UseMigrations();

// Enable Swagger UI in Development
app.UseSwaggerIfDevelopment();

// Configure middleware pipeline
app.UseHttpsRedirection();
app.UseCustomCors();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub").RequireCors("CorsPolicy");

app.Run();
