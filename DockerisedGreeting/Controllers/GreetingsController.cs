// Ignore Spelling: Ip

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace DockerisedGreeting.Controllers;
[Route("api/[controller]")]
[ApiController]
public class GreetingsController : ControllerBase
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _config;

    public GreetingsController(IHttpClientFactory factory, IConfiguration config)
    {
        _factory = factory;
        _config = config;
    }
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? name)
    {
        var client = _factory.CreateClient("client");
        var visitor_name = name is null ? "client" : name;
        var temperature = 0.0;

        //Configuration settings
        var ipInfoToken = _config.GetValue<string>(
            "IpInfoSettings:Token");
        var ipInfoUrlConfig = _config.GetValue<string>(
            "IpInfoSettings:Url");
        var weatherApiKey = _config.GetValue<string>(
            "WeatherSettings:Api");
        var weatherUrlConfig = _config.GetValue<string>(
            "WeatherSettings:Url");

        // Get the visitor's IP address
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        // Use the IP address to get the visitor's location
        var ipInfoUrl = $"{ipInfoUrlConfig}{clientIp}?token={ipInfoToken}";
        var ipInfoResponse = await client.GetFromJsonAsync<IpInfoResponse>(ipInfoUrl);

        if (ipInfoResponse is null)
            return BadRequest("Unable to get client IP");

        var city = ipInfoResponse.City;
        if (city is null)
            return BadRequest("Unable to get city.");

        // Get temperature for the city
        var weatherUrl = $"{weatherUrlConfig}?q={city}&units=metric&appid={weatherApiKey}";
        var weatherResponse = await client
            .GetFromJsonAsync<WeatherResponse>(weatherUrl);

        if (weatherResponse?.Main?.Temp is not null)
        {
            temperature = weatherResponse.Main.Temp;
        }

        var response = new
        {
            client_ip = clientIp,
            location = city,
            greeting = $"Hello, {visitor_name}! The temperature is {temperature} degrees Celsius in {city}"
        };

        return Ok(response);
    }

    public record IpInfoResponse(string? City);
    public record WeatherResponse(Main? Main);
    public record Main(double Temp);
}
