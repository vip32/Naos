namespace Naos.Foundation.Application
{
    public class DashboardMenuItem
    {
        public DashboardMenuItem(string name, string url)
        {
            this.Name = name;
            this.Url = url;
        }

        public string Name { get; set; }

        public string Url { get; set; }
    }
}
