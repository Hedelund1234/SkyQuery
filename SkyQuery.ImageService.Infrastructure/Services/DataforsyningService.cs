using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkyQuery.ImageService.Application.Interfaces;
using SkyQuery.ImageService.Application.Interfaces.Persistence;
using SkyQuery.ImageService.Domain.Converters;
using SkyQuery.ImageService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Infrastructure.Services
{
    public class DataforsyningService : IDataforsyningService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataforsyningService> _logger;
        private readonly IDataforsyningImageRepository _imageRepository;

        private readonly string _baseUrl = "https://api.dataforsyningen.dk/orto_foraar_DAF";
        private readonly string _layer = "geodanmark_2024_12_5cm";
        private readonly string _token;
        private readonly int _width = 1024;
        private readonly int _height = 1024;

        private readonly int _areaSize = 5000;

        private string _bbox = "";

        public DataforsyningService(IHttpClientFactory httpClientFactory, ILogger<DataforsyningService> logger, IDataforsyningImageRepository imageRepository, IConfiguration cfg)
        {
            _httpClient = httpClientFactory.CreateClient("dataforsyningclient");
            _logger = logger;
            _token = cfg["Dataforsyningen:ApiKey"]
                ?? throw new InvalidOperationException("Dataforsyningen:ApiKey mangler");
            _bbox = "";
            _imageRepository = imageRepository;
        }

        public async Task<ImageAvailable> GetMapFromDFAsync(ImageRequest request)
        {
            ImageAvailable resultImage = new ImageAvailable();
            resultImage.UserId = request.UserId;
            resultImage.Mgrs = request.Mgrs;
            try
            {
                var (lon, lat) = MgrsHelper.MgrsToLonLat(request.Mgrs);
                var (x, y) = MgrsHelper.LonLatToWebMercator(lon, lat);
                var (minx, miny, maxx, maxy) = MgrsHelper.CalculateBoxForPicture(_areaSize, x, y);

                _bbox = $"{minx},{miny},{maxx},{maxy}";
            }
            catch (Exception ex)
            {
                _logger.LogError("Box Calculation went wrong UserId: {userId} Mgrs: {mgrs} Exception: {ex}", request.UserId, request.Mgrs, ex);
                throw new InvalidDataException($"Box Calculation went wrong for request from  {request.UserId} - Mgrs was requested: {request.Mgrs}");
            }

            var mgrsForInput = request.Mgrs.Replace(" ", "");

            //This checks if image is in database
            try
            {
                var image = await _imageRepository.GetImageByMgrs(mgrsForInput);
                if (image != null && image.Bytes.Length > 0)
                {
                    _logger.LogInformation("Image already existed in Database");
                    resultImage.Image = image.Bytes;
                    return resultImage;
                }
            }
            catch
            {
                _logger.LogError("While getting database content something went wrong");
                throw new Exception();
            }

            if (_bbox == "")
            {
                _logger.LogInformation("Box for {request.UserId} was empty", request.UserId);
                throw new InvalidDataException($"Box size was empty for request from  {request.UserId} - Mgrs was requested: {request.Mgrs}");
            }

            var url = $"{_baseUrl}?service=WMS&request=GetMap&version=1.3.0" +
                        $"&layers={_layer}&styles=&crs=EPSG:3857&bbox={_bbox}" +
                        $"&width={_width}&height={_height}&format=image/png&token={_token}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode is HttpStatusCode.BadRequest // 400
                                        or HttpStatusCode.Forbidden // 403
                                        or HttpStatusCode.NotFound) // 404
                {
                    _logger.LogInformation("Permanent DF-fejl {Status} for {Mgrs}. Sender til DLQ.", (int)response.StatusCode, request.Mgrs);

                    throw new InvalidDataException($"Permanent DF-fejl {(int)response.StatusCode}.");
                }

                var result = await response.Content.ReadAsByteArrayAsync();
                if (result.Length == 388) // This is the size of the "no data" image from DF
                {
                    _logger.LogInformation("No picture found {Status} for {Mgrs}.", (int)response.StatusCode, request.Mgrs);
                    throw new InvalidDataException($"Response from DF was empty for request from  {request.UserId} - Mgrs was requested: {request.Mgrs}");
                }
                resultImage.Image = result;
                _logger.LogInformation("External Api successfully called for UserId: {userId} Mgrs: {mgrs}", request.UserId, request.Mgrs);

                // This saves to db
                var image = new Image
                {
                    Mgrs = mgrsForInput,
                    Bytes = result
                };
                await _imageRepository.AddImageAsync(image);

                return resultImage;
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not call external API for UserId: {userId} Mgrs: {mgrs} Exception: {ex}", request.UserId, request.Mgrs, ex);
                throw;
            }
        }
    }
}
