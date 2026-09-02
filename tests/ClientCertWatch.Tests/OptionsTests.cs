using ClientCertWatch;

namespace ClientCertWatch.Tests;

public class OptionsTests
{
    [Fact]
    public void Parse_NoArgs_ReturnsDefaultOptions()
    {
        var options = Options.Parse(Array.Empty<string>());

        Assert.Empty(options.Hosts);
        Assert.Null(options.InputFile);
        Assert.False(options.ReadFromStdin);
        Assert.Equal(443, options.DefaultPort);
        Assert.Equal(10, options.TimeoutSeconds);
        Assert.Equal(30, options.WarnThreshold);
        Assert.Equal(14, options.CriticalThreshold);
        Assert.False(options.JsonOutput);
        Assert.False(options.ShowHelp);
    }

    [Fact]
    public void Parse_HelpFlag_SetsShowHelp()
    {
        var options = Options.Parse(new[] { "--help" });
        Assert.True(options.ShowHelp);

        options = Options.Parse(new[] { "-h" });
        Assert.True(options.ShowHelp);
    }

    [Fact]
    public void Parse_SingleHost_AddsHostToList()
    {
        var options = Options.Parse(new[] { "example.com" });

        Assert.Single(options.Hosts);
        Assert.Equal("example.com", options.Hosts[0]);
    }

    [Fact]
    public void Parse_MultipleHosts_AddsAllHosts()
    {
        var options = Options.Parse(new[] { "example.com", "test.com", "another.com" });

        Assert.Equal(3, options.Hosts.Count);
        Assert.Equal("example.com", options.Hosts[0]);
        Assert.Equal("test.com", options.Hosts[1]);
        Assert.Equal("another.com", options.Hosts[2]);
    }

    [Fact]
    public void Parse_FileFlag_SetsInputFile()
    {
        var options = Options.Parse(new[] { "--file", "hosts.txt" });
        Assert.Equal("hosts.txt", options.InputFile);

        options = Options.Parse(new[] { "-f", "domains.txt" });
        Assert.Equal("domains.txt", options.InputFile);
    }

    [Fact]
    public void Parse_StdinFlag_SetsReadFromStdin()
    {
        var options = Options.Parse(new[] { "--stdin" });
        Assert.True(options.ReadFromStdin);
    }

    [Fact]
    public void Parse_PortFlag_SetsDefaultPort()
    {
        var options = Options.Parse(new[] { "--port", "8443" });
        Assert.Equal(8443, options.DefaultPort);

        options = Options.Parse(new[] { "-p", "9443" });
        Assert.Equal(9443, options.DefaultPort);
    }

    [Fact]
    public void Parse_TimeoutFlag_SetsTimeoutSeconds()
    {
        var options = Options.Parse(new[] { "--timeout", "30" });
        Assert.Equal(30, options.TimeoutSeconds);

        options = Options.Parse(new[] { "-t", "60" });
        Assert.Equal(60, options.TimeoutSeconds);
    }

    [Fact]
    public void Parse_WarnFlag_SetsWarnThreshold()
    {
        var options = Options.Parse(new[] { "--warn", "60" });
        Assert.Equal(60, options.WarnThreshold);

        options = Options.Parse(new[] { "-w", "45" });
        Assert.Equal(45, options.WarnThreshold);
    }

    [Fact]
    public void Parse_CriticalFlag_SetsCriticalThreshold()
    {
        var options = Options.Parse(new[] { "--critical", "7" });
        Assert.Equal(7, options.CriticalThreshold);

        options = Options.Parse(new[] { "-c", "21" });
        Assert.Equal(21, options.CriticalThreshold);
    }

    [Fact]
    public void Parse_JsonFlag_SetsJsonOutput()
    {
        var options = Options.Parse(new[] { "--json" });
        Assert.True(options.JsonOutput);
    }

    [Fact]
    public void Parse_NoColorFlag_SetsNoColor()
    {
        var options = Options.Parse(new[] { "--no-color" });
        Assert.True(options.NoColor);
    }

    [Fact]
    public void Parse_MixedOptions_ParsesAllCorrectly()
    {
        var options = Options.Parse(new[]
        {
            "example.com",
            "--port", "8443",
            "--warn", "60",
            "--critical", "30",
            "--json",
            "test.com"
        });

        Assert.Equal(2, options.Hosts.Count);
        Assert.Equal("example.com", options.Hosts[0]);
        Assert.Equal("test.com", options.Hosts[1]);
        Assert.Equal(8443, options.DefaultPort);
        Assert.Equal(60, options.WarnThreshold);
        Assert.Equal(30, options.CriticalThreshold);
        Assert.True(options.JsonOutput);
    }

    [Fact]
    public void Parse_UnknownOption_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Options.Parse(new[] { "--unknown" }));
    }

    [Fact]
    public void Parse_MissingValueForFlag_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Options.Parse(new[] { "--port" }));
        Assert.Throws<ArgumentException>(() => Options.Parse(new[] { "--file" }));
        Assert.Throws<ArgumentException>(() => Options.Parse(new[] { "--timeout" }));
    }
}
