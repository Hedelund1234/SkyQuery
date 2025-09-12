using SkyQuery.ImageService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Application.Interfaces.Persistence
{
    public interface IDataforsyningImageRepository
    {
        Task AddImageAsync(Image image);
        Task<Image?> GetImageByMgrs(string mgrs);
    }
}
