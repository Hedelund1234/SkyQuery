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
                throw new InvalidOperationException("Image with that timestamp already in tempdb"); // Needs to be changed!
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

        public async Task<ImageAvailable> GetAsync(Guid userId, string mgrs)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Invalid userId", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(mgrs))
            {
                throw new ArgumentException("Mgrs is required", nameof(mgrs));
            }

            var safeMgrs = mgrs.Replace(" ", "");

            var dir = Path.Combine(_root, userId.ToString());

            if (!Directory.Exists(dir))
            {
                throw new FileNotFoundException($"No image for {userId} found");
            }

            // Find seneste fil der matcher MGRS
            var pattern = $"{safeMgrs}_*.png";

            var latestPath = Directory.EnumerateFiles(dir, pattern, SearchOption.TopDirectoryOnly)
                                        .OrderByDescending(p => File.GetCreationTimeUtc(p))   // eller LastWriteTimeUtc
                                        .FirstOrDefault();

            if (latestPath is null)
            {
                throw new FileNotFoundException($"No image starting with {safeMgrs} found for user {userId}");
            }
                

            var bytes = await File.ReadAllBytesAsync(latestPath);

            var image = new ImageAvailable
            {
                UserId = userId,
                Mgrs = mgrs,
                ContentType = "png",
                Image = bytes
            };

            return image;
        }

        public List<string> CheckReadiness(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Invalid userId", nameof(userId));
            }

            var dir = Path.Combine(_root, userId.ToString());

            if (!Directory.Exists(dir))
            {
                return new List<string>();
                //throw new FileNotFoundException($"No image for {userId} found");
            }

            var files = Directory.GetFiles(dir)
                                .Select(f => Path.GetFileName(f) ?? string.Empty)
                                .ToList();

            return files;
        }
    }
}
