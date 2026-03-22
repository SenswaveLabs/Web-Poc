
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Senswave.Web.LiveUpdate.Models;

namespace Senswave.Web.LiveUpdate.Services;

internal sealed class LiveUpdateService(NavigationManager navigation) : ILiveUpdateService
{
    private HubConnection? _connection;

    public event Action<UpdateEvent>? OnUpdate;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    public async Task StartAsync()
    {
        if (_connection != null)
            return;

        var hubUrl = navigation.ToAbsoluteUri("/signalr/liveupdates/live");
        var builder = new HubConnectionBuilder();

        _connection = builder
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

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

        await _connection.StartAsync();
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
