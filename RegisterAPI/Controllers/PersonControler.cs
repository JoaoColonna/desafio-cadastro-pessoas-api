using Microsoft.AspNetCore.Mvc;

namespace RegisterAPI.Controllers
{
    // V1
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        [HttpPost]
        public IActionResult Create(PersonDto dto)
        {
            var person = _personService.Create(dto);
            return Ok(person);
        }
    }

    // V2
    [ApiController]
    [Route("api/v2/[controller]")]
    public class PersonControllerV2 : ControllerBase
    {
        private readonly IPersonServiceV2 _personService;

        public PersonControllerV2(IPersonServiceV2 personService)
        {
            _personService = personService;
        }

        [HttpPost]
        public IActionResult Create(PersonV2Dto dto)
        {
            var person = _personService.Create(dto);
            return Ok(person);
        }
    }

}
