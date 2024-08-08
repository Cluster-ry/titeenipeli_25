using System.Net.Http.Headers;
using System.Text;

namespace Titeenipeli_bot
{
    public static class Requests
    {   
        private static readonly HttpClient Client = new HttpClient();
        
        public static async Task CreateUserRequestAsync(string ip, string json) 
        {
            string url = $"{ip}/users";

            try
            {
                StringContent content = new(json, Encoding.UTF8, "application/json");
                Console.WriteLine($"Making a POST Request to '{url}' with the following body:\n{await content.ReadAsStringAsync()}"); // DEBUG

                HttpResponseMessage response = await Client.PostAsync(url, content);
                Console.WriteLine($"\nResponse:\n'{response}'"); // DEBUG

                response.EnsureSuccessStatusCode();

                string? cookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.FirstOrDefault() : null;
                if (cookies == null) {
                    throw new Exception("Unable to get cookies.");
                }

                SetCookies(cookies);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: '{e}'");
                return;
            }
        }

        private static void SetCookies(string cookies)
        {
            Client.DefaultRequestHeaders.Clear();
            string[] cookieList = cookies.Split("; "); // this is ugly I know
            foreach (string cookie in cookieList)
            {
                string[] cookieSplit = cookie.Split('=');
                if (!cookieSplit[0].Equals("X-Authorization")) continue;
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cookieSplit[1]);
                return;
            }
        }

        public static async Task SetGuildRequestAsync(string ip, string json)
        {
            string url = $"{ip}/users";

            try
            {
                StringContent content = new(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await Client.PutAsync(url, content);
                
                Console.WriteLine($"Made the following PUT Request:\n'{response.RequestMessage}'"); // DEBUG
                Console.WriteLine($"\nContent:\n{await content.ReadAsStringAsync()}");
                Console.WriteLine($"\nResponse:\n'{response}'"); // DEBUG

                response.EnsureSuccessStatusCode();
                // api givces a new jwt token once the guild has been set 
                string? cookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.FirstOrDefault() : null;
                if (cookies == null) {
                    throw new Exception("Unable to get cookies.");
                }

                SetCookies(cookies);
            }
            catch (Exception e)
            {
                Console.WriteLine($"exception: '{e}'");
                return;
            }
        }
    }
}