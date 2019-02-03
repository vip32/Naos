namespace Naos.Core.RequestFiltering.App
{
    public enum CriteriaOperator
    {
        /// <summary>
        /// Equal to
        /// </summary>
        [CriteriaOperatorMetadata(Value = "==", Abbreviation = "eq")]
        Equal = 10,

        /// <summary>
        /// Greater than
        /// </summary>
        [CriteriaOperatorMetadata(Value = ">", Abbreviation = "gt")]
        GreaterThan = 20,

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        [CriteriaOperatorMetadata(Value = ">=", Abbreviation = "gte")]
        GreaterThanOrEqual = 21,

        /// <summary>
        /// Less than
        /// </summary>
        [CriteriaOperatorMetadata(Value = "<", Abbreviation = "lt")]
        LessThan = 30,

        /// <summary>
        /// Less than or equal to
        /// </summary>
        [CriteriaOperatorMetadata(Value = "<=", Abbreviation = "lte")]
        LessThanOrEqual = 31,

        /// <summary>
        /// Contains
        /// </summary>
        [CriteriaOperatorMetadata(Value = "Contains", IsFunction = true, Abbreviation = "ct")]
        Contains = 40,

        /// <summary>
        /// StartsWith
        /// </summary>
        [CriteriaOperatorMetadata(Value = "StartsWith", IsFunction = true, Abbreviation = "sw")]
        StartsWith = 41,

        /// <summary>
        /// EndsWith
        /// </summary>
        [CriteriaOperatorMetadata(Value = "EndsWith", IsFunction = true, Abbreviation = "ew")]
        EndsWith = 42,

        ///// <summary>
        ///// EndsWith
        ///// </summary>
        //[CriteriaOperatorMetadata(Value = "Between", IsFunction = true, Abbreviation = "btw", Template="VAL1,VAL2")]
        //Between = 43
    }
}
