using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

async Task RunPassenger()
{
	using var ws = new ClientWebSocket();
	var uri = new Uri("ws://localhost:5000/ws/passenger?serviceId=servis1");
	Console.WriteLine($"Connecting to {uri}...");
	await ws.ConnectAsync(uri, CancellationToken.None);
	Console.WriteLine("Connected. Waiting for location messages...");

	var buffer = new byte[1024 * 4];
	while (ws.State == WebSocketState.Open)
	{
		var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
		if (result.MessageType == WebSocketMessageType.Text)
		{
			var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
			Console.WriteLine("Received: " + msg);
		}
		else if (result.MessageType == WebSocketMessageType.Close)
		{
			Console.WriteLine("Closed by server");
			break;
		}
	}
}

await RunPassenger();
