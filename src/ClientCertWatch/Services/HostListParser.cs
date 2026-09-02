namespace ClientCertWatch.Services;

public class HostListParser
{
    public static List<(string Hostname, int Port)> ParseHostList(IEnumerable<string> lines, int defaultPort = 443)
    {
        var hosts = new List<(string, int)>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                continue;
            }

            var parts = trimmed.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            var hostname = parts[0].Trim();
            var port = defaultPort;

            if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out var parsedPort))
            {
                port = parsedPort;
            }

            if (!string.IsNullOrWhiteSpace(hostname))
            {
                hosts.Add((hostname, port));
            }
        }

        return hosts;
    }

    public static List<(string Hostname, int Port)> ParseFromFile(string filePath, int defaultPort = 443)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);
        return ParseHostList(lines, defaultPort);
    }

    public static async Task<List<(string Hostname, int Port)>> ParseFromStdinAsync(int defaultPort = 443)
    {
        var lines = new List<string>();
        string? line;
        while ((line = await Console.In.ReadLineAsync()) != null)
        {
            lines.Add(line);
        }
        return ParseHostList(lines, defaultPort);
    }
}
