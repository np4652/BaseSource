using BaseSource.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseSource.AppCode.Service
{
    public interface IDbContext
    {
        Task<List<BBPSOutlet>> BBPSOutletDetail(int top);
        Task<List<BBPSOutlet>> BBPSOutletDetail();
        Task<List<BBPSOutlet>> BBPOutletMobiles();
        Task LogError(string error, string origin);
        Task createLog(string response, string action = "", string type = "");
    }
}
