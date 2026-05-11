using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NileGuideApi.Data
{
    // Allows dotnet-ef tooling to create the DbContext outside the running web host.
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var basePath = ResolveBasePath();

            // Match the same configuration sources used by the app so tooling sees the same connection string.
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException("DefaultConnection missing");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(cs);

            return new AppDbContext(optionsBuilder.Options);
        }

        private static string ResolveBasePath()
        {
            var candidatePaths = new[]
            {
                Directory.GetCurrentDirectory(),
                AppContext.BaseDirectory
            };

            foreach (var candidatePath in candidatePaths)
            {
                var resolvedPath = FindPathContaining(candidatePath, "NileGuideApi.csproj")
                    ?? FindPathContaining(candidatePath, "appsettings.json");

                if (resolvedPath != null)
                    return resolvedPath;
            }

            throw new InvalidOperationException("Could not locate the project directory for design-time configuration.");
        }

        private static string? FindPathContaining(string startPath, string markerFileName)
        {
            var directory = new DirectoryInfo(Path.GetFullPath(startPath));
            if (!directory.Exists)
                directory = directory.Parent ?? directory;

            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, markerFileName)))
                    return directory.FullName;

                directory = directory.Parent;
            }

            return null;
        }
    }
}
