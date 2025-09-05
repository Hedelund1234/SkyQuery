using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Application.Interfaces
{
    public interface IDataforsyningService
    {
        Task<byte[]> GetMapFromDFAsync(Guid userId, string mgrs);
    }
}
