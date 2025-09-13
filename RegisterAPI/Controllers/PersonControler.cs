using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Application.DTOs;

namespace RegisterAPI.Controllers
{
    // V1
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Requer autenticação
    public class PersonController : ControllerBase
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        [HttpPost]
        public IActionResult Create(        PersonDto dto)
        {
            var person = _personService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = person.Id }, person);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var people = _personService.GetAll();
            return Ok(people);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var person = _personService.GetById(id);
            if (person == null) return NotFound("Pessoa não encontrada.");
            return Ok(person);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, PersonDto dto)
        {
            var updated = _personService.Update(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _personService.Delete(id);
            return NoContent();
        }
    }

    // V2
    [ApiController]
    [Route("api/v2/[controller]")]
    [Authorize] // Requer autenticação
    public class PersonV2 : ControllerBase
    {
        private readonly IPersonServiceV2 _personService;

        public PersonV2(IPersonServiceV2 personService)
        {
            _personService = personService;
        }

        [HttpPost]
        public IActionResult Create(PersonV2Dto dto)
        {
            var person = _personService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = person.Id }, person);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var people = _personService.GetAll();
            return Ok(people);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var person = _personService.GetById(id);
            if (person == null) return NotFound("Pessoa não encontrada.");
            return Ok(person);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, PersonV2Dto dto)
        {
            var updated = _personService.Update(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _personService.Delete(id);
            return NoContent();
        }
    }
}
