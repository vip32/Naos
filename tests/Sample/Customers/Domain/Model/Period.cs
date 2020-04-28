namespace Naos.Sample.Customers.Domain
{
    using System;
    using System.Collections.Generic;
    using Naos.Foundation.Domain;

    public class Period : ValueObject
    {
        private Period(DateTime? startDate, DateTime? endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
        }

        public static Period NoTerm => new Period(null, null);

        public DateTime? StartDate { get; private set; }

        public DateTime? EndDate { get; private set; }

        public static Period Create(DateTime? startDate, DateTime? endDate)
        {
            Check.Throw(new EndDateShouldOccurAfterStartDateSpecification(startDate, endDate));

            return new Period(startDate, endDate);
        }

        public bool IsInPeriod(DateTime date)
        {
            var left = !this.StartDate.HasValue || this.StartDate.Value <= date;
            var right = !this.EndDate.HasValue || this.EndDate.Value >= date;

            return left && right;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return this.StartDate;
            yield return this.EndDate;
        }

        public class EndDateShouldOccurAfterStartDateSpecification : Specification // // business rule example
        {
            private readonly DateTime? startDate;
            private readonly DateTime? endDate;

            public EndDateShouldOccurAfterStartDateSpecification(DateTime? startDate, DateTime? endDate)
            {
                this.startDate = startDate;
                this.endDate = endDate;
            }

            public override string Description => "StartDate should occur after EndDate";

            public override bool IsSatisfied()
            {
                if (this.startDate.HasValue && this.endDate.HasValue)
                {
                    return this.startDate.Value <= this.endDate.Value;
                }

                return true;
            }
        }
    }
}
