namespace Naos.Core.Filtering.App
{
    using EnsureThat;

    public class OrderBy
    {
        public OrderBy(string name, OrderByDirection direction = OrderByDirection.Ascending)
        {
            EnsureArg.IsNotNullOrEmpty(name);

            this.Name = name;
            this.Direction = direction;
        }

        public string Name { get; }

        public OrderByDirection Direction { get; }
    }
}
