using Microsoft.Extensions.Logging;
using SkyQuery.ImageService.Application.Interfaces;
using SkyQuery.ImageService.Domain.Converters;
using SkyQuery.ImageService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SkyQuery.ImageService.Application.Services
{
    public class DataforsyningService : IDataforsyningService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataforsyningService> _logger;

        private readonly string _baseUrl = "https://api.dataforsyningen.dk/orto_foraar";
        private readonly string _layer = "jylland_2011_10cm";
        private readonly string _token = "a072fc76ce87ca3c3997e2c6a1a8b396";
        private readonly int _width = 800;
        private readonly int _height = 800;

        private readonly int _areaSize = 5000;

        private string _bbox = "";

        public DataforsyningService(IHttpClientFactory httpClientFactory, ILogger<DataforsyningService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("dataforsyningclient");
            _logger = logger;
            _bbox = "";
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
            }


            var url = $"{_baseUrl}?service=WMS&request=GetMap&version=1.3.0" +
                        $"&layers={_layer}&styles=&crs=EPSG:3857&bbox={_bbox}" +
                        $"&width={_width}&height={_height}&format=image/png&token={_token}";

            try
            {
                byte[] result = await _httpClient.GetByteArrayAsync(url);
                resultImage.Image = result;
                //TODO: Save in database (Model needs to be made first)
                _logger.LogInformation("External Api successfully called for UserId: {userId} Mgrs: {mgrs}", request.UserId, request.Mgrs);

                return resultImage;
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not call external API for UserId: {userId} Mgrs: {mgrs} Exception: {ex}", request.UserId, request.Mgrs, ex);
                throw new Exception();
                //return resultImage;
            }
        }
    }
}
