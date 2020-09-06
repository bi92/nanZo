using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class Test_parsing : MonoBehaviour
{

    #region messageType

    [System.Serializable]
    public class S_message
    {
        public float delay;

       
        public txt_Struct txt_const;

    
    }


    [System.Serializable]
    public struct txt_Struct
    {
       public string en;
        public string ko;

    }

    #endregion


    public string path_json;

    [System.Serializable]
    public struct C_trigger
    {
        public string type;
        public double latitude;
        public double longitude;
        public float range;

    }

    [System.Serializable]
   public class Dialog_01
   {
        public string title;
        public string description;
        public int created_date;
        public int version;

        public C_trigger[] trigger;
        public S_message[] messages;
    }

   
    [System.Serializable]
    public class triggerList
    {

        public C_trigger[] trigger;
    }

    [System.Serializable]
    public class Dialog_list
    {
        public Dialog_01[] dialogs;

    }



    //debugging inspector
    
    public triggerList triggertest_01;
    public Dialog_list debug_dialogs;

    private void Start()
    {
      
    

        #region load json data


         string json_folder = path_json;

        //var jtc3 = LoadJsonFile<Dialog_01>(Application.dataPath + json_folder, "NanAndJo");

        //jtc3.DebugParsing();


        #endregion

        //get string txt from json file
        string jsonString = File.ReadAllText(Application.dataPath + json_folder+ "/NanAndJo.json");
       triggerList triggerList_01 = JsonUtility.FromJson<triggerList>(jsonString);

        Dialog_list _Dialog_01 = JsonUtility.FromJson<Dialog_list>(jsonString);

        Debug.Log("trigger list의 갯수" + triggerList_01.trigger.Length);

   
        //copy for debugging
        triggertest_01 = triggerList_01;
        debug_dialogs = _Dialog_01;

        
    }

     
    #region ext

    //object ->json
    string ObjectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    //json -> object
    T JsonToOject<T>(string jsonData)
    {
        return JsonUtility.FromJson<T>(jsonData);

    }


    void CreateJsonFile(string createPath, string fileName, string jsonData)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", createPath, fileName), FileMode.Create);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }



    //load json data
    T LoadJsonFile<T>(string loadPath, string fileName)
    {
        FileStream fileStream = new FileStream(string.Format("{0}/{1}.json", loadPath, fileName), FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        string jsonData = Encoding.UTF8.GetString(data);

        return JsonUtility.FromJson<T>(jsonData);

    }

    #endregion


}
