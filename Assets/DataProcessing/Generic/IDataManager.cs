using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataManager
{
    void Init(int screenBoundX, int screenBoundY);
    void Clean();
    IData GetNextData();
    IEnumerable<IData> GetAllData();
    dynamic GetDataBounds();
    IDataReader GetDataReader();
}
