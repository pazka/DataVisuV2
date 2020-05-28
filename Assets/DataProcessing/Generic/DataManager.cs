using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class DataManager : IDataManager
{
    public abstract void Init(int screenBoundX, int screenBoundY);
    public abstract void Clean();
    public abstract IData GetNextData();
    public abstract IEnumerable<IData> GetAllData();
    public abstract dynamic GetDataBounds();
    public abstract IDataReader GetDataReader();
}
