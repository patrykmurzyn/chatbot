using ChatbotAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatbotAI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register SQL Server database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ChatbotAI.Domain.Repositories.IMessageRepository, ChatbotAI.Infrastructure.Data.Repositories.MessageRepository>();
            services.AddScoped<ChatbotAI.Domain.Repositories.ISessionRepository, ChatbotAI.Infrastructure.Data.Repositories.SessionRepository>();
            services.AddScoped<ChatbotAI.Domain.Repositories.ICharacterRepository, ChatbotAI.Infrastructure.Data.Repositories.CharacterRepository>();
            services.AddScoped<ChatbotAI.Domain.Repositories.IMessageRatingRepository, ChatbotAI.Infrastructure.Data.Repositories.MessageRatingRepository>();
            services.AddScoped<ChatbotAI.Domain.Repositories.IUnitOfWork, ChatbotAI.Infrastructure.Data.Repositories.UnitOfWork>();

            return services;
        }
    }
} 