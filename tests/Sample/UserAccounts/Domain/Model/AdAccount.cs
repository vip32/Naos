namespace Naos.Sample.UserAccounts.Domain
{
    using System.Collections.Generic;
    using EnsureThat;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class AdAccount : ValueObject
    {
        private AdAccount()
        {
        }

        public string Domain { get; private set; }

        public string Name { get; private set; }

        public static implicit operator string(AdAccount value) => value.ToString();

        public static explicit operator AdAccount(string value) => For(value);

        public static AdAccount For(string value)
        {
            EnsureArg.IsNotNullOrEmpty(value, nameof(value));
            EnsureArg.IsTrue(value.Contains("\\"), nameof(value));

            return new AdAccount
            {
                Domain = value.SliceTill("\\"),
                Name = value.SliceFrom("\\")
            };
        }

        public override string ToString() => $"{this.Domain}\\{this.Name}";

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return this.Domain;
            yield return this.Name;
        }
    }
}
