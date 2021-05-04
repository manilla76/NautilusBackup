using System;

namespace ThermoDpSQLReader.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
        string ReadSQL(string pileToFind);
    }
}
