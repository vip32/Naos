namespace Naos.Core.Scheduling.Domain
{
    using System.Threading.Tasks;

    public interface IScheduledTask
    {
        Task ExecuteAsync(string[] args = null);
    }
}
