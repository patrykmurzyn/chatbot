using ChatbotAI.API;
using ChatbotAI.API.Services;
using ChatbotAI.Application;
using ChatbotAI.Infrastructure;
using ChatbotAI.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

namespace ChatbotAI.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers application and infrastructure services.
        /// </summary>
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication();
            services.AddInfrastructure(configuration);
            return services;
        }

        /// <summary>
        /// Configures ChatSettings using the Options pattern.
        /// </summary>
        public static IServiceCollection AddChatSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ChatSettings>(configuration.GetSection("ChatSettings"));
            return services;
        }

        /// <summary>
        /// Registers the IStreamingService implementation based on ChatSettings (Strategy Pattern).
        /// </summary>
        public static IServiceCollection AddStreamingServices(this IServiceCollection services, IConfiguration configuration)
        {
            var chatSettings = configuration.GetSection("ChatSettings").Get<ChatSettings>() ?? new ChatSettings();

            if (chatSettings.UseMcpService)
            {
                services.AddHttpClient<IStreamingService, ShapeshifterMcpService>(client =>
                {
                    client.BaseAddress = new Uri(chatSettings.McpBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                });
            }
            else
            {
                services.AddSingleton<IStreamingService, LoremIpsumService>();
            }

            return services;
        }

        /// <summary>
        /// Registers the core chat service.
        /// </summary>
        public static IServiceCollection AddChatServices(this IServiceCollection services)
        {
            services.AddSingleton<IChatService, ChatService>();
            return services;
        }

        /// <summary>
        /// Registers API-specific features like CORS, SignalR, and Swagger.
        /// </summary>
        public static IServiceCollection AddApiFeatures(this IServiceCollection services, IConfiguration configuration)
        {
            // CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                    builder.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" })
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials());
            });

            // SignalR
            services.AddSignalR();

            // Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }
    }
} 