using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Titeenipeli_bot;

public static class Requests
{
    private readonly static HttpClient Client = new HttpClient();

    public static async Task<int> CreateUserRequestAsync(string host, string json)
    {
        string url = $"{host}/api/v1/users";

        try
        {
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync(url, content);

            //DEBUG
            /*
            Console.WriteLine($"Making a POST Request to '{url}' with the following body:\n{await content.ReadAsStringAsync()}");
            Console.WriteLine($"\nResponse:\n'{response}'");
            */

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Dictionary<string, string>? bodyJson =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

            // Console.WriteLine($"body: '{bodyJson}'\nguild: '{bodyJson["guild"]}'");
            return bodyJson?["guild"] != null ? 0 : 1;
        }
        catch (Exception e)
        {
            Console.WriteLine($"exception on CreateUser: '{e}'");
            throw;
        }
    }

    public static AuthenticationHeaderValue? GetAuthHeader()
    {
        return Client.DefaultRequestHeaders.Authorization;
    }
}