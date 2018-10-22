namespace Naos.Core.Domain
{
    using AutoMapper;

    /// <summary>
    /// Various options for the <see cref="IRepository{T}"/>
    /// </summary>
    public interface IRepositoryOptions
    {
        IMapper Mapper { get; set; }

        bool PublishEvents { get; set; }

        // VALIDATIONS?
    }
}