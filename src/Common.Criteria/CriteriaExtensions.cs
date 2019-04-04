namespace Naos.Core.Common
{
    using System;

    public static class CriteriaExtensions
    {
        public static string ToValue(this CriteriaOperator @operator)
        {
            var metadata = GetMetadata(@operator);
            return (metadata != null) ? ((CriteriaOperatorMetadata)metadata).Value : @operator.ToString();
        }

        public static string ToAbbreviation(this CriteriaOperator @operator)
        {
            var metadata = GetMetadata(@operator);
            return (metadata != null) ? ((CriteriaOperatorMetadata)metadata).Abbreviation : @operator.ToString();
        }

        public static bool IsFunction(this CriteriaOperator @operator)
        {
            var metadata = GetMetadata(@operator);
            return (metadata != null) ? ((CriteriaOperatorMetadata)metadata).IsFunction : false;
        }

        public static CriteriaOperator FromValue(string value, CriteriaOperator @default = CriteriaOperator.Equal)
        {
            if(string.IsNullOrEmpty(value))
            {
                return @default;
            }

            foreach(var enumValue in Enum.GetValues(typeof(CriteriaOperator)))
            {
                Enum.TryParse(enumValue.ToString(), true, out CriteriaOperator @operator);
                var metaDataValue = @operator.GetAttributeValue<CriteriaOperatorMetadata, string>(x => x.Value);
                if(metaDataValue != null && metaDataValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    return @operator;
                }
            }

            return @default;
        }

        public static CriteriaOperator FromAbbreviation(string value, CriteriaOperator @default = CriteriaOperator.Equal)
        {
            if(string.IsNullOrEmpty(value))
            {
                return @default;
            }

            foreach(var enumValue in Enum.GetValues(typeof(CriteriaOperator)))
            {
                Enum.TryParse(enumValue.ToString(), true, out CriteriaOperator @operator);
                var metaDataValue = @operator.GetAttributeValue<CriteriaOperatorMetadata, string>(x => x.Abbreviation);
                if(metaDataValue != null && metaDataValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    return @operator;
                }
            }

            return @default;
        }

        private static object GetMetadata(CriteriaOperator @operator)
        {
            var type = @operator.GetType();
            var info = type.GetMember(@operator.ToString());
            if((info != null) && (info.Length > 0))
            {
                var attrs = info[0].GetCustomAttributes(typeof(CriteriaOperatorMetadata), false);
                if((attrs != null) && (attrs.Length > 0))
                {
                    return attrs[0];
                }
            }

            return null;
        }
    }
}
