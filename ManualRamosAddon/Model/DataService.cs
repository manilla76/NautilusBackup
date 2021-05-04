using System;
using System.Threading.Tasks;

namespace ManualRamosAddon.Model
{
    public class DataService : IDataService
    {
        public void GetData(Action<DataItem, Exception> callback)
        {
            // Use this to connect to the actual data service

            var item = new DataItem();
            callback(item, null);
        }

        public async Task<string> GetDataAsync()
        {
            throw new NotImplementedException();
            await Task.Delay(1);
        }
    }
}