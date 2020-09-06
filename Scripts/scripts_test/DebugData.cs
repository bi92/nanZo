using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructCollection;
using UnityEngine.UI;
using System.IO;

public class DebugData : MonoBehaviour
{

    public DataCollection data_nan;
    
    [Header("json file name + path")]
    public string path_json;
    void Awake()
    {
        data_nan = new DataCollection();

        string json_folder = path_json;

        //get string txt from json file
        string jsonString = File.ReadAllText(Application.dataPath + json_folder);
        data_nan = JsonUtility.FromJson<DataCollection>(jsonString);

      

    }


}
