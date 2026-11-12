namespace SkyeMusicCompanion.Services
{
    public static class ServerEndpointBuilder
    {
        public static (string TcpHost, int TcpPort, string BaseUrl) Parse(string rawInput, int defaultPort)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return (string.Empty, defaultPort, string.Empty);

            string cleaned = rawInput.Trim();

            if (!cleaned.Contains("://"))
            {
                cleaned = "http://" + cleaned;
            }

            if (Uri.TryCreate(cleaned, UriKind.Absolute, out Uri? uri))
            {
                string scheme = uri.Scheme;
                string host = uri.Host;
                int port = !uri.IsDefaultPort ? uri.Port : (scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : defaultPort);

                // Build clean base URL (e.g., http://10.0.0.211:5050 or https://my-tunnel.trycloudflare.com)
                string baseUrl;
                bool isDefaultHttp = scheme.Equals("http", StringComparison.OrdinalIgnoreCase) && port == 80;
                bool isDefaultHttps = scheme.Equals("https", StringComparison.OrdinalIgnoreCase) && port == 443;

                if (uri.IsDefaultPort || isDefaultHttp || isDefaultHttps)
                {
                    baseUrl = $"{scheme}://{host}";
                }
                else
                {
                    baseUrl = $"{scheme}://{host}:{port}";
                }

                return (host, port, baseUrl);
            }

            return (rawInput.TrimEnd('/'), defaultPort, $"http://{rawInput}");
        }
    }
}