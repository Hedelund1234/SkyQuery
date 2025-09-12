using Microsoft.EntityFrameworkCore;
using SkyQuery.ImageService.Application.Interfaces.Persistence;
using SkyQuery.ImageService.Domain.Entities;
using SkyQuery.ImageService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Infrastructure.Persistence
{
    public class DataforsyningImageRepository : IDataforsyningImageRepository
    {
        private readonly ImageServiceDbContext _dbContext;
        public DataforsyningImageRepository(ImageServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddImageAsync(Image image)
        {
            await _dbContext.Images.AddAsync(image);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Image?> GetImageByMgrs(string mgrs)
        {
            var image = await _dbContext.Images.AsNoTracking().FirstOrDefaultAsync(i => i.Mgrs == mgrs);
            return image;
        }
    }
}
