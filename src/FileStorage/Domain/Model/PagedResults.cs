namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Naos.Core.Common;

    public class PagedResults : IHasNextPageFunc
    {
        public static readonly IReadOnlyCollection<FileInformation> Empty = new ReadOnlyCollection<FileInformation>(new FileInformation[0]);
        public static PagedResults EmptyResults = new PagedResults(Empty);

        public PagedResults(
            IReadOnlyCollection<FileInformation> files)
        {
            this.Files = files;
            this.HasMore = false;
            ((IHasNextPageFunc)this).NextPageFunc = null;
        }

        public PagedResults(
            IReadOnlyCollection<FileInformation> files,
            bool hasMore,
            Func<Task<NextPageResult>> nextPageFunc)
        {
            this.Files = files;
            this.HasMore = hasMore;
            ((IHasNextPageFunc)this).NextPageFunc = nextPageFunc;
        }

        public PagedResults(Func<Task<NextPageResult>> nextPageFunc)
        {
            ((IHasNextPageFunc)this).NextPageFunc = nextPageFunc;
        }

        public IReadOnlyCollection<FileInformation> Files { get; private set; }

        public bool HasMore { get; private set; }

        Func<Task<NextPageResult>> IHasNextPageFunc.NextPageFunc { get; set; }

        public async Task<bool> NextPageAsync()
        {
            if(((IHasNextPageFunc)this).NextPageFunc == null)
            {
                return false;
            }

            var result = await ((IHasNextPageFunc)this).NextPageFunc().AnyContext();
            if(result.Success)
            {
                this.Files = result.Files;
                this.HasMore = result.HasMore;
                ((IHasNextPageFunc)this).NextPageFunc = result.NextPageFunc;
            }
            else
            {
                this.Files = Empty;
                this.HasMore = false;
                ((IHasNextPageFunc)this).NextPageFunc = null;
            }

            return result.Success;
        }
    }
}
