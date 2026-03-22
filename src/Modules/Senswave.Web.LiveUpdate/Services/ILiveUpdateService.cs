using Senswave.Web.LiveUpdate.Models;

namespace Senswave.Web.LiveUpdate.Services;

public interface ILiveUpdateService : IAsyncDisposable
{
    event Action<UpdateEvent>? OnUpdate;

    bool IsConnected { get; }

    Task StartAsync();

    void Trigger(UpdateEvent update);
}
