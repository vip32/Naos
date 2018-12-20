//namespace Naos.Core.Common.Web
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Net.Http;
//    using Microsoft.Extensions.Logging;

//    public class HttpMessageHandlerBuilder // https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
//    {
//        private readonly IList<HttpMessageHandler> handlers = new List<HttpMessageHandler>();
//        private readonly HttpClientLogHandler rootHandler;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="HttpMessageHandlerBuilder"/> class.
//        /// </summary>
//        public HttpMessageHandlerBuilder()
//        {
//            this.rootHandler = new HttpClientLogHandler();
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="HttpMessageHandlerBuilder"/> class.
//        /// </summary>
//        /// <param name="logger">Logger.</param>
//        public HttpMessageHandlerBuilder(ILogger logger)
//        {
//            this.rootHandler = new HttpClientLogHandler(logger);
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="HttpMessageHandlerBuilder"/> class.
//        /// </summary>
//        /// <param name="rootHandler">root (last) handler.</param>
//        public HttpMessageHandlerBuilder(HttpClientLogHandler rootHandler)
//        {
//            this.rootHandler = rootHandler;
//        }

//        /// <summary>
//        /// Adds a <see cref="HttpMessageHandler"/> to the chain of handlers.
//        /// </summary>
//        /// <param name="handler"></param>
//        /// <returns></returns>
//        public HttpMessageHandlerBuilder Add(HttpMessageHandler handler)
//        {
//            if (handler is HttpClientLogHandler)
//            {
//                throw new ArgumentException($"Can't add handler of type {nameof(HttpClientLogHandler)}.");
//            }

//            if (this.handlers.Count > 0)
//            {
//                ((DelegatingHandler)this.handlers.LastOrDefault()).InnerHandler = handler;
//            }

//            this.handlers.Add(handler);
//            return this;
//        }

//        /// <summary>
//        /// Builds the chain and adds the root handler as the last handler.
//        /// </summary>
//        /// <returns></returns>
//        public HttpMessageHandler Build()
//        {
//            if (this.handlers.Count > 0)
//            {
//                ((DelegatingHandler)this.handlers.LastOrDefault()).InnerHandler = this.rootHandler;
//            }
//            else
//            {
//                return this.rootHandler;
//            }

//            return this.handlers.FirstOrDefault();
//        }
//    }
//}
