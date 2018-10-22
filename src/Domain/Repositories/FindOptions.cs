namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Various options to specify the <see cref="IRepository{T}"/> find operations
    /// </summary>
    /// <seealso cref="Naos.Core.Domain.IFindOptions" />
    public class FindOptions : IFindOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FindOptions"/> class.
        /// </summary>
        /// <param name="skip">The skip amount.</param>
        /// <param name="take">The take amount.</param>
        public FindOptions(int? skip = null, int? take = null)
        {
            this.Take = take;
            this.Skip = skip;
        }

        public int? Take { get; set; }

        public int? Skip { get; set; }
    }
}