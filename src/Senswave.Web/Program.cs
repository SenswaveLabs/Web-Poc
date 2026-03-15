using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Senswave.Web;
using Senswave.Web.Devices;
using Senswave.Web.DataSources;
using Senswave.Web.Homes.Services;
using Senswave.Web.Integration;
using Senswave.Web.Services;
using Senswave.Web.Services.Shared;
using Senswave.Web.Shared.Extensions;
using Senswave.Web.Shared.Services;
using Senswave.Web.Themes;
using Senswave.Web.Users;
using Senswave.Web.Users.Auth.Services;
using Senswave.Web.Users.Users.Services;
using Senswave.Web.Devices.Services;

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
builder.Services.AddSingleton<IThemeService, ThemeService>();

// Modules - Users
builder.Services.AddUsers(builder.Configuration);
builder.Services.AddSingleton<IUserService, UserService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<SenswaveAuthenticationProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());
builder.Services.AddSingleton<IAuthenticationService>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());
builder.Services.AddSingleton<ITokenStore>(sp => sp.GetRequiredService<SenswaveAuthenticationProvider>());

// Modules - Homes
builder.Services.AddSingleton<IHomeService, HomeService>();
builder.Services.AddSingleton<IRoomService, RoomService>();

// Modules - Data Sources
builder.Services.AddDataSources(builder.Configuration);

// Modules - Devices
builder.Services.AddDevices(builder.Configuration);
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddSingleton<IDeviceListService>(sp => sp.GetRequiredService<DeviceService>());




await builder.Build().RunAsync();
