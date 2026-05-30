WebSocketApp - Minimal WebSocket broadcast demo (driver -> passengers)

Files created in this folder:

- `WebSocketApp.csproj`
- `Program.cs`
- `wwwroot/index.html` (test client)

Run locally:

```bash
# change to project folder
cd /Users/sinanbgn/Desktop/WebSocketApp

# run the app
dotnet run
```

Open the address printed by `dotnet run` (typically http://localhost:5000 or https://localhost:5001). The sample client is available at `/`.

Server endpoints:
- `ws://<host>/ws/driver`  : driver sends JSON locations
- `ws://<host>/ws/passenger`: passenger receives driver updates
