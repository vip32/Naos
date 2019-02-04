//namespace Naos.Core.Common.Web
//{
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.Extensions.DependencyInjection;

//    [ApiController]
//    [Route("api/[controller]/[action]")]
//    public abstract class BaseController : Controller
//    {
//        private IMediator _mediator;

//        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
//    }
//}
