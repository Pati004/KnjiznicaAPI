using Microsoft.EntityFrameworkCore;
using KnjiznicaAPI.Data;
using SQLitePCL;

namespace KnjiznicaAPI.Configuration
{
    public static class DatabaseInitializer
    {
        public static void InitializeSQLite()
        {
            // Initialize SQLitePCL batteries
            Batteries.Init();
        }

        public static void EnsureDatabaseCreated(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KnjiznicaDbContext>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseInitializer");

            try
            {
                // Ensure database is created
                context.Database.EnsureCreated();

                logger.LogInformation("Database initialized successfully");

                // Check if data exists, if not seed it
                if (!context.Avtorji.Any())
                {
                    logger.LogInformation("Seeding database with initial data...");
                    // Database will be seeded automatically via ModelBuilder configuration
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred creating the database");
                throw;
            }
        }

        public static void ConfigureSQLite(this IServiceCollection services, IConfiguration configuration)
        {
            // Initialize SQLite
            InitializeSQLite();

            // Add DbContext
            services.AddDbContext<KnjiznicaDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);

                // Enable sensitive data logging in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });
        }
    }
}