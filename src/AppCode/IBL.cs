using BaseSource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseSource.AppCode
{
    public interface IBL
    {
        Task<List<MessageResponseList>> PostDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey);
        Task<List<MessageResponseList>> PostAlternateDataAsync(string _namespace, string name, List<string> head, List<string> body, string apiKey);

        Task<string> PostInitialForm3DataAsync(InitialForm3 request);
        Task<List<BBPSOutlet>> BBPSOutletDetail(int top);
    }
}
