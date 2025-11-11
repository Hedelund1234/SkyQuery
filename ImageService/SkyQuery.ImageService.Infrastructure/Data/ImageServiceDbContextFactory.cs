using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Infrastructure.Data
{
    public class ImageServiceDbContextFactory : IDesignTimeDbContextFactory<ImageServiceDbContext>
    {
        public ImageServiceDbContext CreateDbContext(string[] args)
        {
            // find appsettings.json i startup-projektet (SkyQuery.ImageService)
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../SkyQuery.ImageService");

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("DefaultConnection");

            var options = new DbContextOptionsBuilder<ImageServiceDbContext>()
                .UseSqlServer(conn)
                .Options;

            return new ImageServiceDbContext(options);
        }
    }
}
