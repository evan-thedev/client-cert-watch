using ClientCertWatch.Models;

namespace ClientCertWatch.Services;

public interface ITlsChecker
{
    Task<CertificateResult> CheckCertificateAsync(string hostname, int port, int timeoutSeconds, CancellationToken cancellationToken = default);
}
