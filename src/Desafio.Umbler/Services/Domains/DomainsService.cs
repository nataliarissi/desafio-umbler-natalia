using Desafio.Umbler.Models.ApiModels;
using Desafio.Umbler.Models.Entities;
using Desafio.Umbler.Models.ViewModels;
using Desafio.Umbler.Persistence;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services.Domains
{
    public class DomainsService : IDomainsService
    {
        private readonly DatabaseContext _db;
        private readonly LookupClient _lookup;
        private readonly ILogger<DomainsService> _logger;

        public DomainsService(DatabaseContext db, ILogger<DomainsService> logger)
        {
            _db = db;
            _lookup = new LookupClient();
            _logger = logger;
        }

        public async Task<Result<DomainViewModel>> GetDomainByName(string domainName)
        {
            try
            {
                var result = new Result<Domain>();

                var domain = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

                var domainDetails = await GetDomainDetails(domainName);

                if (domainDetails == null) 
                {
                    domain = new Domain(domainName, domainDetails);

                    _db.Domains.Add(domain);
                }

                if (DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)
                {
                    domain.UpdateDomain(domainName, domainDetails); 
                    _db.Domains.Update(domain);
                }

                await _db.SaveChangesAsync();
                
                return new Result<DomainViewModel>(new DomainViewModel(domain));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao obter um domínio pelo nome {domainName}", domainName);
                return new Result<DomainViewModel>("Não foi possível localizar o domínio informado. Contate o setor de suporte");
            }
        }

        private async Task<DomainQueryResult> GetDomainDetails(string domainName)
        {
            var result = new DomainQueryResult();

            var whoIsResponse = await WhoisClient.QueryAsync(domainName);
            result.WhoIs = whoIsResponse.Raw;

            var queryResult = await _lookup.QueryAsync(domainName, QueryType.ANY);
            var record = queryResult.Answers.ARecords().FirstOrDefault();

            result.Ttl = record?.TimeToLive ?? 0;
            result.Ip = record?.Address?.ToString();

            if (!string.IsNullOrWhiteSpace(result.Ip))
            {
                var hostResponse = await WhoisClient.QueryAsync(result.Ip);
                result.HostedAt = hostResponse.OrganizationName;
            }

            return result;
        }
    }
}