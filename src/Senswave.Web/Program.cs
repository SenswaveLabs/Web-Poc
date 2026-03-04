using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Senswave.Web;
using Senswave.Web.Integration;
using Senswave.Web.Services;
using Senswave.Web.Services.Shared.Loading;
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
builder.Services.AddScoped<ILoadingService, LoadingService>();

// Modules
builder.Services.AddSenswaveShared();
builder.Services.AddUsers(builder.Configuration);

// Services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<SenswaveAuthenticationProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());
builder.Services.AddScoped<IAuthenticationService>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());
builder.Services.AddScoped<ITokenStore>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());


await builder.Build().RunAsync();
