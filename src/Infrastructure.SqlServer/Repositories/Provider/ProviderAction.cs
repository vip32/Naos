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

    public enum ProviderAction
    {
        /// <summary>
        /// None
        /// </summary>
        None,

        /// <summary>
        /// Inserted
        /// </summary>
        Inserted,

        /// <summary>
        /// Updated
        /// </summary>
        Updated,

        /// <summary>
        /// Deleted
        /// </summary>
        Deleted
    }
}
