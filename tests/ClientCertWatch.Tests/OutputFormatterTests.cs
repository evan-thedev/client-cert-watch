using System.Text.Json;
using ClientCertWatch.Models;
using ClientCertWatch.Services;

namespace ClientCertWatch.Tests;

public class OutputFormatterTests
{
    [Fact]
    public void ToJson_SingleResult_ReturnsValidJson()
    {
        var results = new List<CertificateResult>
        {
            new CertificateResult
            {
                Hostname = "example.com",
                Port = 443,
                IsReachable = true,
                TlsHandshakeSuccess = true,
                Subject = "CN=example.com",
                Issuer = "CN=Test CA",
                NotBefore = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                NotAfter = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DaysRemaining = 90
            }
        };

        var formatter = new OutputFormatter(useColors: false, warnThreshold: 30, criticalThreshold: 14);
        var json = formatter.ToJson(results);

        Assert.NotNull(json);
        var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        
        Assert.True(root.ValueKind == JsonValueKind.Array);
        Assert.Equal(1, root.GetArrayLength());
        
        var first = root[0];
        Assert.Equal("example.com", first.GetProperty("hostname").GetString());
        Assert.Equal(443, first.GetProperty("port").GetInt32());
        Assert.True(first.GetProperty("isReachable").GetBoolean());
        Assert.True(first.GetProperty("tlsHandshakeSuccess").GetBoolean());
        Assert.Equal("CN=example.com", first.GetProperty("subject").GetString());
        Assert.Equal("CN=Test CA", first.GetProperty("issuer").GetString());
        Assert.Equal(90, first.GetProperty("daysRemaining").GetInt32());
        Assert.Equal("ok", first.GetProperty("severity").GetString());
    }

    [Fact]
    public void ToJson_MultipleResults_ReturnsArrayOfResults()
    {
        var results = new List<CertificateResult>
        {
            new CertificateResult
            {
                Hostname = "example.com",
                Port = 443,
                IsReachable = true,
                TlsHandshakeSuccess = true,
                DaysRemaining = 90
            },
            new CertificateResult
            {
                Hostname = "test.com",
                Port = 8443,
                IsReachable = true,
                TlsHandshakeSuccess = true,
                DaysRemaining = 20
            }
        };

        var formatter = new OutputFormatter(useColors: false, warnThreshold: 30, criticalThreshold: 14);
        var json = formatter.ToJson(results);

        var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        
        Assert.Equal(2, root.GetArrayLength());
        Assert.Equal("example.com", root[0].GetProperty("hostname").GetString());
        Assert.Equal("test.com", root[1].GetProperty("hostname").GetString());
        Assert.Equal("ok", root[0].GetProperty("severity").GetString());
        Assert.Equal("warning", root[1].GetProperty("severity").GetString());
    }

    [Fact]
    public void ToJson_FailedResult_IncludesErrorMessage()
    {
        var results = new List<CertificateResult>
        {
            new CertificateResult
            {
                Hostname = "unreachable.com",
                Port = 443,
                IsReachable = false,
                TlsHandshakeSuccess = false,
                ErrorMessage = "Connection failed: Host unreachable"
            }
        };

        var formatter = new OutputFormatter(useColors: false, warnThreshold: 30, criticalThreshold: 14);
        var json = formatter.ToJson(results);

        var parsed = JsonDocument.Parse(json);
        var root = parsed.RootElement;
        
        var first = root[0];
        Assert.False(first.GetProperty("isReachable").GetBoolean());
        Assert.False(first.GetProperty("tlsHandshakeSuccess").GetBoolean());
        Assert.Equal("Connection failed: Host unreachable", first.GetProperty("errorMessage").GetString());
        Assert.Equal("critical", first.GetProperty("severity").GetString());
    }

    [Theory]
    [InlineData(60, "ok")]
    [InlineData(20, "warning")]
    [InlineData(10, "critical")]
    [InlineData(-5, "critical")]
    public void ToJson_DifferentDaysRemaining_CorrectSeverity(int daysRemaining, string expectedSeverity)
    {
        var results = new List<CertificateResult>
        {
            new CertificateResult
            {
                Hostname = "example.com",
                Port = 443,
                IsReachable = true,
                TlsHandshakeSuccess = true,
                DaysRemaining = daysRemaining
            }
        };

        var formatter = new OutputFormatter(useColors: false, warnThreshold: 30, criticalThreshold: 14);
        var json = formatter.ToJson(results);

        var parsed = JsonDocument.Parse(json);
        var severity = parsed.RootElement[0].GetProperty("severity").GetString();
        
        Assert.Equal(expectedSeverity, severity);
    }
}
