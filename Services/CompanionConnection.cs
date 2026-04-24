
using System.Net.Sockets;
using SkyeMusicCompanion.Helpers;

namespace SkyeMusicCompanion.Services
{
    public class CompanionConnection
    {
        private TcpClient? _client;
        private StreamWriter? _writer;
        private CancellationTokenSource? _cts;

        public bool IsConnected => _client != null && _client.Connected;

        public event Action? Connected;
        public event Action? Disconnected;
        public event Action? ServerClosing;
        public event Action<string>? NowPlayingReceived;
        public event Action<string>? PlaylistReceived;
        public event Action<string>? StreamInfoReceived;
        public event Action<string>? UnknownMessageReceived;
        public event Action<string>? VolumeReceived;
        public event Action<string>? MuteReceived;

        public async Task ConnectAsync(string host, int port)
        {
            try
            {
                _cts = new CancellationTokenSource();

                _client = new TcpClient();
                await _client.ConnectAsync(host, port);
                Connected?.Invoke();
                ScreenBehaviorManager.ApplyBehavior();

                _writer = new StreamWriter(_client.GetStream())
                {
                    AutoFlush = true
                };

                _ = SendHelloAsync();

                // Start listening for server messages
                _ = Task.Run(() => ListenLoop(_cts.Token));

                // Ask the server for the current state
                RequestNowPlaying();
                RequestVolume();
                RequestMute();
            }
            catch (Exception ex)
            {
                Log.Write("Connection error: " + ex.Message);
            }
        }
        public async Task ReconnectAsync()
        {
            try
            {
                Disconnect();
                await ConnectAsync(Settings.HostServerIp, Settings.HostServerPort);
            }
            catch (Exception ex)
            {
                Log.Write("Reconnect failed: " + ex.Message);
            }
        }
        public void Disconnect()
        {
            try { _cts?.Cancel(); } catch { }
            try { _client?.Close(); } catch { }
            Disconnected?.Invoke();
            ScreenBehaviorManager.ResetToSystem();

            _cts = null;
            _client = null;
            _writer = null;
        }
        private async Task ListenLoop(CancellationToken token)
        {
            try
            {
                using var reader = new StreamReader(_client!.GetStream());

                while (!token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(token);
                    if (line == null)
                    {
                        Log.Write("Client disconnected (EOF).");
                        Disconnected?.Invoke();
                        ScreenBehaviorManager.ResetToSystem();
                        return;
                    }
                    // Split once to detect message type
                    var parts = line.Split('|');
                    var type = parts[0].ToUpperInvariant();

                    switch (type)
                    {
                        case "NOWPLAYING":
                            NowPlayingReceived?.Invoke(line);
                            break;
                        case "PLAYLIST":
                            PlaylistReceived?.Invoke(line);
                            break;
                        case "STREAMINFO":
                            StreamInfoReceived?.Invoke(line);
                            break;
                        case "VOL":
                            VolumeReceived?.Invoke(line);
                            break;
                        case "MUTE":
                            MuteReceived?.Invoke(line);
                            break;
                        case "SERVERCLOSING":
                            ServerClosing?.Invoke();
                            break;
                        default:
                            UnknownMessageReceived?.Invoke(type);
                            break;
                    }
                    // Safe, generic log
                    Log.Write("NETWORK PAYLOAD RECEIVED (" + type + ")");
                }
            }
            catch (Exception ex)
            {
                Log.Write("Listen error: " + ex.Message);
                Disconnected?.Invoke();
                ScreenBehaviorManager.ResetToSystem();
            }
        }
        public async Task SendHelloAsync()
        {
            if (_writer == null)
                return;

            string deviceName = DeviceInfoHelper.GetDeviceName();
            string message = $"hello|{deviceName}";

            try
            {
                await _writer.WriteLineAsync(message);
                Log.Write("Sent HELLO with device name: " + deviceName);
            }
            catch (Exception ex)
            {
                Log.Write("SendHelloAsync error: " + ex.Message);
            }
        }
        public void RequestVolume()
        {
            SendCommand("vol");
        }
        public void SetVolume(int percent)
        {
            SendCommand($"volset|{percent}");
        }
        public void RequestMute()
        {
            SendCommand("mute");
        }
        public void SetMute(bool value)
        {
            SendCommand($"muteset|{value.ToString().ToLower()}");
        }
        public void SendCommand(string cmd)
        {
            try
            {
                _writer?.WriteLine(cmd);
                Log.Write("Sent command: " + cmd);
            }
            catch (Exception ex)
            {
                Log.Write("Send error: " + ex.Message);
                Disconnected?.Invoke();
                ScreenBehaviorManager.ResetToSystem();
            }
        }
        public void RequestNowPlaying()
        {
            SendCommand("NOWPLAYING");
        }
    }
}
