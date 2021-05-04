using System;
using System.Threading.Tasks;

namespace ManualRamosAddon.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
        Task<string> GetDataAsync();
    }
}
