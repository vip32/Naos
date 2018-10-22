namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Various options to specify the <see cref="IRepository{T}"/> find operations
    /// </summary>
    /// <seealso cref="Naos.Core.Domain.IFindOptions" />
    public interface IFindOptions
    {
        int? Skip { get; set; }

        int? Take { get; set; }
    }
}