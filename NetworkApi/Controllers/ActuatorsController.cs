/*
 * Actuators controller.
 *
 * @author Michel Megens
 * @email  michel@michelmegens.net
 */

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SensateService.NetworkApi.Controllers
{
    [ApiController]
	[Produces("application/json")]
	[Route("[controller]")]
    public class ActuatorsController : ControllerBase
    {
        // GET: api/Actuators
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Actuators/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Actuators
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Actuators/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
