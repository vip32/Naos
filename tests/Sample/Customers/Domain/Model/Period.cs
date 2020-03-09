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

        public DateTime? StartDate { get; }

        public DateTime? EndDate { get; }

        public static Period Create(DateTime? startDate, DateTime? endDate)
        {
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
    }
}
