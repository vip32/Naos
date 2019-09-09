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

    public interface IIndexMap<T>
    {
        string Description { get; set; }

        string Name { get; set; }

        Func<T, object> Value { get; set; }

        Func<T, IEnumerable<object>> Values { get; set; }
    }
}
