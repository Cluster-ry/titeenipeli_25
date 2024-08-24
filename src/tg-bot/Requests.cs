using System.Net.Http.Headers;
using System.Text;

namespace Titeenipeli_bot;

public static class Requests
{
    private readonly static HttpClient Client = new HttpClient();

    public async static Task CreateUserRequestAsync(string uri, string json)
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

            string? cookies = response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
                ? values.FirstOrDefault()
                : null;
            if (cookies == null)
            {
                throw new Exception("Unable to get cookies.");
            }

        }
        catch (Exception e)
        {
            Console.WriteLine($"exception on CreateUser: '{e}'");
            throw;
        }
    }

    public static HttpRequestHeaders GetHeaders()
    {
        return Client.DefaultRequestHeaders;
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
            // api gives a new jwt token once the guild has been set 
            string? cookies = response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
                ? values.FirstOrDefault()
                : null;
            if (cookies == null)
            {
                throw new Exception("Unable to get cookies.");
            }

        }
        catch (Exception e)
        {
            Console.WriteLine($"exception on SetGuild: '{e}'");
            throw;
        }
    }
}