using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Senswave.Web.LiveUpdate.Models;
using Senswave.Web.Shared.Services;

namespace Senswave.Web.LiveUpdate.Services;

internal sealed class LiveUpdateService(
    IConfiguration configuration,
    ILogger<LiveUpdateService> logger,
    NavigationManager navigation, 
    ITokenStore tokenStore) : ILiveUpdateService
{
    private HubConnection? _connection;

    public event Func<UpdateEvent, Task>? OnUpdate;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task Initialize(string homeId)
    {
        if (_connection == null || _connection.State == HubConnectionState.Disconnected)
        {
            var hubUrl = Path.Combine(configuration["Api:BaseUrl"]!, "signalr/liveupdates/live");
            var builder = new HubConnectionBuilder();

            _connection = builder
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = async () =>
                    {
                        var token = await tokenStore.GetBearerToken();
                        return token.Value;
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.Remove("Update");

            _connection.On<string, string>("Update", (typeStr, payload) =>
            {
                if (Enum.TryParse<UpdateType>(typeStr, out var type))
                {
                    OnUpdate?.Invoke(new UpdateEvent
                    {
                        Type = type,
                        Payload = payload
                    });
                }
            });

            try
            {
                await _connection.StartAsync();
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, "Failed to start to update");
            }
        }

        if (_connection.State != HubConnectionState.Connected)
        {
            return;
        }

        await _connection.SendAsync("Initialize", homeId);
    }

    public void Trigger(UpdateEvent update)
    {
        OnUpdate?.Invoke(update);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
