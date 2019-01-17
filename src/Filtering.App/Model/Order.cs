namespace Naos.Core.Filtering.App
{
    using EnsureThat;

    public class Order
    {
        public Order(string name, OrderDirection direction = OrderDirection.Asc)
        {
            EnsureArg.IsNotNullOrEmpty(name);

            this.Name = name;
            this.Direction = direction;
        }

        public string Name { get; }

        public OrderDirection Direction { get; }
    }
}
