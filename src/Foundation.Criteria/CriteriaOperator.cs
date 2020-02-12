namespace Naos.Foundation
{
    public enum CriteriaOperator
    {
        /// <summary>
        /// Equal to
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = "==", Abbreviation = "eq")]
        Equal = 10,

        /// <summary>
        /// Equal to
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = "!=", Abbreviation = "ne")]
        NotEqual = 11,

        /// <summary>
        /// Greater than
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = ">", Abbreviation = "gt")]
        GreaterThan = 20,

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = ">=", Abbreviation = "ge")]
        GreaterThanOrEqual = 21,

        /// <summary>
        /// Less than
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = "<", Abbreviation = "lt")]
        LessThan = 30,

        /// <summary>
        /// Less than or equal to
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = "<=", Abbreviation = "le")]
        LessThanOrEqual = 31,

        /// <summary>
        /// Contains
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = "Contains", IsFunction = true, Abbreviation = "ct")]
        Contains = 40,

        /// <summary>
        /// StartsWith
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = "StartsWith", IsFunction = true, Abbreviation = "sw")]
        StartsWith = 41,

        /// <summary>
        /// EndsWith
        /// </summary>
        [CriteriaOperatorMetadataAttribute(Value = "EndsWith", IsFunction = true, Abbreviation = "ew")]
        EndsWith = 42,

        ///// <summary>
        ///// EndsWith
        ///// </summary>
        //[CriteriaOperatorMetadata(Value = "Between", IsFunction = true, Abbreviation = "btw", Template="VAL1,VAL2")]
        //Between = 43
    }
}
