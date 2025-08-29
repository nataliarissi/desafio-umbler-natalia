using System.Threading.Tasks;
using Whois.NET;

namespace Desafio.Umbler.Services.WhoIs
{
    public class WhoIsService : IWhoIsService
    {
        public async Task<WhoisResponse> QueryAsync(string query)
        {
            return await WhoisClient.QueryAsync(query);
        }
    }
}
