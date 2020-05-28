using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public interface IDataReader
{
    void Init();
    void Clean();
    
    IData GetData();
    void GoToNextData();
}
