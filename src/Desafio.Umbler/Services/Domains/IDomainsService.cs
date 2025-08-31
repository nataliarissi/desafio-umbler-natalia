using Desafio.Umbler.Models.ApiModels;
using Desafio.Umbler.Models.Entities;
using Desafio.Umbler.Models.ViewModels;
using System.Threading.Tasks;

namespace Desafio.Umbler.Services.Domains
{
    public interface IDomainsService
    {
        Task<Result<DomainViewModel>> GetDomainByName(string domainName);
    }
}
