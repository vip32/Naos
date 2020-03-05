namespace Naos.Messaging.Application.Web
{
    using System.Collections.Generic;
    using System.Net;
    using EnsureThat;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Application;
    using Naos.Messaging.Domain;
    using NSwag.Annotations;

    [Route("naos/messaging/echo")]
    [ApiController]
    public class NaosMessagingEchoController : ControllerBase // or use normal middleware?  https://stackoverflow.com/questions/47617994/how-to-use-a-controller-in-another-assembly-in-asp-net-core-mvc-2-0?rq=1
    {
        private readonly ILogger<NaosMessagingEchoController> logger;
        private readonly IMessageBroker messageBroker;
        private readonly ISubscriptionMap subscriptionMap;

        public NaosMessagingEchoController(
            ILogger<NaosMessagingEchoController> logger,
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
        [OpenApiTag("Naos Echo")]
        public ActionResult<IEnumerable<SubscriptionDetails>> Get()
        {
            var message = new EchoMessage
            {
                CorrelationId = this.HttpContext.GetCorrelationId(),
                Text = $"echo ({this.HttpContext.GetRequestId()})"
            };

            this.messageBroker.Publish(message);

            return this.Ok(message);
            //return this.Ok(this.subscriptionMap?.GetAll());
        }
    }
}
