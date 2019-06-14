namespace Naos.Foundation.Application
{
    using System;
    using System.Net;

    /// <summary>
    /// Bad request exception type for exceptions thrown by Naos api controllers.
    /// </summary>
    [Serializable]
    public class NotFoundException : HttpException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException()
            : base(HttpStatusCode.NotFound)
        {
        }
    }
}
