namespace Naos.Core.RequestFiltering.App
{
    /// <summary>
    /// Provides access to the <see cref="FilterContext"/> for the current request.
    /// </summary>
    public interface IFilterContextAccessor
    {
        /// <summary>
        /// The <see cref="FilterContext"/> for the current request.
        /// </summary>
        FilterContext Context { get; set; }
    }
}
