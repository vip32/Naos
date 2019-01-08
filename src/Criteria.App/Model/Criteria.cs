namespace Naos.Core.Filtering.App
{
    using EnsureThat;

    public class Criteria
    {
        public Criteria(string name, CriteriaOperator @operator, object value)
        {
            EnsureArg.IsNotNullOrEmpty(name);

            this.Name = name;
            this.Operator = @operator;
            this.Value = value;
        }

        public string Name { get; }

        public CriteriaOperator Operator { get; }

        public object Value { get; }
    }
}
