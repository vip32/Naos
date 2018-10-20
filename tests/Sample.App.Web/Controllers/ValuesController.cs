namespace Naos.Sample.App.Web.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly AppConfiguration configuration;

        public ValuesController(IOptions<AppConfiguration> configuration)
        {
            EnsureThat.EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureThat.EnsureArg.IsNotNull(configuration.Value, nameof(configuration.Value));

            this.configuration = configuration.Value;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { $"{this.configuration.Name} 1", $"{this.configuration.Name} 2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
