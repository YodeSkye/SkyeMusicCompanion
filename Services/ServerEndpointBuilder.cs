namespace SkyeMusicCompanion.Services;

public static class ServerEndpointBuilder
{
    public static string BuildEndpoint(string rawInput, int defaultPort)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return string.Empty;

        string cleaned = rawInput.Trim();

        // 1. Preserve explicit schemes (http://, https://) and path routing
        if (cleaned.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            cleaned.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return cleaned.TrimEnd('/');
        }

        // 2. Default to http:// for raw IPs or domains
        string candidate = "http://" + cleaned;

        if (Uri.TryCreate(candidate, UriKind.Absolute, out Uri? parsedUri))
        {
            // 3. Fall back to the user's current settings port if no explicit port or path was given
            if (parsedUri.IsDefaultPort && string.IsNullOrEmpty(parsedUri.AbsolutePath) || parsedUri.AbsolutePath == "/")
            {
                var builder = new UriBuilder(parsedUri)
                {
                    Port = defaultPort
                };
                return builder.Uri.ToString().TrimEnd('/');
            }

            return parsedUri.ToString().TrimEnd('/');
        }

        return rawInput.TrimEnd('/');
    }
}