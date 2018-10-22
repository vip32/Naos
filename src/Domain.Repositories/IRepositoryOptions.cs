namespace Naos.Core.Domain.Repositories
{
    /// <summary>
    /// Various options for the <see cref="IRepository{T}"/>
    /// </summary>
    public interface IRepositoryOptions
    {
        IEntityMapper Mapper { get; set; }

        bool PublishEvents { get; set; }

        // VALIDATIONS?
        // DEFAULT SPECIFICATIONS
    }
}