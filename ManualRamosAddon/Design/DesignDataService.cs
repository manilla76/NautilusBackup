﻿using ManualRamosAddon.Model;
using System;
using System.Threading.Tasks;

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