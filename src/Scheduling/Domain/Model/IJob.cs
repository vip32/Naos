namespace Naos.Core.Scheduling.Domain
{
    using System.Threading.Tasks;

    public interface IJob
    {
        Task ExecuteAsync(string[] args = null);
    }
}
