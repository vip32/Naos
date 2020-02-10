namespace Naos.JobScheduling.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IJob
    {
        Task ExecuteAsync(string[] args = null);

        Task ExecuteAsync(string correlationId, string[] args = null);

        Task ExecuteAsync(CancellationToken cancellationToken, string[] args = null);

        Task ExecuteAsync(string correlationId, CancellationToken cancellationToken, string[] args = null);
    }
}
