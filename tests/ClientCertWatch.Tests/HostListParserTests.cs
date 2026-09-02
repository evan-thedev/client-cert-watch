using ClientCertWatch.Services;

namespace ClientCertWatch.Tests;

public class HostListParserTests
{
    [Fact]
    public void ParseHostList_SingleHost_ReturnsHostWithDefaultPort()
    {
        var lines = new[] { "example.com" };
        var result = HostListParser.ParseHostList(lines, 443);

        Assert.Single(result);
        Assert.Equal("example.com", result[0].Hostname);
        Assert.Equal(443, result[0].Port);
    }

    [Fact]
    public void ParseHostList_HostWithPort_ReturnsHostWithSpecifiedPort()
    {
        var lines = new[] { "example.com:8443" };
        var result = HostListParser.ParseHostList(lines, 443);

        Assert.Single(result);
        Assert.Equal("example.com", result[0].Hostname);
        Assert.Equal(8443, result[0].Port);
    }

    [Fact]
    public void ParseHostList_MultipleHosts_ReturnsAllHosts()
    {
        var lines = new[] { "example.com", "test.com:8443", "another.com" };
        var result = HostListParser.ParseHostList(lines, 443);

        Assert.Equal(3, result.Count);
        Assert.Equal("example.com", result[0].Hostname);
        Assert.Equal(443, result[0].Port);
        Assert.Equal("test.com", result[1].Hostname);
        Assert.Equal(8443, result[1].Port);
        Assert.Equal("another.com", result[2].Hostname);
        Assert.Equal(443, result[2].Port);
    }

    [Fact]
    public void ParseHostList_IgnoresComments_ReturnsOnlyValidHosts()
    {
        var lines = new[] { "# This is a comment", "example.com", "# Another comment" };
        var result = HostListParser.ParseHostList(lines, 443);

        Assert.Single(result);
        Assert.Equal("example.com", result[0].Hostname);
    }

    [Fact]
    public void ParseHostList_IgnoresBlankLines_ReturnsOnlyValidHosts()
    {
        var lines = new[] { "", "example.com", "  ", "test.com", "\t" };
        var result = HostListParser.ParseHostList(lines, 443);

        Assert.Equal(2, result.Count);
        Assert.Equal("example.com", result[0].Hostname);
        Assert.Equal("test.com", result[1].Hostname);
    }

    [Fact]
    public void ParseHostList_MixedInput_ReturnsOnlyValidHosts()
    {
        var lines = new[]
        {
            "# Production servers",
            "example.com",
            "",
            "test.com:8443",
            "# Staging",
            "staging.com:9443"
        };
        var result = HostListParser.ParseHostList(lines, 443);

        Assert.Equal(3, result.Count);
        Assert.Equal("example.com", result[0].Hostname);
        Assert.Equal(443, result[0].Port);
        Assert.Equal("test.com", result[1].Hostname);
        Assert.Equal(8443, result[1].Port);
        Assert.Equal("staging.com", result[2].Hostname);
        Assert.Equal(9443, result[2].Port);
    }
}
