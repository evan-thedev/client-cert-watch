namespace ClientCertWatch;

public class Options
{
    public List<string> Hosts { get; set; } = new();
    public string? InputFile { get; set; }
    public bool ReadFromStdin { get; set; }
    public int DefaultPort { get; set; } = 443;
    public int TimeoutSeconds { get; set; } = 10;
    public int WarnThreshold { get; set; } = 30;
    public int CriticalThreshold { get; set; } = 14;
    public bool JsonOutput { get; set; }
    public bool NoColor { get; set; }
    public bool ShowHelp { get; set; }

    public static Options Parse(string[] args)
    {
        var options = new Options();
        var noColorEnv = Environment.GetEnvironmentVariable("NO_COLOR");
        options.NoColor = !string.IsNullOrEmpty(noColorEnv);

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg == "--help" || arg == "-h")
            {
                options.ShowHelp = true;
                return options;
            }
            else if (arg == "--json")
            {
                options.JsonOutput = true;
            }
            else if (arg == "--no-color")
            {
                options.NoColor = true;
            }
            else if (arg == "--stdin")
            {
                options.ReadFromStdin = true;
            }
            else if (arg == "--file" || arg == "-f")
            {
                if (i + 1 < args.Length)
                {
                    options.InputFile = args[++i];
                }
                else
                {
                    throw new ArgumentException($"Missing value for {arg}");
                }
            }
            else if (arg == "--port" || arg == "-p")
            {
                if (i + 1 < args.Length && int.TryParse(args[++i], out var port))
                {
                    options.DefaultPort = port;
                }
                else
                {
                    throw new ArgumentException($"Invalid port value for {arg}");
                }
            }
            else if (arg == "--timeout" || arg == "-t")
            {
                if (i + 1 < args.Length && int.TryParse(args[++i], out var timeout))
                {
                    options.TimeoutSeconds = timeout;
                }
                else
                {
                    throw new ArgumentException($"Invalid timeout value for {arg}");
                }
            }
            else if (arg == "--warn" || arg == "-w")
            {
                if (i + 1 < args.Length && int.TryParse(args[++i], out var warn))
                {
                    options.WarnThreshold = warn;
                }
                else
                {
                    throw new ArgumentException($"Invalid warn threshold for {arg}");
                }
            }
            else if (arg == "--critical" || arg == "-c")
            {
                if (i + 1 < args.Length && int.TryParse(args[++i], out var critical))
                {
                    options.CriticalThreshold = critical;
                }
                else
                {
                    throw new ArgumentException($"Invalid critical threshold for {arg}");
                }
            }
            else if (!arg.StartsWith('-'))
            {
                options.Hosts.Add(arg);
            }
            else
            {
                throw new ArgumentException($"Unknown option: {arg}");
            }
        }

        return options;
    }

    public static void PrintHelp()
    {
        Console.WriteLine(@"Client Cert Watch - Monitor TLS certificate expiration

USAGE:
  client-cert-watch [OPTIONS] [HOSTNAME...]

OPTIONS:
  -h, --help              Show this help message
  -f, --file FILE         Read hostnames from file (one per line, # for comments)
      --stdin             Read hostnames from stdin
  -p, --port PORT         Default port for connections (default: 443)
  -t, --timeout SECONDS   Connection timeout in seconds (default: 10)
  -w, --warn DAYS         Warning threshold in days (default: 30)
  -c, --critical DAYS     Critical threshold in days (default: 14)
      --json              Output results as JSON
      --no-color          Disable colored output

EXIT CODES:
  0 - All certificates OK (reachable, valid, days > warn threshold)
  1 - At least one warning (days <= warn, > critical)
  2 - At least one critical/expired/unreachable/handshake failure

EXAMPLES:
  client-cert-watch example.com
  client-cert-watch example.com:8443 --warn 60 --critical 30
  client-cert-watch --file domains.txt --json
  echo ""example.com"" | client-cert-watch --stdin

Environment:
  NO_COLOR - If set (non-empty), disables colored output

Author: Evan Parrott
License: MIT
");
    }
}
