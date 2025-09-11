using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SkyQuery.AppGateway.Application.Interfaces;
using SkyQuery.AppGateway.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkyQuery.AppGateway.Infrastructure.TempStorage
{
    public class FileSystemImageStore : IImageStore
    {
        private readonly string _root;

        public FileSystemImageStore(IConfiguration cfg)
        {
            _root = cfg["ImageStore:Root"] ?? throw new InvalidOperationException("ImageStore:Root is not configured");

            Directory.CreateDirectory(_root);
        }

        public async Task PutAsync(ImageAvailable imageAvailable)
        {
            if (imageAvailable == null)
            {
                throw new ArgumentNullException(nameof(imageAvailable));
            }

            if (imageAvailable.Image == null || imageAvailable.Image.Length == 0)
            {
                throw new ArgumentException("Image is empty", nameof(imageAvailable));
            }

            if (imageAvailable.UserId == Guid.Empty || imageAvailable.Mgrs == null)
            {
                throw new ArgumentException("UserId og Mgrs not present", nameof(imageAvailable));
            }

            if (imageAvailable.ContentType != "png")
            {
                throw new NotSupportedException("ContentType not supported (only support png)");
            }

            var id = imageAvailable.UserId.ToString();

            var safeMgrs = imageAvailable.Mgrs.Replace(" ", "");

            var extensions = imageAvailable.ContentType switch
            {
                "png" => ".png",
                _ => throw new UnreachableException()
            };

            var fileName = $"{safeMgrs}_{DateTime.UtcNow:yyyyMMdd_HHmm}{extensions}";

            

            var dir = Path.Combine(_root, id);
            Directory.CreateDirectory(dir);

            // Gem billedet som en "rigtig" fil (så du kan dobbeltklikke på den i mappen)
            var filePath = Path.Combine(dir, fileName);

            if (File.Exists(filePath))
            {
                throw new InvalidOperationException("Image with that timestamp already in tempdb");
            }

            // Atomisk skriv: skriv til .tmp og rename
            var tmp = filePath + ".tmp";
            await File.WriteAllBytesAsync(tmp, imageAvailable.Image);
            File.Move(tmp, filePath, overwrite: true);


            // Gem lidt metadata ved siden af som JSON
            var metaPath = Path.Combine(dir, $"{fileName}_meta.json");
            var metaTmp = metaPath + ".tmp";
            var meta = new
            {
                imageAvailable.UserId,
                imageAvailable.Mgrs,
                imageAvailable.ContentType,
                Filename = fileName,
                Length = imageAvailable.Image.LongLength,
                Created = DateTimeOffset.UtcNow
            };

            await File.WriteAllTextAsync(metaTmp, System.Text.Json.JsonSerializer.Serialize(meta));
            File.Move(metaTmp, metaPath, overwrite: true);
        }

        public async Task<ImageAvailable> GetAsync(Guid userId, string Mgrs)
        {
            throw new NotImplementedException();
        }
    }
}
