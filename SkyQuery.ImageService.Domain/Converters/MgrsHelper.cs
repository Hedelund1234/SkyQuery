using System;
using System.Globalization;
using System.Net;
using System.Web;
using CoordinateSharp;

namespace SkyQuery.ImageService.Domain.Converters
{
    public static class MgrsHelper
    {
        public static (double lon, double lat) MgrsToLonLat(string mgrs)
        {
            if (string.IsNullOrWhiteSpace(mgrs))
            {
                throw new ArgumentException("MGRS is empty");
            }

            // fjern mellemrum, TryParse kan normalt også selv, men det gør ingen skade
            string clean = mgrs.Replace(" ", "");

            if (!Coordinate.TryParse(clean, out Coordinate c))
            {
                throw new ArgumentException($"Could not parse MGRS: '{mgrs}'");
            }

            double lat = c.Latitude.DecimalDegree;
            double lon = c.Longitude.DecimalDegree;
            return (lon, lat);
        }

        public static (double x, double y) LonLatToWebMercator(double lon, double lat)
        {
            // Mercator-clamp
            lat = Math.Max(Math.Min(lat, 85.05112878), -85.05112878);

            const double R = 6378137.0; // WGS84
            double lonRad = lon * Math.PI / 180.0;
            double latRad = lat * Math.PI / 180.0;

            double x = R * lonRad;
            double y = R * Math.Log(Math.Tan(Math.PI / 4.0 + latRad / 2.0));
            return (x, y);
        }

        public static (int minx, int miny, int maxx, int maxy) CalculateBoxForPicture(double areaSize, double x, double y)
        {
            var halfSize = areaSize / 2;

            double minxD = x - halfSize;
            double minyD = y - halfSize;
            double maxxD = x + halfSize;
            double maxyD = y + halfSize;

            int minx = (int)minxD;
            int miny = (int)minyD;
            int maxx = (int)maxxD;
            int maxy = (int)maxyD;
            

            return (minx, miny, maxx, maxy);
        }
    }
}
