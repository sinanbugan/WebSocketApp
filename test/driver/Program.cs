using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

async Task RunDriver()
{
	using var ws = new ClientWebSocket();
	var uri = new Uri("ws://localhost:5000/ws/driver?serviceId=servis1");
	Console.WriteLine($"Connecting to {uri}...");
	await ws.ConnectAsync(uri, CancellationToken.None);
	Console.WriteLine("Connected. Sending 10 location messages...");

	for (int i = 0; i < 10; i++)
	{
		var json = $"{{\"lat\":{37.0 + i * 0.001},\"lon\":{27.0 + i * 0.001}}}";
		var bytes = Encoding.UTF8.GetBytes(json);
		await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
		Console.WriteLine("Sent: " + json);
		await Task.Delay(1000);
	}

	await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
	Console.WriteLine("Driver finished.");
}

await RunDriver();
