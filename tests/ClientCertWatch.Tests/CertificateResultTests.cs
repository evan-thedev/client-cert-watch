using ClientCertWatch.Models;

namespace ClientCertWatch.Tests;

public class CertificateResultTests
{
    [Fact]
    public void GetSeverity_UnreachableHost_ReturnsCritical()
    {
        var result = new CertificateResult
        {
            IsReachable = false
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Critical, severity);
    }

    [Fact]
    public void GetSeverity_TlsHandshakeFailed_ReturnsCritical()
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = false
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Critical, severity);
    }

    [Fact]
    public void GetSeverity_HasError_ReturnsCritical()
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = true,
            ErrorMessage = "Some error",
            DaysRemaining = 100
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Critical, severity);
    }

    [Fact]
    public void GetSeverity_NoDaysRemaining_ReturnsCritical()
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = true,
            DaysRemaining = null
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Critical, severity);
    }

    [Fact]
    public void GetSeverity_ExpiredCert_ReturnsCritical()
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = true,
            DaysRemaining = -5
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Critical, severity);
    }

    [Fact]
    public void GetSeverity_BelowCriticalThreshold_ReturnsCritical()
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = true,
            DaysRemaining = 10
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Critical, severity);
    }

    [Fact]
    public void GetSeverity_BelowWarnThreshold_ReturnsWarning()
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = true,
            DaysRemaining = 20
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Warning, severity);
    }

    [Fact]
    public void GetSeverity_AboveWarnThreshold_ReturnsOk()
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = true,
            DaysRemaining = 60
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(Severity.Ok, severity);
    }

    [Theory]
    [InlineData(31, Severity.Ok)]
    [InlineData(30, Severity.Warning)]
    [InlineData(15, Severity.Warning)]
    [InlineData(14, Severity.Critical)]
    [InlineData(0, Severity.Critical)]
    public void GetSeverity_BoundaryValues_ReturnsCorrectSeverity(int daysRemaining, Severity expected)
    {
        var result = new CertificateResult
        {
            IsReachable = true,
            TlsHandshakeSuccess = true,
            DaysRemaining = daysRemaining
        };

        var severity = result.GetSeverity(warnThreshold: 30, criticalThreshold: 14);

        Assert.Equal(expected, severity);
    }
}
