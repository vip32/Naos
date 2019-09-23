namespace Naos.RequestFiltering.App
{
    using System.Threading;

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