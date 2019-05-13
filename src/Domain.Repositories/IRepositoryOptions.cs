namespace Naos.Core.Domain.Repositories
{
    /// <summary>
    /// Various options for the <see cref="IGenericRepository{T}"/>.
    /// </summary>
    public interface IRepositoryOptions
    {
        IEntityMapper Mapper { get; set; }

        bool PublishEvents { get; set; }

        // VALIDATIONS?
        // DEFAULT SPECIFICATIONS/filters?
    }
}