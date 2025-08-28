using Desafio.Umbler.Models.ApiModels;
using Desafio.Umbler.Models.Entities;
using System.Threading.Tasks;

namespace Desafio.Umbler.Services.Domains
{
    public interface IDomainsService
    {
        Task<Result<Domain>> GetDomainByName(string domainName);
    }
}
