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

Deployment
----------

1) Yerel olarak publish (Release):

```bash
cd /Users/sinanbgn/Desktop/WebSocketApp
dotnet restore
dotnet publish -c Release -f net10.0 -o ./publish
```

2) Sunucuya yükleme (örnek SCP):

```bash
scp WebSocketApp-publish.zip user@your.server:/path/to/deploy/
```

3) Sunucuda açma ve çalıştırma:

```bash
ssh user@your.server
unzip WebSocketApp-publish.zip -d /opt/WebSocketApp
cd /opt/WebSocketApp/publish
# Framework-dependent ise:
dotnet WebSocketApp.dll
# Veya self-contained binary varsa:
./WebSocketApp
```

4) systemd ile servis (örnek): `/etc/systemd/system/websocketapp.service`

```ini
[Unit]
Description=WebSocketApp
After=network.target

[Service]
WorkingDirectory=/opt/WebSocketApp/publish
ExecStart=/usr/bin/dotnet /opt/WebSocketApp/publish/WebSocketApp.dll
Restart=always
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

Sonra servisleri yeniden yükleyip etkinleştirin:

```bash
sudo systemctl daemon-reload
sudo systemctl enable --now websocketapp
```

Notlar:
- `WebSocketApp-publish.zip` paketini ben `publish` klasörünü zipleyerek oluşturdum.
- Eğer farklı bir framework (`net8.0` vs) hedefliyorsanız `dotnet publish` komutundaki `-f` parametresini değiştirin.

