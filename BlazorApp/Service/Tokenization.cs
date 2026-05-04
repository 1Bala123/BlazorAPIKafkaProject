using System.Net.Http.Headers;


public class TokenService
{
    private readonly HttpClient _httpClient;
    private string _token;
    private DateTime _expiry;

    public TokenService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InitializeAsync()
    {
        var response = await _httpClient.PostAsJsonAsync("api/products/GenerateAPIValidationToken", new { username = "user", password = "pass" });
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _token = result.Token;
        _expiry = result.Expiry;
    }

    public async Task<string> GetTokenAsync()
    {
        if (DateTime.UtcNow >= _expiry)
        {
            var refreshResponse = await _httpClient.PostAsync("api/products/GenerateAPIValidationToken", null);
            var result = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
            _token = result.Token;
            _expiry = result.Expiry;
        }
        return _token;
    }
}

public class AuthMessageHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;

    public AuthMessageHandler(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenService.GetTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}

public class AuthResponse
{
    public string Token { get; set; }
    public DateTime Expiry { get; set; }
}
