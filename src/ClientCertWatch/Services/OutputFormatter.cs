using System.Text.Json;
using ClientCertWatch.Models;

namespace ClientCertWatch.Services;

public class OutputFormatter
{
    private readonly bool _useColors;
    private readonly int _warnThreshold;
    private readonly int _criticalThreshold;

    public OutputFormatter(bool useColors, int warnThreshold, int criticalThreshold)
    {
        _useColors = useColors;
        _warnThreshold = warnThreshold;
        _criticalThreshold = criticalThreshold;
    }

    public void PrintTable(List<CertificateResult> results)
    {
        Console.WriteLine();
        PrintRow("HOSTNAME", "PORT", "REACHABLE", "TLS OK", "DAYS LEFT", "STATUS", isHeader: true);
        Console.WriteLine(new string('-', 100));

        foreach (var result in results)
        {
            var severity = result.GetSeverity(_warnThreshold, _criticalThreshold);
            var hostPort = $"{result.Hostname}:{result.Port}";
            var reachable = result.IsReachable ? "Yes" : "No";
            var tlsOk = result.TlsHandshakeSuccess ? "Yes" : "No";
            var daysLeft = result.DaysRemaining?.ToString() ?? "N/A";
            var status = GetStatusText(result, severity);

            if (_useColors)
            {
                var color = GetColorForSeverity(severity);
                Console.Write($"{hostPort,-30} {result.Port,-6} {reachable,-10} {tlsOk,-8} ");
                Console.Write($"{color}{daysLeft,-10}{AnsiColors.Reset} ");
                Console.WriteLine($"{color}{status}{AnsiColors.Reset}");
            }
            else
            {
                PrintRow(hostPort, result.Port.ToString(), reachable, tlsOk, daysLeft, status);
            }

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                var errorColor = _useColors ? AnsiColors.Red : "";
                var resetColor = _useColors ? AnsiColors.Reset : "";
                Console.WriteLine($"  {errorColor}└─ {result.ErrorMessage}{resetColor}");
            }
        }
        Console.WriteLine();
    }

    public string ToJson(List<CertificateResult> results)
    {
        var jsonResults = results.Select(r => new
        {
            hostname = r.Hostname,
            port = r.Port,
            isReachable = r.IsReachable,
            tlsHandshakeSuccess = r.TlsHandshakeSuccess,
            subject = r.Subject,
            issuer = r.Issuer,
            notBefore = r.NotBefore?.ToString("O"),
            notAfter = r.NotAfter?.ToString("O"),
            daysRemaining = r.DaysRemaining,
            severity = r.GetSeverity(_warnThreshold, _criticalThreshold).ToString().ToLower(),
            errorMessage = r.ErrorMessage
        }).ToList();

        return JsonSerializer.Serialize(jsonResults, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private void PrintRow(string col1, string col2, string col3, string col4, string col5, string col6, bool isHeader = false)
    {
        Console.WriteLine($"{col1,-30} {col2,-6} {col3,-10} {col4,-8} {col5,-10} {col6}");
    }

    private string GetStatusText(CertificateResult result, Severity severity)
    {
        if (!result.IsReachable)
            return "UNREACHABLE";
        if (!result.TlsHandshakeSuccess)
            return "TLS FAILED";
        if (!result.DaysRemaining.HasValue)
            return "NO CERT";
        if (result.DaysRemaining.Value < 0)
            return "EXPIRED";
        if (severity == Severity.Critical)
            return "CRITICAL";
        if (severity == Severity.Warning)
            return "WARNING";
        return "OK";
    }

    private string GetColorForSeverity(Severity severity)
    {
        return severity switch
        {
            Severity.Ok => AnsiColors.Green,
            Severity.Warning => AnsiColors.Yellow,
            Severity.Critical => AnsiColors.Red,
            _ => ""
        };
    }
}

public static class AnsiColors
{
    public const string Reset = "\x1b[0m";
    public const string Red = "\x1b[91m";
    public const string Green = "\x1b[92m";
    public const string Yellow = "\x1b[93m";
}
