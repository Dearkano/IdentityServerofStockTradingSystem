using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IdentityServerofStockTradingSystem.Controllers
{
    public class TResponse
    {
        public string Id { get; set; }
        public string Account_id { get; set; }
        public char Account_type { get; set; }
        public double Balance_available { get; set; }
        public double Balance_unavailabble { get; set; }
        public string Person_id { get; set; }
        public string Name { get; set; }
        public char Sex { get; set; }
        public string Phone_number { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
    }
    public class Utility
    {
        public static async Task<TResponse> GetIdentity(string access_token)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://111.231.75.113:5001/identity"),
                Method = HttpMethod.Get,
            };
            request.Headers.Add("Authorization", access_token);
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            TResponse res = JsonConvert.DeserializeObject<TResponse>(json);
            return res;
        }
    }
}
