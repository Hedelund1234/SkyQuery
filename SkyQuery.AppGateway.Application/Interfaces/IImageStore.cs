using SkyQuery.AppGateway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AppGateway.Application.Interfaces
{
    public interface IImageStore
    {
        Task PutAsync(ImageAvailable imageAvailable);
        Task<ImageAvailable> GetAsync(Guid userId, string Mgrs);
    }
}
