using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Desafio.Umbler.Services.Domains;
using Desafio.Umbler.Models.ApiModels;
using Desafio.Umbler.Attributes;
using Desafio.Umbler.Models.Entities;

namespace Desafio.Umbler.Controllers
{
    [Route("api/domains")]
    public class DomainController : Controller
    {
        private readonly IDomainsService _service;

        public DomainController(IDomainsService service)
        {
            _service = service;
        }

        [HttpGet, Route("{domainName}")]
        public async Task<IActionResult> Get([DomainOrIp(ErrorMessage = "Domínio ou IP informado é inválido")] string domainName)
        {
            if (!ModelState.IsValid)
                return GenerateErrorResult<Domain>();

            var result = await _service.GetDomainByName(domainName);

            return GenerateResult(result);
        }

        private IActionResult GenerateErrorResult<T>()
        {
            var errorResult = new Result<T>();
            foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
            {
                errorResult.Messages.Add(new Message(MessageType.Error, modelError.ErrorMessage));
            }
            return BadRequest(errorResult);
        }

        private IActionResult GenerateResult<T>(Result<T> result)
        {
            if (result.Messages.Any(x => x.Type == MessageType.Error))
                return BadRequest(result);
            return Ok(result);
        }
    }
}
