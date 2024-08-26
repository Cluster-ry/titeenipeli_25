using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Titeenipeli_bot;

public static class Requests
{
    private readonly static HttpClient Client = new HttpClient();

    public async static Task<int> CreateUserRequestAsync(string uri, string json)
    {
        string url = $"{uri}/api/v1/users";

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
            return bodyJson["guild"] != null ? 0 : 1;
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

    public async static Task SetGuildRequestAsync(string ip, string json)
    {
        string url = $"{ip}/api/v1/users";

        try
        {
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PutAsync(url, content);

            //DEBUG
            /*
            Console.WriteLine($"Made the following PUT Request:\n'{response.RequestMessage}'");
            Console.WriteLine($"\nContent:\n{await content.ReadAsStringAsync()}");
            Console.WriteLine($"\nResponse:\n'{response}'");
             */

            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            Console.WriteLine($"exception on SetGuild: '{e}'");
            throw;
        }
    }
}