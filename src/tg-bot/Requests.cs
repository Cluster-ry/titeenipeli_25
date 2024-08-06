using System.Text;
using Newtonsoft.Json;

namespace Titeenipeli_bot
{
    public class Requests
    {   
        private static readonly HttpClient client = new HttpClient();
        
        
        public static async Task<string> CreateUserRequestAsync(string ip, string json) {
            string url = $"{ip}/users";

            try
            {
                // DEBUG
                Console.WriteLine($"Making a POST Request to '{url}' with the following body:\n{json}");
                
                StringContent content = new(JsonConvert.SerializeObject(json), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);

                // DEBUG
                Console.WriteLine($"User Creation response:\n'{response}'");

                response.EnsureSuccessStatusCode();

                string responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: '{e}'");
                return "";
            }
        } 

    }
}