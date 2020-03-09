namespace Naos.Sample.UserAccounts.Domain
{
    using System.Collections.Generic;
    using EnsureThat;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class AdAccount : ValueObject
    {
        private AdAccount()
        {
        }

        private AdAccount(string domain, string name)
        {
            this.Domain = domain;
            this.Name = name;
        }

        public string Domain { get; }

        public string Name { get; }

        //public static implicit operator string(AdAccount value) => value.ToString();

        //public static explicit operator AdAccount(string value) => For(value);

        public static AdAccount For(string value)
        {
            EnsureArg.IsNotNullOrEmpty(value, nameof(value));

            //EnsureArg.IsTrue(value.Contains("\\", System.StringComparison.OrdinalIgnoreCase), nameof(value));
            Check.Throw(new AdAccountShouldBePartOfDomainSpecification(value));

            return new AdAccount(value.SliceTill("\\"), value.SliceFrom("\\"));
        }

        public override string ToString() => $"{this.Domain}\\{this.Name}";

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return this.Domain;
            yield return this.Name;
        }

        public class AdAccountShouldBePartOfDomainSpecification : Specification // // business rule example
        {
            public AdAccountShouldBePartOfDomainSpecification(string value)
                : base(() => value.Contains("\\", System.StringComparison.OrdinalIgnoreCase))
            {
            }

            public override string Description => "AD Account should be part of a domain";
        }
    }
}
