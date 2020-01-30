namespace Naos.Foundation.Application
{
    public class DashboardMenuItem
    {
        public DashboardMenuItem(string name, string url = null, string icon = null)
        {
            this.Name = name;
            this.Url = url ?? $"/{name}";
            this.Icon = icon;
        }

        public string Name { get; set; }

        public string Url { get; set; }

        public string Icon { get; set; }
    }
}
