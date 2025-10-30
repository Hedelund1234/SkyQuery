using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Domain.Entities
{
    public class ImageRequestWithToken
    {
        public Guid UserId { get; set; }
        public string Mgrs { get; set; } = string.Empty;
        public string JWT { get; set; } = string.Empty;
    }
}
