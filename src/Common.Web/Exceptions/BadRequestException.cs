namespace Naos.Core.Common.Web
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// Bad request exception type for exceptions thrown by Naos api controllers.
    /// </summary>
    [Serializable]
    public class BadRequestException : HttpException
    {
        public BadRequestException(ModelStateDictionary modelState)
            : base(HttpStatusCode.BadRequest)
        {
            this.ModelState = modelState;
        }

        public BadRequestException()
            : base(HttpStatusCode.BadRequest)
        {
        }

        public BadRequestException(string message)
            : base(HttpStatusCode.BadRequest, message)
        {
        }

        public ModelStateDictionary ModelState { get; }
    }
}
