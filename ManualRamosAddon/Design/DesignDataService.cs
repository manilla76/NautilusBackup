using System;
using ManualRamosAddon.Model;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ManualRamosAddon.Design
{
    public class DesignDataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to create design time data

            var item = new DataItem();
            callback(item, null);
        }

        public Task<string> GetDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}