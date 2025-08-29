using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services.WhoIs
{
    public interface IWhoIsService
    {
        Task<WhoisResponse> QueryAsync(string query);
    }
}
