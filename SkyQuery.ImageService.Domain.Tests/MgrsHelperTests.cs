using SkyQuery.ImageService.Domain.Converters;
using System.Runtime.InteropServices.Marshalling;

namespace SkyQuery.ImageService.Domain.Tests
{
    public class MgrsHelperTests
    {
        [Fact]
        public void That_MgrsToLonLat_Works()
        {
            var (lon, lat) = MgrsHelper.MgrsToLonLat("32UNG 3032 2526");

            double lonShouldBe = 9.4772290771554388;
            double latShouldBe = 55.272864730948058;


            Assert.Equal(lon, lonShouldBe);
            Assert.Equal(lat, latShouldBe);
        }

        [Fact]
        public void That_LonLatToWebMercator_Works()
        {
            var (lon, lat) = MgrsHelper.MgrsToLonLat("32UNG 3032 2526");

            var (x, y) = MgrsHelper.LonLatToWebMercator(lon, lat);

            double xShouldBe = 1055000.3150001494;
            double yShouldBe = 7415004.7085246937;

            Assert.Equal(x, xShouldBe);
            Assert.Equal(y, yShouldBe);
        }

        [Fact]
        public void That_CalculateBoxForPicture_Works()
        {
            var (lon, lat) = MgrsHelper.MgrsToLonLat("32UNG 3032 2526");

            var (x, y) = MgrsHelper.LonLatToWebMercator(lon, lat);

            double areaSize = 5000;

            var (minx, miny, maxx, maxy) = MgrsHelper.CalculateBoxForPicture(areaSize, x, y);

            double minxShouldBe = 1052500;
            double minyShouldBe = 7412504;
            double maxxShouldBe = 1057500;
            double maxyShouldBe = 7417504;

            Assert.Equal(minx, minxShouldBe);
            Assert.Equal(miny, minyShouldBe);
            Assert.Equal(maxx, maxxShouldBe);
            Assert.Equal(maxy, maxyShouldBe);

        }
    }
}
