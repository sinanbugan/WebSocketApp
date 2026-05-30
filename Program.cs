using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseWebSockets();

// Servis bazlı yolcu listeleri


ConcurrentDictionary<string, ConcurrentBag<WebSocket>> servicePassengers = new();

app.Map("/ws/driver", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var serviceId = context.Request.Query["serviceId"].ToString();
        using var driverSocket = await context.WebSockets.AcceptWebSocketAsync();
        await HandleDriverAsync(driverSocket, serviceId, servicePassengers);
    }
    else
    {
        Console.WriteLine("/ws/driver: Not a WebSocket request. Request headers:");
        foreach (var h in context.Request.Headers)
        {
            Console.WriteLine($"{h.Key}: {h.Value}");
        }
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("This endpoint accepts only WebSocket upgrade requests.");
    }
});

app.Map("/ws/passenger", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var serviceId = context.Request.Query["serviceId"].ToString();
        var passengerSocket = await context.WebSockets.AcceptWebSocketAsync();

        var passengers = servicePassengers.GetOrAdd(serviceId, _ => new ConcurrentBag<WebSocket>());
        passengers.Add(passengerSocket);

        await HandlePassengerAsync(passengerSocket, serviceId, servicePassengers);
    }
    else
    {
        Console.WriteLine("/ws/passenger: Not a WebSocket request. Request headers:");
        foreach (var h in context.Request.Headers)
        {
            Console.WriteLine($"{h.Key}: {h.Value}");
        }
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("This endpoint accepts only WebSocket upgrade requests.");
    }
});

static async Task HandleDriverAsync(WebSocket driverSocket, string serviceId,
                                    ConcurrentDictionary<string, ConcurrentBag<WebSocket>> servicePassengers)
{
    var buffer = new byte[1024 * 4];
    while (driverSocket.State == WebSocketState.Open)
    {
        var result = await driverSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var locationJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"[{serviceId}] Şoförden konum: {locationJson}");

            if (servicePassengers.TryGetValue(serviceId, out var passengers))
            {
                foreach (var passenger in passengers)
                {
                    if (passenger.State == WebSocketState.Open)
                    {
                        var bytes = Encoding.UTF8.GetBytes(locationJson);
                        await passenger.SendAsync(new ArraySegment<byte>(bytes),
                                                  WebSocketMessageType.Text,
                                                  true,
                                                  CancellationToken.None);
                    }
                }
            }
        }
    }
}

app.Run();

static async Task HandlePassengerAsync(WebSocket passengerSocket, string serviceId,
                                       ConcurrentDictionary<string, ConcurrentBag<WebSocket>> servicePassengers)
{
    var buffer = new byte[1024 * 4];
    while (passengerSocket.State == WebSocketState.Open)
    {
        var result = await passengerSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.MessageType == WebSocketMessageType.Close)
        {
            if (servicePassengers.TryGetValue(serviceId, out var passengers))
            {
                passengers.TryTake(out passengerSocket);
            }
            await passengerSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                             "Yolcu ayrıldı",
                                             CancellationToken.None);
        }
    }
}

