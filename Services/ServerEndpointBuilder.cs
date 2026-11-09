namespace SkyeMusicCompanion.Services;

public static class ServerEndpointBuilder
{
    public static (string Endpoint, int Port) Parse(string rawInput, int defaultPort)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return (string.Empty, defaultPort);

        string cleaned = rawInput.Trim();

        // 1. Handle explicit full URLs (http://, https://) like Cloudflare tunnels
        if (cleaned.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            cleaned.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(cleaned, UriKind.Absolute, out Uri? uri))
            {
                // Extract explicit port if specified in the URL (e.g. https://test.com:5050)
                int urlPort = !uri.IsDefaultPort ? uri.Port : (uri.Scheme == Uri.UriSchemeHttps ? 443 : 80);

                // Return clean base scheme and host (stripping path/port for the clean endpoint)
                string cleanBase = $"{uri.Scheme}://{uri.Host}";
                return (cleanBase, urlPort);
            }
        }

        // 2. Handle raw IPs or domains with or without inline ports (e.g. 10.0.0.211:5050)
        string candidate = "http://" + cleaned;

        if (Uri.TryCreate(candidate, UriKind.Absolute, out Uri? parsedUri))
        {
            string host = parsedUri.Host;
            int port = !parsedUri.IsDefaultPort ? parsedUri.Port : defaultPort;

            // Reconstruct a clean host string (or IP:port if you prefer keeping the port in the endpoint view)
            string endpoint = parsedUri.IsDefaultPort ? host : $"{host}:{port}";

            return ($"http://{endpoint}", port);
        }

        return (rawInput.TrimEnd('/'), defaultPort);
    }
}