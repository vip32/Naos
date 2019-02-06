namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.Threading.Tasks;

    public interface IHasNextPageFunc
    {
        Func<Task<NextPageResult>> NextPageFunc { get; set; }
    }
}
