namespace Naos.Core.Domain
{
    /// <summary>
    /// Various options to specify the <see cref="IRepository{T}"/> find operations
    /// </summary>
    public interface IFindOptions
    {
        int? Skip { get; set; }

        int? Take { get; set; }
    }
}