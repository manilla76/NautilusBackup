using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualRamosAddon.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
        Task<string> GetDataAsync();
    }
}
