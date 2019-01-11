namespace Naos.Core.Discovery.App
{
    public interface IServiceRegistry
    {
        void Register(ServiceRegistration registration);

        void DeRegister(string id);
    }
}
