using Microsoft.EntityFrameworkCore;
using SkyQuery.ImageService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Infrastructure.Data
{
    public class ImageServiceDbContext : DbContext
    {
        public ImageServiceDbContext(DbContextOptions<ImageServiceDbContext> options)
            : base(options) { }

        public DbSet<Image> Images { get; set; }
    }
}
