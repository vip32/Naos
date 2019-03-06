namespace Naos.Core.Domain.Repositories
{
    using Naos.Core.Common.Mapping;

    /// <summary>
    /// Various options for the <see cref="IRepository{T}"/>
    /// </summary>
    public interface IRepositoryOptions
    {
        IMapper Mapper { get; set; }

        bool PublishEvents { get; set; }

        // VALIDATIONS?
        // DEFAULT SPECIFICATIONS/filters?
    }
}