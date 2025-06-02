using ChatbotAI.API.Hubs;
using ChatbotAI.Application;
using ChatbotAI.Infrastructure;
using ChatbotAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using ChatbotAI.API.Services;
using ChatbotAI.API;
using System.Net.Http.Headers;
using ChatbotAI.Application.Services;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add SignalR
builder.Services.AddSignalR();

// Register application and infrastructure services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add configuration binding for ChatSettings and bind to instance to respect class defaults
var chatSettingsSection = builder.Configuration.GetSection("ChatSettings");
builder.Services.Configure<ChatSettings>(chatSettingsSection);
var chatSettings = chatSettingsSection.Get<ChatSettings>() ?? new ChatSettings();
var useMcp = chatSettings.UseMcpService;

// Choose streaming service implementation based on configuration
if (useMcp)
{
    // Register MCP Shapeshifter service as a typed HTTP client for streaming
    builder.Services.AddHttpClient<IStreamingService, ShapeshifterMcpService>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:3000");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
    });
}
else
{
    // Fallback to the built-in Lorem Ipsum streaming service
    builder.Services.AddSingleton<IStreamingService, LoremIpsumService>();
}

// Register chat service for managing message generation and DB operations with singleton lifetime
builder.Services.AddSingleton<IChatService, ChatService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Initialize the database using migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS - must be before routing
app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub").RequireCors("CorsPolicy");

app.Run();
