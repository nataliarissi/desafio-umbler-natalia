using Desafio.Umbler.Models.Entities;
using System;

namespace Desafio.Umbler.Models.ViewModels
{
    public class DomainViewModel
    {
        public DomainViewModel()
        {
             
        }

        public DomainViewModel(Domain domain)
        {
            Name = domain.Name;
            Ip = domain.Ip;
            HostedAt = domain.HostedAt;
        }

        public string Name { get; set; }
        public string Ip { get; set; }
        public string HostedAt { get; set; }
    }
}