using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// The data Manager will make the bridge between 
/// the application layer and the data aquisition layer.
/// You will start to see graphic specific logic that use the Data
/// Logic
/// 
/// </summary>

public interface IDataManager
{
    void Init(int screenBoundX, int screenBoundY);
    void Clean();
    IData GetNextData();
    IEnumerable<IData> GetAllData();
    object GetDataBounds();
    IDataReader GetDataReader();
}
