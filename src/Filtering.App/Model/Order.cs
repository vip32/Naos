namespace Naos.Core.RequestFiltering.App
{
    using EnsureThat;
    using Naos.Core.Common;

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

        public override string ToString()
        {
            return $"(t) => t.{this.Name} ({this.Direction.ToDescription()})";
        }
    }
}
