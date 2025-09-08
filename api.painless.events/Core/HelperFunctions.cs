using api.painless.events.Entities;
using GeoCoordinatePortable;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;

namespace api.painless.events.Core
{
    public class HelperFunctions
    {

        private readonly IConfiguration _configuration;
        private ReadContext _readContext;
        private WriteContext _writeContext;

        public HelperFunctions(IConfiguration configuration, ReadContext readContext, WriteContext writeContext)
        {
            _configuration = configuration;
            _readContext = readContext;
            _writeContext = writeContext;
        }




        public async Task<(string[], string[], string, double, double)> GetIpInfo(string IpAddress)
        {
            string TimeZone = "";
            double Latitude = 0;
            double Longitude = 0;

            if (IpAddress.Length > 0)
            {
                try
                {
                    using (MaxMind.GeoIP2.DatabaseReader maxmind = new MaxMind.GeoIP2.DatabaseReader(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "\\GeoLite2-City.mmdb"))
                    {
                        MaxMind.GeoIP2.Responses.CityResponse? maxmindResponse = maxmind.City(IpAddress);
                        Latitude = maxmindResponse.Location.Latitude ?? 0;
                        Longitude = maxmindResponse.Location.Longitude ?? 0;
                        TimeZone = maxmindResponse.Location.TimeZone ?? "";
                    }

                }
                catch (Exception e) { }
            }
            //calculate api distances
            SortedDictionary<int, string> distances = new SortedDictionary<int, string>();
            GeoCoordinate gc1 = new GeoCoordinate(Latitude, Longitude);
            List<Node> nodes = await (from n in _readContext.Nodes where n.IsActive == 1 select n).ToListAsync();
            List<string> unsortedNodes = nodes.Select(x => x.Domain).ToList();
            foreach (Node node in nodes)
            {
                GeoCoordinate gc2 = new GeoCoordinate(node.Latitude, node.Longitude);
                int distance = (int)gc1.GetDistanceTo(gc2);
                distances.Add(distance, node.Domain);
            }
            List<string> sortedNodes = distances.OrderBy(x => x.Key).Select(x => x.Value).ToList();
            return (sortedNodes.ToArray(), unsortedNodes.ToArray(), TimeZone, Latitude, Longitude);
        }



        public async Task<string> GetFileChecksum(string fileUrl)
        {
            string checksum = "";
            try
            {
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(fileUrl))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var sha256 = SHA256.Create())
                            {
                                byte[] checksumBytes = sha256.ComputeHash(stream);
                                checksum = BitConverter.ToString(checksumBytes).Replace("-", string.Empty);
                            }
                        }
                    }
                }
            }
            catch (Exception e) { }
            return checksum;
        }





    }
}
