using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

public class WooCommerceApi
{
    private readonly HttpClient httpClient;

    public WooCommerceApi(Settings settings)
    {
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

        httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(settings.Uri, "/wp-json/wc/v3/") };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes($"{settings.ConsumerKey}:{settings.ConsumerSecret}")));
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint, Dictionary<string, string> parameters)
    {
        var queryString = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return await httpClient.GetAsync($"{endpoint}?{queryString}");
    }

    public async Task<HttpResponseMessage> PostAsync(string endpoint, Dictionary<string, string> parameters, string json)
    {
        var queryString = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        return await httpClient.PostAsync($"{endpoint}?{queryString}", new StringContent(json, Encoding.UTF8, "application/json"));
    }
}