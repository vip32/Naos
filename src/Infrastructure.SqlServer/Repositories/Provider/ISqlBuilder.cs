namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public interface ISqlBuilder
    {
        string IndexColumnNameSuffix { get; }

        string BuildUseDatabase(string databaseName);

        string BuildDeleteByKey(string tableName);

        string BuildDeleteByTags(string tableName);

        string BuildValueSelectByKey(string tableName);

        string BuildValueSelectByTags(string tableName);

        string BuildDataSelectByKey(string tableName);

        //string BuildCriteriaSelect(string column, CriteriaOperator op, string value);

        string BuildCriteriaSelect<T>(Expression expression = null, IEnumerable<IIndexMap<T>> indexMaps = null);

        //string BuildCriteriaSelect<TDoc>(IEnumerable<IIndexMap<TDoc>> indexMaps = null, ICriteria criteria = null); //Expression<Func<T, bool>> expression

        string BuildTagSelect(string tag);

        string BuildSortingSelect(/*SortColumn sorting = SortColumn.Id*/);

        string BuildPagingSelect(int? skip = null, int? take = null, int? defaultTakeSize = 0, int? maxTakeSize = 0);

        string BuildFromTillDateTimeSelect(DateTime? fromDateTime = null, DateTime? tillDateTime = null);

        string TableNamesSelect();
    }
}
