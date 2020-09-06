using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructCollection;
using UnityEngine.UI;
using System.IO;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class DataSorter : MonoBehaviour
{

    public DataCollection nan;
    public DataCollection jo;

    [Header("디버그용/다이얼로그 시작 인덱스 ")]
    public int debug_dialogStartIndex;

    [Header("json file name + path")]
    public string path_json;
    SourceLoader sourceLoader;
    public bool setOriginValue;
    int settingDialogNumber;
    public GpsManager gpsmanager;

    public string selectedCharacterName;
    [HideInInspector]
    public Dialog[] curDialogs
    {
        get { return _curDialogs; }
        set
        {

            _curDialogs = value;

            debug_dialogForType = _curDialogs;
        }
    }

    public Dialog[] debug_dialogForType;
     Dialog[] _curDialogs;

   
    [Header("현재 진행중인 다이얼로그 인덱스")]
    public int _curDialogIndex =0;
    // 현재 다이얼로그 인덱스
    public int curDialogIndex
    {
        get { return _curDialogIndex; }
        set
        {
            if(value!= _curDialogIndex|| value == 0)
            {
               
                if(value ==0)
                {
                    OnCurDialogChanged += ChangeDialogIndexEvented;
                }

                if (value >=curDialogs.Length)
                {
                    _curDialogIndex = value;
                    Debug.Log("dialog is end");
                    d_sequencer.curDialogState = DialogSequencer.DialogState.endDialog;
                  
                }
                else
                {
                  
                    _curDialogIndex = value;
                    OnCurDialogChanged();
                    curContentList = curDialogs[_curDialogIndex].contents;

                }

                DialogSequencer.isEnterTriggerArea = false;
            
            }
        }
    }


    //data source to download - video + 3d(bundle)
    //public Source[] list_source;

    //deebugging
    Content[] debugContent;

    //get set content for sort type;
    Content[] _curContentList;
    public List<Source> List_bundle;

    #region event Area
    //event call when datatype setting is over 
    public delegate void ChangeCurDialog();
    public event ChangeCurDialog OnCurDialogChanged;


  
    #endregion

    public SelectManager selectmanager;

    #region LoadingArea
    
    public int _loadableSourceNumber =0;
    public int loadableSourceNumber
    {
        get { return _loadableSourceNumber; }
        set
        {
            if(value != _loadableSourceNumber)
            {
                _loadableSourceNumber = value;
              
            }
        }
    }

  

    Source[] list_jsonSources;
    //source 다운로드 순서리스트
    public List<Content> list_DownloadContent;

  
    public List<object> list_DownloadObject;
    public List<object> list_sources; 
    public List<object> list_LoadObject; 

    #endregion

    public Content[] curContentList
    {
        get {

            return _curContentList; 
        }
        set {
          
            _curContentList = value;
            debugContent = _curContentList;
        }
    }

    public static DataSorter instance;

    //gameobject
    public DialogSequencer d_sequencer;

    public AudioClip[] debug_audio;

    public AudioSource a_source;
    public bool jsonReadEnd;
    void Awake()
    {
        SourceLoader.isPermissionChecked = false;
        // d_sequencer.enabled = false;
        List_bundle = new List<Source>();
        list_LoadObject = new List<object>();
        jsonReadEnd = false;
        var obj = FindObjectsOfType<DataSorter>();
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        instance = this;

        loadableSourceNumber = 0;
        sourceLoader = this.GetComponent<SourceLoader>();
        list_sources = new List<object>();
        list_DownloadObject = new List<object>();


    }

    [HideInInspector]
    public int dataOrder;

    //values need for loading animation
    [HideInInspector]
    public int loadingCount;


    //version debug
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            PlayerPrefs.DeleteKey("nan");
            //PlayerPrefs.DeleteKey("jo");
        }

    }

    public void OnStartAfterJson(string jsonPath, int order)
    {
       
        list_DownloadContent = new List<Content>();
        

        a_source = this.GetComponent<AudioSource>();

        dataOrder = order;
        //get data froma json file 
        switch (order)
        {
            case 0:
                nan.data.dataOrder = 0;
                nan.data.dataName = "nan";
                GetDataFromJson(jsonPath, out nan, "nan");
                break;

            case 1:
                jo.data.dataOrder = 1;
                jo.data.dataName = "jo";
                GetDataFromJson(jsonPath, out jo, "jo");
                break;

        }

        
    }

   
    void GetDataFromJson(string path, out DataCollection outData,string dataname)
    {

        outData = new DataCollection();
        //get string txt from json file
        string jsonString = File.ReadAllText(path);

        Debug.Log("get data from ->"+path);
        //origin data
        outData = JsonUtility.FromJson<DataCollection>(jsonString);

        bool isUpdated = IsJsonVersionUpdated(dataname, outData.data.file_info.version);
        //set values for type
        for (int i = 0; i < outData.data.dialogs.Length; i++)
        {
            
            settingDialogNumber = i;
            SetTriggerType(outData.data.dialogs[i].trigger, out outData.data.dialogs[i].trigger.u_type);
            sourceLoader.loadEnd = true;
            outData.data.dialogs[i].dataName = dataname;
            for (int a = 0; a < outData.data.dialogs[i].contents.Length; a++)
            {
                outData.data.dialogs[i].contents[a].dataName = dataname;
                outData.data.dialogs[i].contents[a].isUpdate = isUpdated;
                SetContentType(outData.data.dialogs[i].contents[a]);
                if (i >= outData.data.dialogs.Length - 1 && a == outData.data.dialogs[i].contents.Length - 1)
                {
                    setOriginValue = true;
                    sourceLoader.loadEnd = true;
                }
            }
        }

         if(outData.data.sources !=null)
         {
            Debug.Log(outData.data.sources[0].name);
            for (int i = 0; i < outData.data.sources.Length; i++)
            {

                outData.data.sources[i].dataName = dataname;
                outData.data.sources[i].isUpdate = isUpdated;
                list_sources.Add(outData.data.sources[i]);
                
            }

         }
    
        //check json version
        
        Debug.Log(dataname + "소스 카운트" + list_sources.Count);
      

        //sort 완료
        if (dataOrder ==1)
        {

            for (int i = 0; i < list_sources.Count; i++)
            {
                DeleteFileExVersion(list_sources[i]);
            }

            for (int i = 0; i < list_sources.Count; i++)
            {
               
                CheckFileExist(list_sources[i]);
            }

            jsonReadEnd = true;
            Debug.Log("list_LoadObject 갯수는 " + list_LoadObject.Count+ "다운로드 받아야 할 갯수 " + list_DownloadObject.Count);


            if(!sourceLoader.isNeedToDownload)
            {

                StartCoroutine(SourceLoader.instance.LoadSourceSequencer());

                if (list_LoadObject.Count ==0)
                {

                }
                else
                {
                 
                }
              
            }
          
         
        }

       
        dataOrder++;
        sourceLoader.jsonSort = true;
        //curDialogs = outData.data.dialogs;
        //list_jsonSources = outData.data.sources;

    }

    void CheckFileExist( object origin)
    {
        string path = null;
        var type = origin.GetType();

        bool isbundleType = false;
        if (type == typeof(Content))
        {
            Content _content = (Content)origin;
            path = _content.audio.sourcelink;

            //put result link on 
        }
        else if (type == typeof(Source))
        {
            Source source = (Source)origin;
            path = source.sourcelink;
            if(source.sourcetype == "bundle")
            {
                isbundleType = true;
            }
        }


        string[] files = path.Split('/');
        string file = files[files.Length - 1];
        string targetPath = Application.persistentDataPath + "/" + file;

        if(isbundleType)
        {
            targetPath = targetPath +".unity3d";
            Debug.Log("+++++++++++++++++bundle local path"+targetPath);
        }

        if (!File.Exists(targetPath))
        {
            Debug.Log("name =" + file + "need to download" + "list_DownloadObject index ->" + list_DownloadObject.Count);
            Debug.Log("targetPath =" + targetPath);
            //put download list
            list_DownloadObject.Add(origin);
        }
        else
        {
          
            Debug.Log("name =" + file + "already exist" + "list_LoadObject index ->" + list_LoadObject.Count);
            list_LoadObject.Add(origin);
            
        }

           
    }


    void DeleteFileExVersion(object origin)
    {
        string path = null;
        var type = origin.GetType();

        bool isbundleType = false;

        bool isVersionChanged = false;
        if (type == typeof(Content))
        {
            Content _content = (Content)origin;
            path = _content.audio.sourcelink;
            isVersionChanged = _content.isUpdate;

            //put result link on 
        }
        else if (type == typeof(Source))
        {
            Source source = (Source)origin;
            path = source.sourcelink;
            if (source.sourcetype == "bundle")
            {
                isbundleType = true;
            }
            isVersionChanged = source.isUpdate;
        }


        string[] files = path.Split('/');
        string file = files[files.Length - 1];
        string targetPath = Application.persistentDataPath + "/" + file;

        if (isbundleType)
        {
            targetPath = targetPath + ".unity3d";
            Debug.Log("+++++++++++++++++bundle local path" + targetPath);
        }

        if (isVersionChanged)
        {
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
                Debug.Log("deleteFile" + file);
            }

        }
      
    }


    void SetDialogIndex()
    {
        Debug.Log("dialogindex is set");
     
    }

    void SetContentType( Content data )
    {
        
        contentType dataType = contentType.none;

        
        //message
        if (data.message.txt_const.ko != null)
         {
            if (dataType == contentType.none)
            {
                data.type = contentType.message;

            }
            else
            {
                data.type = contentType.multiple;

            }
        }
      
       else if(data.button.option.ko != null)
        {
            if (dataType == contentType.none)
            {
                data.type = contentType.button;
                SetActionType(data, data.button.action, out data.button.type);
            }
            else
            {
                data.type = contentType.multiple;

            }
        }
        
        else if (data.audio.autoplay)
        {
            if (dataType == contentType.none)
            {
                data.type = contentType.audio;
           

                list_sources.Add(data);

            }
            else
            {
                data.type = contentType.multiple;

            }
        }
       
        else if (data.vibrate.autoplay)
        {
            if (dataType == contentType.none)
            {
                data.type = contentType.vibrate;

            }
            else
            {
                data.type = contentType.multiple;

            }

        }
      
        else if (data.input.options !=null)
        {
            if (dataType == contentType.none)
            {
                data.type = contentType.input;

                for (int i = 0; i < data.input.options.Length; i++)
                {
                    SetActionType(data, data.input.options[i].action, out data.input.options[i].type);
                }


            }
            else
            {
                data.type = contentType.multiple;
            }
        }
   
        //set my dialog number
        data.dialogNumber = settingDialogNumber;
        debug_audio = new AudioClip[list_DownloadContent.Count];
    }

    void SetActionType( Content data, Action action, out actiontype type)
    {
        type = actiontype.notSet;

        if (action.gotoCamera != null)
        {
            type = actiontype.gotoCamera;
        }
        else if (action.gotoMap != null)
        {
            type = actiontype.gotoMap;

        }
        else if (action.gotoContent != 0)
        {
            type  = actiontype.gotoContent;
        }

       
    }
    
    void SetTriggerType(Trigger trigger, out _triggertype type)
    {
        type = _triggertype.notSet;

        if(trigger.type.location.latitude != 0)
        {
            type = _triggertype.location;
          
        }
        else if(trigger.type.marker.sourcelink !=null)
        {
            type = _triggertype.marker;

        }
        else if(trigger.type.time.delay !=0)
        {
            type = _triggertype.time;
        }
        
    }


    public  int sourceCount = 0;
    public  bool sourceLoadEnd;

  
    public void DownloadSources( int sourceIndex)
    {
        Source[] sources = null;
        

        switch (sourceIndex)
        {
            case 0:
                sources = nan.data.sources;
                break;

            case 1:
                sources = jo.data.sources;
                break;
        }



        if (sourceCount <sources.Length)
        {
            Debug.Log("sources Download call ->" + sourceIndex + "dataorder" + sourceCount + "sourceCount");
            sourceLoader.DownSource = false;
            sourceLoader.loadEnd = true;
            switch (sources[sourceCount].sourcetype)
            {
                case "video":
                   // StartCoroutine(sourceLoader.DownloadFile(sources[sourceCount].sourcelink, sources[sourceCount], sourceIndex));
                    break;

                case "bundle":

                    bool isExist = false;
                    int existIndex =0;
                    //번들 중복 체크 
                    for (int i = 0; i < List_bundle.Count; i++)
                    {
                        if(List_bundle[i].name == sources[sourceCount].name)
                        {
                            //path 연결
                            isExist = true;
                            existIndex = i;
                        }
                        
                    }

                    if(isExist)
                    {

                        switch (sourceIndex)
                        {
                            case 0:
                                nan.data.sources[sourceCount].loadObject = List_bundle[existIndex].loadObject;
                                nan.data.sources[sourceCount].resultlink = List_bundle[existIndex].resultlink;
                                break;

                            case 1:
                                jo.data.sources[sourceCount].loadObject = List_bundle[existIndex].loadObject;
                                jo.data.sources[sourceCount].resultlink = List_bundle[existIndex].resultlink;
                                break;
                        }
                        //link path only
                        Debug.Log("bundle already exist");
                        sourceLoader.downEnd = true; 
                    }
                    else
                    {
                        Debug.Log("bundle no exist");
                        List_bundle.Add(sources[sourceCount]);
                        //loadbundle
                        StartCoroutine(sourceLoader.LoadBundle(sources[sourceCount]));
                        Debug.Log("downsource in bundleLoad ->"+sourceLoader.DownSource);
                      
                    }
                 
                    break;

            }

          
            //sourceCount++;

        }
        else
        {
            sourceCount = 0;
            Debug.Log("sources Download end");
            sourceLoader.tempSourceIndex++;

         
            if (sourceLoader.tempSourceIndex < 2)
            {
                Debug.Log("tempSourceIndex count up" + sourceLoader.tempSourceIndex);
                sourceLoader.DownSource = true;
                StartCoroutine(sourceLoader.DownSourceFIles(sourceLoader.tempSourceIndex));
               // sourceLoadEnd = true;
            }
            else
            {
                Debug.Log("resource Settings over");
                //activate select manager
                selectmanager.isLoadingEnd = true;
                selectmanager.UpdateLoadingBar();
            
            }
        }
        //이동
        loadableSourceNumber--;
        sourceCount++;
    }

  

    public void SetSelectDataDialogs()
    {
        switch(selectedCharacterName)
        {
            case "nan":
                curDialogs = nan.data.dialogs;
                break;

            case "jo":
                curDialogs = nan.data.dialogs;
                break;
        }

       
        curDialogIndex = 0;

        //after load - d sequencer's enabled
        // d_sequencer.curContent = curContentList[0];
        //  d_sequencer.enabled = true;

    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded: " + scene.name);

        if(scene.name == "S_02_Main")
        {
            d_sequencer = GameObject.FindGameObjectWithTag("dialogSequencer").GetComponent<DialogSequencer>();
            gpsmanager = GameObject.FindGameObjectWithTag("gpsManager").GetComponent<GpsManager>();
            d_sequencer.curContent = curContentList[0];
       
            d_sequencer.selectData = new DataCollection();
           switch(selectedCharacterName)
            {
                case "nan":
                    d_sequencer.selectData = nan;
                    break;

                case "jo":
                    d_sequencer.selectData = jo;
                    break;
           }
          
         
            d_sequencer.SetArTargetToImage();
            d_sequencer.enabled = true;
        }
  
        
    }


    //put download sources to ar object
    public bool SetSourceToPath(string resultpath, object origin)
    {


        var type = origin.GetType();

        if(type == typeof(Content))
        {

            Content _content = (Content)origin;
            Debug.Log("type is content"+ _content.id);


        }
        else if(type == typeof(Source))
        {
            Debug.Log("type is sources");

            Source source = (Source)origin;

            int targetIndex = 0;
            Source[] refSource = null;
            switch (source.dataName)
            {
                case "nan":
                    refSource = nan.data.sources;
                    break;

                case "jo":
                    refSource = jo.data.sources;
                    break;
            }

            for (int i = 0; i < refSource.Length; i++)
            {
                if (refSource[i].name == (source.name))
                {
                    targetIndex = i;
                    break;
                }

            }

            switch (source.sourcetype)
            {
                case "video":

                    refSource[targetIndex].resultlink = resultpath;
                    Debug.Log("videoPath" + targetIndex + "number ->" + resultpath);

                    break;

                case "bundle":
                    refSource[targetIndex].loadObject = source.loadObject;
                    Debug.Log("bundle path" + targetIndex + "number ->" + resultpath);

                    break;
            }

            switch (source.dataName)
            {
                case "nan":
                    nan.data.sources[targetIndex].resultlink = refSource[targetIndex].resultlink;
                    nan.data.sources[targetIndex].loadObject = refSource[targetIndex].loadObject;
                    break;

                case "jo":
                    jo.data.sources[targetIndex].resultlink = refSource[targetIndex].resultlink;
                    jo.data.sources[targetIndex].loadObject = refSource[targetIndex].loadObject;
                    break;
            }
        }
      


        loadableSourceNumber--;
        return true;


    }


    //put download sources to ar object
    public bool SetSourceToPath(string resultpath, Source origin_source)
    {
        int targetIndex = 0;
        int bundleIndex = 0;
        Source[] refSource = null;
        switch (origin_source.dataName)
        {
            case "nan":
                 refSource = nan.data.sources;
                break;

            case "jo":
                  refSource = jo.data.sources;
                break;
        }

       
        for (int i = 0; i < refSource.Length; i++)
        {
            if (refSource[i].name == origin_source.name)
            {
                targetIndex = i;
                break;
            }

        }

        switch (origin_source.sourcetype)
        {
            case "video":
              
                refSource[targetIndex].resultlink = resultpath;
                Debug.Log("videoPath"+ targetIndex +"number ->"+ resultpath);
                switch (origin_source.dataName)
                {
                    case "nan":
                        nan.data.sources[targetIndex].resultlink = resultpath;
                        break;

                    case "jo":
                        jo.data.sources[targetIndex].resultlink = resultpath;
                        break;
                }

                break;

            case "bundle":

                if(List_bundle.Count != 0)
                {
                    for (int i = 0; i < List_bundle.Count; i++)
                    {
                        if (List_bundle[i].name == origin_source.name)
                        {
                            bundleIndex = i;
                            Debug.Log("targetIndex" + targetIndex);
                            Debug.Log("targetIndex" + List_bundle[bundleIndex].name);
                            break;
                        }

                    }
                    
                    switch (origin_source.dataName)
                    {
                        case "nan":
                            nan.data.sources[targetIndex].loadObject = List_bundle[bundleIndex].loadObject;
                            Debug.Log("nan ="+nan.data.sources[targetIndex].loadObject.name+"/" + "List_bundle ->" + List_bundle[bundleIndex].name);
                            break;

                        case "jo":
                            jo.data.sources[targetIndex].loadObject = List_bundle[bundleIndex].loadObject;
                            Debug.Log("jo ="+jo.data.sources[targetIndex].loadObject.name + "/" + "List_bundle ->" + List_bundle[bundleIndex].name);
                            break;
                    }
                }
              
                break;
        }

        SourceLoader.instance.loadSource = true;
        loadableSourceNumber--;
        return true;

    }

  

    int debugAint = 0;
    public void SetSourceToPath(string path, Content targetContent, AudioClip result)
    {

        int targetIndex = 0;
        DataCollection dataOrigin = new DataCollection();
        switch (targetContent.dataName)
        {
            case "nan":
                dataOrigin = nan;
                break;

            case "jo":
                dataOrigin = jo;
                break;
        }

        for (int i = 0; i < dataOrigin.data.dialogs[targetContent.dialogNumber].contents.Length; i++)
        {
            if (dataOrigin.data.dialogs[targetContent.dialogNumber].contents[i].id == targetContent.id)
            {
                targetIndex = i;
                break;
            }

        }

        if(targetContent.type == contentType.audio)
        {
            switch (targetContent.dataName)
            {
                case "nan":
                    nan.data.dialogs[targetContent.dialogNumber].contents[targetIndex].audio.audioClip = result;
                    break;

                case "jo":
                    jo.data.dialogs[targetContent.dialogNumber].contents[targetIndex].audio.audioClip = result;
                    break;
            }
        }

        sourceLoader.Log("지정된 컨텐츠-->" + targetContent.id);

        SourceLoader.instance.downEnd = true;
        SourceLoader.instance.loadSource = true;
      //  SourceLoader.instance.DownSource = true;
      

    }
   
    bool IsJsonVersionUpdated( string jsonName,float curVersion)
    {

        float exVersion = PlayerPrefs.GetFloat(jsonName);
        Debug.Log(jsonName + "ex version is : " + exVersion + "||| cur data version is ="+ curVersion);
        if (exVersion != curVersion)
        {
            PlayerPrefs.SetFloat(jsonName,curVersion);
            return true;
        }
        else
        {
            return false;
        }

    }




    void ChangeDialogIndexEvented()
    {
        Debug.Log("eventer change dialog index");
    }
}
