namespace Naos.Foundation.Infrastructure
{
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public class SqlServerDocumentProvider<T>
    {
        private readonly ILogger<SqlServerDocumentProvider<T>> logger;
        private readonly SqlServerDocumentProviderOptions options;

        public SqlServerDocumentProvider(SqlServerDocumentProviderOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNullOrEmpty(options.ConnectionString, nameof(options.ConnectionString));

            this.options = options;
            this.logger = options.CreateLogger<SqlServerDocumentProvider<T>>();
        }

        public SqlServerDocumentProvider(Builder<SqlServerDocumentProviderOptionsBuilder, SqlServerDocumentProviderOptions> optionsBuilder)
            : this(optionsBuilder(new SqlServerDocumentProviderOptionsBuilder()).Build())
        {
        }
    }
}
