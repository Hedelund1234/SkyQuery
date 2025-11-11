using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Domain.Entities
{
    public class Image
    {
        public Guid ImageId { get; set; } = Guid.NewGuid();
        [MaxLength(20)]
        public string Mgrs { get; set; } = "";
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        [MaxLength(10)]
        public string ContentType { get; set; } = "png";
    }
}
