using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public interface IDataReader
{
    /// <summary>
    /// Create the runtime variable used later by the entity
    /// Manage the init position of a data cursor
    /// 
    /// </summary>
    void Init();

    /// <summary>
    /// Clear the necessary ressources : 
    /// Connexion / Objects / Semaphores etc...
    /// 
    /// </summary>
    void Clean();
    
    /// <summary>
    /// Get Data at the current cursor
    /// </summary>
    IData GetData();

    /// <summary>
    /// Move the cursor
    /// </summary>
    void GoToNextData();
}
