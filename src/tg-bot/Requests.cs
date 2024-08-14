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

            SetCookies(cookies);
        }
        catch (Exception e)
        {
            Console.WriteLine($"exception on CreateUser: '{e}'");
            throw;
        }
    }

    private static void SetCookies(string cookies)
    {
        Client.DefaultRequestHeaders.Clear();
        string[] cookieList = cookies.Split("; "); // this is ugly I know, couldn't find a better way to parse
        foreach (string cookie in cookieList)
        {
            string[] cookieSplit = cookie.Split('='); // again, I know
            if (!cookieSplit[0].Equals("X-Authorization")) continue;
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cookieSplit[1]);
            return;
        }
    }

    public static AuthenticationHeaderValue? GetCookies()
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
            // api gives a new jwt token once the guild has been set 
            string? cookies = response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
                ? values.FirstOrDefault()
                : null;
            if (cookies == null)
            {
                throw new Exception("Unable to get cookies.");
            }

            SetCookies(cookies);
        }
        catch (Exception e)
        {
            Console.WriteLine($"exception on SetGuild: '{e}'");
            throw;
        }
    }
}