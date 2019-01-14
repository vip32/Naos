namespace Naos.Core.App
{
    public class ServiceContext // TODO: or is this pure App layer
    {
        public ServiceContext()
        {
            this.Service = new ServiceDescriptor();
            this.TenantId = "default";
        }

        public string TenantId { get; set; }

        public bool TestMode { get; set; }

        public string UserIdentity { get; set; }

        public string Username { get; set; }

        public ServiceDescriptor Service { get; set; }

        public ServiceDescriptor Referrer { get; set; }

        // runtimedescriptor?
    }
}
