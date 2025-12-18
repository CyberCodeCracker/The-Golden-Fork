using Blazored.LocalStorage;
using golden_fork.Front.Services;
using golden_fork.Front;
using GoldenFork.Frontend;
using GoldenFork.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API URL
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>()
                  ?? new ApiSettings { BaseUrl = "http://localhost:5128" };

// HttpClient
// Register HttpClient with the auth handler
builder.Services.AddScoped<JwtAuthHandler>();
builder.Services.AddScoped<CartService>();
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
})
.AddHttpMessageHandler<JwtAuthHandler>();

// Default HttpClient uses the handler
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("API"));

// Services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped(_ => apiSettings);

// THIS IS THE ONLY LINE YOU NEED FOR AUTH IN .NET 8 STANDALONE
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();


await builder.Build().RunAsync();