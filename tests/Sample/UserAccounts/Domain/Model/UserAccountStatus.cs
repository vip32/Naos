namespace Naos.Sample.UserAccounts.Domain
{
    using System;
    using System.Collections.Generic;
    using EnsureThat;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class UserAccountStatus : ValueObject
    {
        public static readonly UserAccountStatus Active = new UserAccountStatus(UserAccountStatusType.Active);
        public static readonly UserAccountStatus Inactive = new UserAccountStatus(UserAccountStatusType.Inactive);

        public UserAccountStatus(string value)
        {
            Enum.TryParse(typeof(UserAccountStatusType), value, out var result);

            this.Value = (UserAccountStatusType?)result ??
                throw new InvalidOperationException("value should be valid and not empty");
        }

        private UserAccountStatus(UserAccountStatusType value)
        {
            this.Value = value;
        }

        public UserAccountStatusType Value { get; private set; }

        public static implicit operator string(UserAccountStatus value) => value.ToString();

        public static explicit operator UserAccountStatus(string value) => For(value);

        public static UserAccountStatus For(string value)
        {
            EnsureArg.IsNotNullOrEmpty(value, nameof(value));

            return new UserAccountStatus(value);
        }

        public override string ToString() => this.Value.ToString();

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return this.Value;
        }
    }
}
