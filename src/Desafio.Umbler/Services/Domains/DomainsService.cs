using Desafio.Umbler.Models.ApiModels;
using Desafio.Umbler.Models.Entities;
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

        public async Task<Result<Domain>> GetDomainByName(string domainName)
        {
            try
            {
                var result = new Result<Domain>();

                var domain = await _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName);

                if (domain == null)
                {
                    var response = await WhoisClient.QueryAsync(domainName);

                    var queryResult = await _lookup.QueryAsync(domainName, QueryType.ANY);
                    var record = queryResult.Answers.ARecords().FirstOrDefault();
                    var address = record?.Address;
                    var ip = address?.ToString();

                    var hostResponse = await WhoisClient.QueryAsync(ip);

                    domain = new Domain
                    {
                        Name = domainName,
                        Ip = ip,
                        WhoIs = response.Raw,
                        Ttl = record?.TimeToLive ?? 0,
                        HostedAt = hostResponse.OrganizationName
                    };

                }

                if (DateTime.Now.Subtract(domain.UpdatedAt).TotalMinutes > domain.Ttl)
                {
                    var response = await WhoisClient.QueryAsync(domainName);

                    var resultAAA = await _lookup.QueryAsync(domainName, QueryType.ANY);
                    var record = resultAAA.Answers.ARecords().FirstOrDefault();
                    var address = record?.Address;
                    var ip = address?.ToString();

                    var hostResponse = await WhoisClient.QueryAsync(ip);

                    domain.Name = domainName;
                    domain.Ip = ip;
                    domain.UpdatedAt = DateTime.Now;
                    domain.WhoIs = response.Raw;
                    domain.Ttl = record?.TimeToLive ?? 0;
                    domain.HostedAt = hostResponse.OrganizationName;
                }

                _db.Domains.Add(domain);
                await _db.SaveChangesAsync();

                return new Result<Domain>(domain);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "", domainName);
                return null;
            }
        }
    }
}
