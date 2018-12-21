namespace Naos.Core.Common.Web
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Custom action result for internal server errors (500)
    /// </summary>
    /// <seealso cref="ObjectResult" />
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object error)
            : base(error)
        {
            this.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}
