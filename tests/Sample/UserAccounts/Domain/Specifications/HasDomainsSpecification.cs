namespace Naos.Sample.UserAccounts.Domain
{
    using System;
    using System.Linq.Expressions;
    using LinqKit;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class HasDomainsSpecification : Specification<UserAccount>
    {
        private readonly string[] domains;

        public HasDomainsSpecification(string[] domains)
        {
            this.domains = domains;
        }

        public override Expression<Func<UserAccount, bool>> ToExpression()
        {
            var expression = PredicateBuilder.New<UserAccount>(); // https://github.com/scottksmith95/LINQKit#predicatebuilder
            foreach (var domain in this.domains.Safe())
            {
                expression = expression.Or(p => p.AdAccount.Domain == domain); // Or = linqkit
            }

            return expression; // acts for all intents and purposes as an Expression<Func<T, bool>> object
        }
    }
}
