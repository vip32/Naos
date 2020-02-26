namespace Naos.Foundation
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CriteriaOperatorMetadataAttribute : Attribute
    {
        public string Value { get; set; }

        public bool IsFunction { get; set; }

        public string Abbreviation { get; set; }
    }
}
