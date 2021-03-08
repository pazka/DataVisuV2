using System.Collections.Generic;

namespace DataProcessing.Generic
{
    public  abstract class DataConverter : IDataManager
    {
        public abstract void Init(int screenBoundX, int screenBoundY);
        public abstract void Clean();
        public abstract IData GetNextData();
        public abstract IEnumerable<IData> GetAllData();
        public abstract IData[] GetDataBounds();
        public abstract IDataReader GetDataReader();
    }
}
