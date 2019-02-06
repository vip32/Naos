namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class NextPageResult
    {
        public bool Success { get; set; }

        public bool HasMore { get; set; }

        public IReadOnlyCollection<FileInformation> Files { get; set; }

        public Func<Task<NextPageResult>> NextPageFunc { get; set; }
    }
}
