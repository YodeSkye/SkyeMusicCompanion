namespace SkyeMusicCompanion.Services
{
    public static class ServerEndpointBuilder
    {
        public static (string Host, int Port) Parse(string rawInput, int defaultPort)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return (string.Empty, defaultPort);

            string cleaned = rawInput.Trim().TrimEnd('/');
            if (cleaned.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[7..];
            else if (cleaned.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned[8..];

            string host = cleaned;
            int port = defaultPort;

            int colonIndex = cleaned.LastIndexOf(':');
            if (colonIndex > 0 && int.TryParse(cleaned.AsSpan(colonIndex + 1), out int parsedPort))
            {
                host = cleaned[..colonIndex];
                port = parsedPort;
            }

            return (host, port);
        }
    }
}