using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Domain.Entities
{
    public class ImageRequest
    {
        public Guid UserId { get; set; }
        public string Mgrs { get; set; } = string.Empty;
    }
}
