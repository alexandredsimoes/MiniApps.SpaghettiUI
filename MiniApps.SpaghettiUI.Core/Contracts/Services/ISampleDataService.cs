using System.Collections.Generic;
using System.Threading.Tasks;

using MiniApps.SpaghettiUI.Core.Models;

namespace MiniApps.SpaghettiUI.Core.Contracts.Services
{
    public interface ISampleDataService
    {
        Task<IEnumerable<SampleOrder>> GetMasterDetailDataAsync();
    }
}
