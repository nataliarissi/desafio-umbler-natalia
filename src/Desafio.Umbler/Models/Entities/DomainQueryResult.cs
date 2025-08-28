using System;

namespace Desafio.Umbler.Models.Entities
{
    public class DomainQueryResult
    {
        public string Ip { get; set; }
        public string WhoIs { get; set; }
        public int Ttl { get; set; }
        public string HostedAt { get; set; }
    }
}
