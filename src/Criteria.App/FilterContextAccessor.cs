namespace Naos.Core.Filtering.App
{
    using System.Threading;
    using Naos.Core.Common.Web;

    /// <inheritdoc />
    public class FilterContextAccessor : IFilterContextAccessor
    {
        private static readonly AsyncLocal<FilterContext> Data = new AsyncLocal<FilterContext>();

        /// <inheritdoc />
        public FilterContext Context
        {
            get => Data.Value;
            set => Data.Value = value;
        }
    }
}