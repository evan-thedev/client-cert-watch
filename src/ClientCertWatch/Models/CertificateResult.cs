namespace ClientCertWatch.Models;

public class CertificateResult
{
    public string Hostname { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool IsReachable { get; set; }
    public bool TlsHandshakeSuccess { get; set; }
    public string? Subject { get; set; }
    public string? Issuer { get; set; }
    public DateTime? NotBefore { get; set; }
    public DateTime? NotAfter { get; set; }
    public int? DaysRemaining { get; set; }
    public string? ErrorMessage { get; set; }

    public Severity GetSeverity(int warnThreshold, int criticalThreshold)
    {
        if (!IsReachable || !TlsHandshakeSuccess || ErrorMessage != null)
        {
            return Severity.Critical;
        }

        if (!DaysRemaining.HasValue)
        {
            return Severity.Critical;
        }

        if (DaysRemaining.Value <= criticalThreshold)
        {
            return Severity.Critical;
        }

        if (DaysRemaining.Value <= warnThreshold)
        {
            return Severity.Warning;
        }

        return Severity.Ok;
    }
}

public enum Severity
{
    Ok = 0,
    Warning = 1,
    Critical = 2
}
