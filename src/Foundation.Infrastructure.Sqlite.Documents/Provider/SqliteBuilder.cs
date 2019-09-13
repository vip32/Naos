namespace Naos.Foundation.Infrastructure
{
    using Microsoft.Extensions.Logging;

    public class SqliteBuilder : SqlBuilder
    {
        public SqliteBuilder(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override string BuildPagingSelect(
            int? skip = null,
            int? take = null,
            int? defaultTakeSize = null,
            int? maxTakeSize = null)
        {
            if (!skip.HasValue && !take.HasValue)
            {
                return $" ORDER BY [key] LIMIT {skip},{defaultTakeSize}";
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

            return $" ORDER BY [key] LIMIT {skip.Value},{take.Value}";
        }

        public override string TableNamesSelect()
        {
            return @"SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
        }
    }
}
