using Optimizely.CMS.Labs.LiquidTemplating.CLI.Models;
using RestSharp;

namespace Optimizely.CMS.Labs.LiquidTemplating.CLI;

public class ContentApiAuthenticator
{
    private readonly string _hostname;
    private readonly string _clientId;
    private readonly string _clientSecret;
    protected readonly RestClient Client;

    private const string _baseRoute = "/api/episerver";

    private string? _activeToken;
    private DateTime _expires;

    public ContentApiAuthenticator(string hostname, string clientId, string clientSecret)
    {
        _hostname = hostname;
        _clientId = clientId;
        _clientSecret = clientSecret;
        Client = new RestClient(hostname + _baseRoute);
    }

    public string GetAuthorizationToken()
    {
        if (_activeToken == null || DateTime.Now > _expires)
        {
            string resource = "/connect/token/";
            var request = new RestRequest(resource, Method.Post);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("client_id", _clientId);
            request.AddParameter("client_secret", _clientSecret);

            var response = Client.Execute<TokenResponse>(request);
            var tokenResponse = response.Data;

            if (tokenResponse != null)
            {
                _activeToken = tokenResponse.access_token;
                _expires = DateTime.Now.AddSeconds(tokenResponse.expires_in);
            }
            else
            {
                throw new Exception("Could not retrieve Authentication token");
            }
        }

        return _activeToken;
    }
}