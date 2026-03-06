using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Senswave.Web;
using Senswave.Web.Homes.Services;
using Senswave.Web.Integration;
using Senswave.Web.Services;
using Senswave.Web.Services.Homes;
using Senswave.Web.Services.Shared;
using Senswave.Web.Shared.Extensions;
using Senswave.Web.Shared.Services;
using Senswave.Web.Users;
using Senswave.Web.Users.Auth.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Root components
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Libraries
builder.Services.AddMudServices();

// Shared
builder.Services.AddSenswaveIntegration(builder.Configuration);
builder.Services.AddSingleton<ILoadingService, LoadingService>();
builder.Services.AddTransient<ILocalStorageService, LocalStorageService>();
builder.Services.AddTransient<ISessionStorageService, SessionStorageService>();
builder.Services.AddSenswaveShared();

// Modules
builder.Services.AddUsers(builder.Configuration);

// Services
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<SenswaveAuthenticationProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());
builder.Services.AddSingleton<IAuthenticationService>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());
builder.Services.AddSingleton<ITokenStore>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());
builder.Services.AddSingleton<IHomeService, HomeService>();



await builder.Build().RunAsync();
