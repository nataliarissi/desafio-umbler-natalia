using System;
using System.ComponentModel.DataAnnotations;

namespace Desafio.Umbler.Models.Entities
{
    public class Domain
    {
        public Domain()
        {
            UpdatedAt = DateTime.Now;
        }

        public Domain(string domainName, DomainQueryResult domain)
        {
            Name = domainName;
            Ip = domain.Ip;
            WhoIs = domain.WhoIs;
            Ttl = domain.Ttl;
            HostedAt = domain.HostedAt;
            UpdatedAt = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string WhoIs { get; set; }
        public int Ttl { get; set; }
        public string HostedAt { get; set; }

        public void UpdateDomain(string domainName, DomainQueryResult domain)
        {
            Name = domainName;
            Ip = domain.Ip;
            WhoIs = domain.WhoIs;
            Ttl = domain.Ttl;
            HostedAt = domain.HostedAt;
            UpdatedAt = DateTime.Now;
        }
    }
}
