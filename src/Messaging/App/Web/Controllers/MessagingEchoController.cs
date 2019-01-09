namespace Naos.Core.Messaging.App.Web.Controllers
{
    using System.Collections.Generic;
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common.Web;

    [Route("api/echo/messaging")]
    [ApiController]
    public class MessagingEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<MessagingEchoController> logger;
        private readonly IMessageBroker messageBroker;
        private readonly ISubscriptionMap subscriptionMap;

        public MessagingEchoController(
            ILogger<MessagingEchoController> logger,
            IMessageBroker messageBroker,
            ISubscriptionMap subscriptionMap)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.messageBroker = messageBroker;
            this.subscriptionMap = subscriptionMap;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<IEnumerable<SubscriptionDetails>> Get()
        {
            this.messageBroker.Publish(new TestMessage
            {
                CorrelationId = this.HttpContext.GetCorrelationId(),
                Data = $"echo ({this.HttpContext.GetRequestId()})"
            });

            return this.Ok(this.subscriptionMap?.GetAll());
        }
    }
}
