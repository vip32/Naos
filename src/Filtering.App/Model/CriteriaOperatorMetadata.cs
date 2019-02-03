namespace Naos.Core.RequestFiltering.App
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class CriteriaOperatorMetadata : Attribute
    {
        public string Value { get; set; }

        public bool IsFunction { get; set; }

        public string Abbreviation { get; set; }
    }
}
