namespace Naos.Core.Scheduling.Domain
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IJob
    {
        Task ExecuteAsync(string[] args = null);

        Task ExecuteAsync(CancellationToken token, string[] args = null);
    }
}
