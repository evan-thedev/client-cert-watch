using ClientCertWatch;
using ClientCertWatch.Models;
using ClientCertWatch.Services;

try
{
    var options = Options.Parse(args);

    if (options.ShowHelp)
    {
        Options.PrintHelp();
        return 0;
    }

    var hostsToParse = new List<string>();
    hostsToParse.AddRange(options.Hosts);

    if (options.InputFile != null)
    {
        var fileHosts = HostListParser.ParseFromFile(options.InputFile, options.DefaultPort);
        foreach (var (hostname, port) in fileHosts)
        {
            hostsToParse.Add($"{hostname}:{port}");
        }
    }

    if (options.ReadFromStdin || (!hostsToParse.Any() && Console.IsInputRedirected))
    {
        var stdinHosts = await HostListParser.ParseFromStdinAsync(options.DefaultPort);
        foreach (var (hostname, port) in stdinHosts)
        {
            hostsToParse.Add($"{hostname}:{port}");
        }
    }

    if (!hostsToParse.Any())
    {
        Console.Error.WriteLine("Error: No hostnames provided. Use --help for usage information.");
        return 2;
    }

    var hostsToCheck = HostListParser.ParseHostList(hostsToParse, options.DefaultPort);
    
    if (!hostsToCheck.Any())
    {
        Console.Error.WriteLine("Error: No valid hostnames found.");
        return 2;
    }

    var checker = new TlsChecker();
    var results = new List<CertificateResult>();

    foreach (var (hostname, port) in hostsToCheck)
    {
        var result = await checker.CheckCertificateAsync(hostname, port, options.TimeoutSeconds);
        results.Add(result);
    }

    var useColors = !options.NoColor && !options.JsonOutput && !Console.IsOutputRedirected;
    var formatter = new OutputFormatter(useColors, options.WarnThreshold, options.CriticalThreshold);

    if (options.JsonOutput)
    {
        var json = formatter.ToJson(results);
        Console.WriteLine(json);
    }
    else
    {
        formatter.PrintTable(results);
    }

    var maxSeverity = results.Max(r => r.GetSeverity(options.WarnThreshold, options.CriticalThreshold));
    return (int)maxSeverity;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine("Use --help for usage information.");
    return 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex.Message}");
    return 2;
}
