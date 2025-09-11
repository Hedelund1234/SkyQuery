using Microsoft.AspNetCore.SignalR;
using SkyQuery.AppGateway.Application.Interfaces;

namespace SkyQuery.AppGateway.Messaging
{
    public class ImageAvailableHandler
    {
        //private readonly IImageStore _store;
        //private readonly IHubContext<SkyQueryImageHub> _hub;

        //public ImageAvailableHandler(IImageStore store, IHubContext<SkyQueryImageHub> hub)
        //{
        //    _store = store; _hub = hub;
        //}

        //public async Task HandleAsync(ImageAvailable evt, CancellationToken ct = default)
        //{
        //    await _store.PutAsync(evt.Id, evt.Bytes, evt.ContentType, evt.FileName, ct);

        //    await _hub.Clients.All.SendAsync("imageAvailable", new
        //    {
        //        id = evt.Id,
        //        fileName = evt.FileName,
        //        contentType = evt.ContentType,
        //        size = evt.Bytes?.Length
        //    }, ct);
        //}
    }
}
