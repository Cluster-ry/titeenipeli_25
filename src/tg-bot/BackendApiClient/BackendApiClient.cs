using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TiteenipeliBot.BackendApiClient.Inputs;
using TiteenipeliBot.BackendApiClient.Results;

namespace TiteenipeliBot.BackendApiClient;

public class BackendClient(BackendOptions backendOptions)
{
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    private readonly HttpClient _httpClient = new();
    private readonly BackendOptions _backendOptions = backendOptions;

    public async Task<string?> CreateUserOrLoginRequest(PostUsersInput userInput)
    {
        string url = $"{_backendOptions.Url}/api/v1/users";
        string json = JsonConvert.SerializeObject(userInput, _serializerSettings);

        try
        {
            StringContent content = new(json, Encoding.UTF8, "application/json");
            content.Headers.Add(_backendOptions.AuthorizationHeaderName, _backendOptions.Token);
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                string body = await response.Content.ReadAsStringAsync();
                ErrorResult? errorResult =
                    JsonConvert.DeserializeObject<ErrorResult>(body) ?? throw new Exception("Unable to deserialize error results.");
                if (errorResult.Code == 1414)
                {
                    return null;
                }
            }
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            PostUsersResult? postUsersResult =
                JsonConvert.DeserializeObject<PostUsersResult>(responseBody) ?? throw new Exception("Unable to deserialize results.");
            return postUsersResult.Token;
        }
        catch (Exception e)
        {
            Console.WriteLine($"exception on CreateUser: '{e}'");
            throw;
        }
    }
}