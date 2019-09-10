namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class SqlBuilder : ISqlBuilder
    {
        public virtual string IndexColumnNameSuffix => "_idx";

        public virtual string BuildUseDatabase(string databaseName = null)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                return null;
            }

            return $"USE [{databaseName}]; ";
        }

        public virtual string BuildDeleteByKey(string tableName)
        {
            return $"DELETE FROM {tableName} WHERE [key]=@key";
        }

        public virtual string BuildDeleteByTags(string tableName)
        {
            return $"DELETE FROM {tableName} WHERE ";
        }

        public virtual string BuildValueSelectByKey(string tableName)
        {
            return $"SELECT [value] FROM {tableName} WHERE [key]=@key";
        }

        public virtual string BuildValueSelectByTags(string tableName)
        {
            return $"SELECT [value] FROM {tableName} WHERE [id]>0";
        }

        public virtual string BuildDataSelectByKey(string tableName)
        {
            return $"SELECT [data] FROM {tableName} WHERE [key]=@key";
        }

        public virtual string BuildTagSelect(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return string.Empty;
            }

            return $" AND [tags] LIKE '%||{this.Sanatize(tag)}||%'";
        }

        public virtual string BuildCriteriaSelect(Expression expression, IEnumerable<IIndexMap> indexMaps = null)
        {
            if (expression == null || indexMaps.IsNullOrEmpty())
            {
                return string.Empty;
            }

            if (expression is LambdaExpression lambdaExpression)
            {
                return this.BuildCriteriaSelect(lambdaExpression.Body, indexMaps);
            }

            // https://github.com/OctopusDeploy/Nevermore/blob/master/source/Nevermore/QueryBuilderWhereExtensions.cs
            if (expression is BinaryExpression binaryExpression)
            {
                //(binaryExpression.Left as BinaryExpression).Left
                if(binaryExpression.NodeType== ExpressionType.AndAlso)
                {
                    return $"{this.BuildCriteriaSelect(binaryExpression.Left, indexMaps)} { this.BuildCriteriaSelect(binaryExpression.Right, indexMaps)}";
                }

                var property = ExpressionHelper.GetProperty(binaryExpression.Left);
                var value = ExpressionHelper.GetValueFromExpression(binaryExpression.Right, property.PropertyType);
                if (!property.Name.EqualsAny(indexMaps?.Select(i => i.Name)))
                {
                    return string.Empty;
                }

                if (property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(char))
                {
                    return binaryExpression.NodeType switch
                    {
                        ExpressionType.NotEqual => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] != '{this.Sanatize(value.ToString())}' ",
                        ExpressionType.GreaterThan => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] > '{this.Sanatize(value.ToString())}' ",
                        ExpressionType.GreaterThanOrEqual => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] >= '{this.Sanatize(value.ToString())}' ",
                        ExpressionType.LessThan => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] < '{this.Sanatize(value.ToString())}' ",
                        ExpressionType.LessThanOrEqual => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] <= '{this.Sanatize(value.ToString())}' ",
                        //ExpressionType.AndAlso => $"{this.BuildCriteriaSelect(binaryExpression.Left, indexMaps)} {this.BuildCriteriaSelect(binaryExpression.Right, indexMaps)}",
                        _ => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] = '{this.Sanatize(value.ToString())}' ",
                    };
                }
                else if (property.PropertyType == typeof(short)
                    || property.PropertyType == typeof(int)
                    || property.PropertyType == typeof(long)
                    || property.PropertyType == typeof(float)
                    || property.PropertyType == typeof(decimal)
                    || property.PropertyType == typeof(bool))
                {
                    return binaryExpression.NodeType switch
                    {
                        ExpressionType.NotEqual => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] != {this.Sanatize(value.ToString())} ",
                        ExpressionType.GreaterThan => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] > {this.Sanatize(value.ToString())} ",
                        ExpressionType.GreaterThanOrEqual => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] >= {this.Sanatize(value.ToString())} ",
                        ExpressionType.LessThan => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] < {this.Sanatize(value.ToString())} ",
                        ExpressionType.LessThanOrEqual => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] <= {this.Sanatize(value.ToString())} ",
                        ExpressionType.AndAlso => $"{this.BuildCriteriaSelect(binaryExpression.Left, indexMaps)} {this.BuildCriteriaSelect(binaryExpression.Right, indexMaps)}",
                        _ => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] = {this.Sanatize(value.ToString())} ",
                    };
                }
                else
                {
                    throw new NotSupportedException("Only basic expression types are supported");
                }
            }

            if (expression is MethodCallExpression methodExpression)
            {
                if (methodExpression.Arguments.Count == 1 && methodExpression.Method.DeclaringType == typeof(string))
                {
                    return this.AddStringMethodFromExpression(methodExpression, indexMaps);
                }

                if (methodExpression.Method.Name == "Contains")
                {
                    return this.AddContainsFromExpression(methodExpression, indexMaps); // TODO: implement
                }

                throw new NotSupportedException("Only method calls that take a single string argument and Enumerable.Contains methods are supported");
            }

            if (expression is MemberExpression memberExpression1)
            {
                return HandleMemberExpression(memberExpression1, true, indexMaps);
            }

            if (expression is UnaryExpression unaryExpression)
            {
                if (!(unaryExpression.Operand is MemberExpression memberExpression2))
                {
                    throw new NotSupportedException("Only boolean properties are allowed when the ! operator is used, i.e. Where(e => !e.BoolProp)");
                }

                return HandleMemberExpression(memberExpression2, false, indexMaps);
            }

            throw new NotSupportedException($"The expression supplied is not supported. Only simple BinaryExpressions, LogicalBinaryExpressions and some MethodCallExpressions are supported. The predicate is a {expression.GetType()}.");

            string HandleMemberExpression(MemberExpression memberExpression, bool value, IEnumerable<IIndexMap> indexMaps = null)
            {
                if (!memberExpression.Member.Name.EqualsAny(indexMaps?.Select(i => i.Name)))
                {
                    return string.Empty;
                }

                if (memberExpression.Type == typeof(bool))
                {
                    return $" AND [{this.Sanatize(memberExpression.Member.Name).ToLower()}{this.IndexColumnNameSuffix}] = '{this.Sanatize(value.ToString())}' "; // =bit dbtype
                }

                throw new NotSupportedException("Only boolean properties are allowed for where expressions without a comparison operator or method call");
            }
        }

        //public virtual string BuildCriteriaSelect<T>(
        //    IEnumerable<IIndexMap<T>> indexMaps = null,
        //    ICriteria criteria = null) // Expression<Func<T, bool>> expression
        //{
        //    if (indexMaps == null || !indexMaps.Any())
        //    {
        //        return null;
        //    }

        //    if (criteria == null)
        //    {
        //        return null;
        //    }

        //    var indexMap = indexMaps.FirstOrDefault(i =>
        //        i.Name.Equals(criteria.Name, StringComparison.OrdinalIgnoreCase));
        //    if (indexMap == null)
        //    {
        //        return null;
        //    }

        //    // small equals hack to handle multiple values and optimize for single values (%)
        //    if ((indexMap.Values != null && indexMap.Value == null) && criteria.Operator == CriteriaOperator.Eq)
        //    {
        //        criteria.Operator = CriteriaOperator.Eqm;
        //    }

        //    return BuildCriteriaSelect(indexMap.Name, criteria.Operator, criteria.Value);
        //}

        //public virtual string BuildCriteriaSelect(string column, CriteriaOperator op, string value)
        //{
        //    if (string.IsNullOrEmpty(column))
        //    {
        //        return null;
        //    }

        //    // TODO: use sql cmd paramaters for the values
        //    // TODO: null value handling
        //    if (op == CriteriaOperator.Gt /*op.Equals(CriteriaOperator.Gt)*/)
        //    {
        //        return $" AND [{this.Sanatize(column).ToLower()}{this.IndexColumnNameSuffix}] > '||{this.Sanatize(value)}' ";
        //    }

        //    if (op == CriteriaOperator.Ge /*op.Equals(CriteriaOperator.Ge)*/)
        //    {
        //        return $" AND [{this.Sanatize(column).ToLower()}{this.IndexColumnNameSuffix}] >= '||{this.Sanatize(value)}' ";
        //    }

        //    if (op == CriteriaOperator.Lt /*op.Equals(CriteriaOperator.Lt)*/)
        //    {
        //        return $" AND [{this.Sanatize(column).ToLower()}{this.IndexColumnNameSuffix}] < '||{this.Sanatize(value)}' ";
        //    }

        //    if (op == CriteriaOperator.Le /*op.Equals(CriteriaOperator.Le)*/)
        //    {
        //        return $" AND [{this.Sanatize(column).ToLower()}{this.IndexColumnNameSuffix}] <= '||{this.Sanatize(value)}' ";
        //    }

        //    if (op == CriteriaOperator.Contains /*op.Equals(CriteriaOperator.Contains)*/)
        //    {
        //        return $" AND [{this.Sanatize(column).ToLower()}{this.IndexColumnNameSuffix}] LIKE '||%{this.Sanatize(value)}%||' ";
        //    }

        //    if (op == CriteriaOperator.Eqm /*op.Equals(CriteriaOperator.Eqm)*/)
        //    {
        //        return $" AND [{this.Sanatize(column).ToLower()}{this.IndexColumnNameSuffix}] LIKE '%||{this.Sanatize(value)}||%' ";
        //    }

        //    // TODO: remove % for much faster PERF

        //    return $" AND [{this.Sanatize(column).ToLower()}{this.IndexColumnNameSuffix}] = '||{this.Sanatize(value)}||' ";
        //}

        public virtual string BuildPagingSelect(int? skip = null, int? take = null,
            int? defaultTakeSize = null, int? maxTakeSize = null)
        {
            if (!skip.HasValue && !take.HasValue)
            {
                return $" OFFSET 0 ROWS FETCH NEXT {defaultTakeSize.ToString()} ROWS ONLY; ";
            }

            if (!skip.HasValue)
            {
                skip = 0;
            }

            if (!take.HasValue && defaultTakeSize.HasValue)
            {
                take = defaultTakeSize;
            }

            if (take.Value > maxTakeSize && maxTakeSize.HasValue)
            {
                take = maxTakeSize;
            }

            if (skip == 0 && !take.HasValue)
            {
                return string.Empty;
            }

            return $" OFFSET {skip.Value} ROWS FETCH NEXT {take.Value} ROWS ONLY; ";
        }

        public virtual string BuildOrderingSelect(
            Expression expression = null,
            bool descending = false,
            IEnumerable<IIndexMap> indexMaps = null)
        {
            if (expression == null || indexMaps.IsNullOrEmpty())
            {
                return " ORDER BY [id] ";
            }

            var column = ExpressionHelper.GetPropertyName(expression);
            if (!column.EqualsAny(indexMaps?.Select(i => i.Name)))
            {
                return " ORDER BY [id] ";
            }

            return $" ORDER BY [{column}{this.IndexColumnNameSuffix}] DESC ";

            //if (sorting == SortColumn.IdDescending)
            //{
            //    return " ORDER BY [id] DESC ";
            //}

            //if (sorting == SortColumn.Key)
            //{
            //    return " ORDER BY [key] ";
            //}

            //if (sorting == SortColumn.KeyDescending)
            //{
            //    return " ORDER BY [key] DESC ";
            //}

            //if (sorting == SortColumn.Timestamp)
            //{
            //    return " ORDER BY [timestamp] ";
            //}

            //if (sorting == SortColumn.TimestampDescending)
            //{
            //    return " ORDER BY [timestamp] DESC ";
            //}

            // return " ORDER BY [id] ";
        }

        public string BuildFromTillDateTimeSelect(
            DateTime? fromDateTime = null,
            DateTime? tillDateTime = null)
        {
            var result = string.Empty;
            if (fromDateTime.HasValue)
            {
                result = $"{result} AND [timestamp] >= '{fromDateTime.Value.ToString("s")}'";
            }

            if (tillDateTime.HasValue)
            {
                result = $"{result} AND [timestamp] < '{tillDateTime.Value.ToString("s")}'";
            }

            return result;
        }

        public virtual string TableNamesSelect()
        {
            return @"
    SELECT QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) AS Name
    FROM INFORMATION_SCHEMA.TABLES";
        }

        public string ToDbType(Type type)
        {
            if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(bool))
            {
                return "bit";
            }
            else if (type == typeof(decimal))
            {
                return "decimal";
            }
            else if (type == typeof(long))
            {
                return "bigint";
            }
            else if (type == typeof(short))
            {
                return "smallint";
            }
            else if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(DateTime))
            {
                return "datetime";
            }
            else if (type == typeof(char))
            {
                return "NVARCHAR(1)";
            }

            return "NVARCHAR(2048)";
        }

        private string Sanatize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            value = value.Replace("'", string.Empty); // character data string delimiter
            value = value.Replace(";", string.Empty); // query delimiter
            value = value.Replace("--", string.Empty); // comment delimiter
            value = value.Replace("/*", string.Empty); // comment delimiter
            value = value.Replace("*/", string.Empty); // comment delimiter
            value = value.Replace("xp_", string.Empty); // comment delimiter
            return value;
        }

        private string AddStringMethodFromExpression(MethodCallExpression methodExpression, IEnumerable<IIndexMap> indexMaps = null)
        {
            var property = ExpressionHelper.GetProperty(methodExpression.Object);
            if (!property.Name.EqualsAny(indexMaps?.Select(i => i.Name)))
            {
                return string.Empty;
            }

            var value = (string)ExpressionHelper.GetValueFromExpression(methodExpression.Arguments[0], typeof(string));

            return methodExpression.Method.Name switch
            {
                "Contains" => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] LIKE '%{this.Sanatize(value)}%' ",
                "StartsWith" => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] LIKE '{this.Sanatize(value)}%' ",
                "EndsWith" => $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] LIKE '%{this.Sanatize(value)}' ",
                _ => throw new NotSupportedException($"The method {methodExpression.Method.Name} is not supported. Only Contains, StartWith and EndsWith are supported"),
            };
        }

        private string AddContainsFromExpression(MethodCallExpression methodExpression, IEnumerable<IIndexMap> indexMaps = null)
        {
            //var property = this.GetProperty(methodExpression.Arguments.Count == 1 ? methodExpression.Arguments[0] : methodExpression.Arguments[1]);
            //var values = (IEnumerable)this.GetValueFromExpression(methodExpression.Arguments.Count == 1 ? methodExpression.Object : methodExpression.Arguments[0], property.PropertyType);

            //return $" AND [{this.Sanatize(property.Name).ToLower()}{this.IndexColumnNameSuffix}] IN ({values.Select(a => )}) ";
            return string.Empty; // TODO
        }
    }
}
