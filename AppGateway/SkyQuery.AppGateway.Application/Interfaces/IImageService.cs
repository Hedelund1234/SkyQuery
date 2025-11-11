using SkyQuery.AppGateway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AppGateway.Application.Interfaces
{
    public interface IImageService
    {
        Task GetNewImage(ImageRequestWithToken requestWithToken);
    }
}
