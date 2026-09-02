# Client Cert Watch

A .NET 8 command-line tool for monitoring TLS certificate expiration and server reachability.

## Overview

Client Cert Watch checks TLS/SSL certificates for a list of domains, reporting:
- Certificate expiration (days remaining)
- TCP reachability
- TLS handshake status
- Certificate subject and issuer information

Results are displayed as a color-coded table or as JSON for CI/CD integration.

## Features

- **Multiple input sources**: Command-line arguments, text files, or stdin
- **Flexible port configuration**: Default port 443, or specify per-host
- **Color-coded output**: Visual severity indicators (auto-disabled for non-TTY or with `--no-color`)
- **JSON output**: Machine-readable format for automation
- **CI-friendly exit codes**: 0 (ok), 1 (warning), 2 (critical/error)
- **Configurable thresholds**: Warning and critical days-remaining thresholds
- **Connection timeout**: Configurable timeout to avoid hanging on unreachable hosts

## Installation

Clone the repository and build:

```bash
git clone https://github.com/evan-thedev/client-cert-watch.git
cd client-cert-watch
dotnet build
```

Or run directly:

```bash
dotnet run --project src/ClientCertWatch -- --help
```

## Usage

```
client-cert-watch [OPTIONS] [HOSTNAME...]
```

### Options

| Flag | Description | Default |
|------|-------------|---------|
| `-h`, `--help` | Show help message | - |
| `-f`, `--file FILE` | Read hostnames from file (one per line, # for comments) | - |
| `--stdin` | Read hostnames from stdin | - |
| `-p`, `--port PORT` | Default port for connections | 443 |
| `-t`, `--timeout SECONDS` | Connection timeout in seconds | 10 |
| `-w`, `--warn DAYS` | Warning threshold in days | 30 |
| `-c`, `--critical DAYS` | Critical threshold in days | 14 |
| `--json` | Output results as JSON | false |
| `--no-color` | Disable colored output | false |

### Exit Codes

- **0**: All certificates OK (reachable, valid, days remaining > warning threshold)
- **1**: At least one warning (days remaining ≤ warning threshold but > critical threshold)
- **2**: At least one critical issue (expired, unreachable, TLS handshake failure, or days remaining ≤ critical threshold)

## Examples

Check a single domain:

```bash
dotnet run --project src/ClientCertWatch -- example.com
```

Check multiple domains with custom port:

```bash
dotnet run --project src/ClientCertWatch -- example.com:8443 test.com:9443
```

Read from a file:

```bash
dotnet run --project src/ClientCertWatch -- --file domains.txt
```

Example `domains.txt`:

```
# Production servers
example.com
api.example.com:8443

# Staging
staging.example.com
```

Output as JSON:

```bash
dotnet run --project src/ClientCertWatch -- example.com --json
```

Pipe domains from stdin:

```bash
echo "example.com" | dotnet run --project src/ClientCertWatch -- --stdin
```

Custom thresholds for monitoring:

```bash
dotnet run --project src/ClientCertWatch -- example.com --warn 60 --critical 30
```

## Output Examples

### Table Output

```
HOSTNAME                       PORT   REACHABLE  TLS OK   DAYS LEFT  STATUS
----------------------------------------------------------------------------------------------------
example.com:443                443    Yes        Yes      90         OK
test.com:8443                  8443   Yes        Yes      25         WARNING
expired.com:443                443    Yes        Yes      -10        EXPIRED
unreachable.com:443            443    No         No       N/A        UNREACHABLE
  └─ Connection failed: Connection refused
```

### JSON Output

```json
[
  {
    "hostname": "example.com",
    "port": 443,
    "isReachable": true,
    "tlsHandshakeSuccess": true,
    "subject": "CN=example.com",
    "issuer": "CN=DigiCert TLS RSA SHA256 2020 CA1",
    "notBefore": "2024-01-15T00:00:00.0000000Z",
    "notAfter": "2025-02-15T23:59:59.0000000Z",
    "daysRemaining": 90,
    "severity": "ok",
    "errorMessage": null
  }
]
```

## Testing

Run unit tests:

```bash
dotnet test
```

## Technology Stack

- .NET 8.0
- C# 12
- xUnit for testing

## Author

Evan Parrott

## License

MIT License - see [LICENSE](LICENSE) file for details.
