using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Bwasm.Cookie.Auth;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Bwasm.Cookie.Providers;
using Bwasm.Cookie.Handler;
using Bwasm.Cookie.Logics;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();

builder.Services
.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddScoped<CookieHandler>();

builder.Services.AddHttpClient("API", options => {
    options.BaseAddress = new Uri("https://localhost:7235/");
})
.AddHttpMessageHandler<CookieHandler>();

builder.Services.AddScoped<IApiLogic, ApiLogic>();

await builder.Build().RunAsync();
