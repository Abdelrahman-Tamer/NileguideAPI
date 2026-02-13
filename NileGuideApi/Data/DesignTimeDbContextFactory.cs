using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NileGuideApi.Data;

namespace NileGuideApi
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer("Server=ABDO\\NILEGUIDEDB;Database=NileGuideDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}