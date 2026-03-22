using Senswave.Web.LiveUpdate.Models;

namespace Senswave.Web.LiveUpdate.Services;

public interface ILiveUpdateService : IAsyncDisposable
{
    event Func<UpdateEvent, Task>? OnUpdate;

    bool IsConnected { get; }

    Task Initialize(string homeId);

    void Trigger(UpdateEvent update);
}
