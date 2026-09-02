using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using ClientCertWatch.Models;

namespace ClientCertWatch.Services;

public class TlsChecker : ITlsChecker
{
    public async Task<CertificateResult> CheckCertificateAsync(string hostname, int port, int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        var result = new CertificateResult
        {
            Hostname = hostname,
            Port = port,
            IsReachable = false,
            TlsHandshakeSuccess = false
        };

        TcpClient? client = null;
        SslStream? sslStream = null;

        try
        {
            client = new TcpClient();
            
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            await client.ConnectAsync(hostname, port, linkedCts.Token);
            result.IsReachable = true;

            sslStream = new SslStream(
                client.GetStream(),
                false,
                (sender, certificate, chain, errors) => true);

            await sslStream.AuthenticateAsClientAsync(hostname, null, System.Security.Authentication.SslProtocols.None, false);
            result.TlsHandshakeSuccess = true;

            var remoteCertificate = sslStream.RemoteCertificate;
            if (remoteCertificate != null)
            {
                var cert = new X509Certificate2(remoteCertificate);
                result.Subject = cert.Subject;
                result.Issuer = cert.Issuer;
                result.NotBefore = cert.NotBefore.ToUniversalTime();
                result.NotAfter = cert.NotAfter.ToUniversalTime();
                
                var daysRemaining = (cert.NotAfter.ToUniversalTime() - DateTime.UtcNow).TotalDays;
                result.DaysRemaining = (int)Math.Floor(daysRemaining);
            }
        }
        catch (SocketException ex)
        {
            result.ErrorMessage = $"Connection failed: {ex.Message}";
        }
        catch (AuthenticationException ex)
        {
            result.ErrorMessage = $"TLS handshake failed: {ex.Message}";
        }
        catch (OperationCanceledException)
        {
            result.ErrorMessage = $"Connection timeout after {timeoutSeconds}s";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            sslStream?.Dispose();
            client?.Dispose();
        }

        return result;
    }
}
