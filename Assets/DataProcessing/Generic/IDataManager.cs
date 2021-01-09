using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// The data Manager will make the bridge between 
/// the application(visual) layer and the data aquisition layer.
/// 
/// It has no knowlege fo Data reading but start to create collections of Data
/// and register the meta-data info like boundaries, count, Type of collection etc...
/// 
/// It also hold a few of the Application logic, meaning the visuals. 
/// You will start to see graphic specific logic that use the Data Collection Logic
/// 
/// </summary>

public interface IDataManager
{
    void Init(int screenBoundX, int screenBoundY);
    void Clean();
    IData GetNextData();
    IEnumerable<IData> GetAllData();
    IData[] getDataBounds();
    IDataReader GetDataReader();
}
