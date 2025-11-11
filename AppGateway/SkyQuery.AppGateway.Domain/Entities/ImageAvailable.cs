using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.AppGateway.Domain.Entities
{
    public class ImageAvailable
    {
        public Guid UserId { get; set; }
        public string Mgrs { get; set; } = "";
        public byte[] Image { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "png";
    }
}
