using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;
using StructCollection;



public class SourceLoader : MonoBehaviour
{


    public Transform ARObject2;
    public VideoPlayer ARObject1;

    public AudioSource test_audio_01;
    public Text Output;
    public RectTransform Content;
    string resultpath;

    [Header("json링크 0번 - 난/ 1번 - 조")]
    public string[] JsonLink;
    DataSorter datasorter;
    public bool DownSource = true;
    public AudioClip resultClip;
    public static SourceLoader instance;
    public List<int> testa;
    public SelectManager selectmanager;

    private string base_url = "http://estrangerlab.com/nanzo/";
    
    private void Awake()
    {
        var obj = FindObjectsOfType<SourceLoader>();
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        instance = this;

        datasorter = this.GetComponent<DataSorter>();
        loadSetEnd = true;
        datasorter.sourceLoadEnd = true;
        DownSource = true;
        jsonSort = true;
        loadSource = true;
        downEnd = true;
        index_downObject = 0;
        OnLoadingCountChanged += loader_OnLoadingCountChanged;
    }
    public bool downEnd;
    public bool loadSetEnd;
    public bool audioDownEnd;
    public int tempSourceIndex = 0;
    int sourceLength;

    bool once = false;
    public static bool isPermissionChecked;

    public bool jsonSort;
    public IEnumerator LoadJsonAndSort()
    {
     
        loadSetEnd = false;
        while(datasorter.dataOrder<2)
        {
          
            yield return new WaitUntil(() => jsonSort);
            if(datasorter.dataOrder<JsonLink.Length)
            {
                Debug.Log("call load number ->" + datasorter.dataOrder);
                StartCoroutine(DownloadFile(JsonLink[datasorter.dataOrder], datasorter.dataOrder));
            }
        
        }
   
    }

    public delegate void LoadingCountChange();
    public event LoadingCountChange OnLoadingCountChanged;

    public int _index_downObject;
    public int index_downObject
    {
        get { return _index_downObject; }
        set
        {
            if(value!= _index_downObject)
            {
                _index_downObject = value;
       
                if (OnLoadingCountChanged != null)
                {
                    OnLoadingCountChanged();
                }

            }
        }
    }

    private void loader_OnLoadingCountChanged()
    {

    }

    bool loadthingsEnd = false;
    public IEnumerator DownSourceSequencer()
    {
       
        Debug.Log("loadending? ->" + loadthingsEnd);
        yield return new WaitUntil(() => loadthingsEnd);
        index_downObject = 0;

        Debug.Log("dowinloading? on downsequencer ->" + loadthingsEnd);
        while (index_downObject< DataSorter.instance.list_DownloadObject.Count)
        {

            yield return new WaitUntil(() => DownSource == true);
            if(loadSource)
            {
                Debug.Log("call download  - >  " + index_downObject);
                CallFileToDownload();
            }
          
        }
        Debug.Log(" on downsequencer end index_downObject/->" + index_downObject+"/" + DataSorter.instance.list_DownloadObject.Count);
        //  yield return null;
        selectmanager.curPannelState = SelectManager.selectPannel.c_03_selectCharacter;
    }

    public int index_loadObect = 0;
    public bool loadSource = false;
    public IEnumerator LoadSourceSequencer()
    {
        while(index_loadObect<DataSorter.instance.list_LoadObject.Count)
        {
            yield return new WaitUntil(() => loadSource == true);
            Debug.Log("call load  - >  " + index_loadObect);
            Debug.Log("list_LoadObject count - >  " + DataSorter.instance.list_LoadObject.Count);
            CallFileToLoad();
            //loadthingsEnd = false;
        }
        DownSource = true;
        loadthingsEnd = true;

        Debug.Log("trigger to check next pannel");
       
        if(!isNeedToDownload)
        {
            if (isPermissionChecked)
            {
                Debug.Log("check pannel state 0n loadseqeuncer");
                Debug.Log("curstate" + selectmanager.curPannelState);
                //start to download
                selectmanager.curPannelState = SelectManager.selectPannel.c_CheckNextState;
            }
            else
            {
                Debug.Log(" move to permission check state");
                selectmanager.curPannelState = SelectManager.selectPannel.c_01_authorization;
            }
            
        }
        else
        {
            Debug.Log("isNeedToDownload first");
            //start to download
            //selectmanager.curPannelState = SelectManager.selectPannel.c_CheckNextState;
        }
      

    }

    void CallFileToLoad()
    {
        Debug.Log("로드 오브젝트 인덱스값" + index_loadObect);
        var targetObject = DataSorter.instance.list_LoadObject[index_loadObect];
        loadSource = false;
      
        if (targetObject.GetType() == typeof(Content))
        {
            //load Audio
            Content _content = (Content)targetObject;
            Debug.Log("start load" + index_loadObect + "=번째");
            StartCoroutine(LoadAudios(_content.audio.sourcelink, _content));

        }
        else if (targetObject.GetType() == typeof(Source))
        {
            Source _source = (Source)targetObject;
            switch (_source.sourcetype)
            {
                case "video":
                    Debug.Log("start load" + "/ video" + index_loadObect);
                    StartCoroutine(DownloadFile(_source.sourcelink, _source));
                    break;

                case "bundle":
                    Debug.Log("bundle alreay exist");
                    bool isExist = false;
                    int existIndex = 0;
                    //번들 중복 체크 
                    for (int i = 0; i < DataSorter.instance.List_bundle.Count; i++)
                    {
                        if (DataSorter.instance.List_bundle[i].name == _source.name)
                        {
                            //path 연결
                            isExist = true;
                            existIndex = i;
                        }

                    }
                    if(isExist)
                    {
                        Debug.Log("bundle has exist just link");
                        datasorter.SetSourceToPath(DataSorter.instance.List_bundle[existIndex].sourcelink, _source);
                        index_loadObect++;
                        DownSource = true;
                    }
                    else
                    {
                        StartCoroutine(LoadBundle(_source));
                    }
                   
                    break;
            }
        }
        index_loadObect++;
    }

    void CallFileToDownload()
    {
        var targetObject = DataSorter.instance.list_DownloadObject[index_downObject];
        DownSource = false;
        loadSource = false;
        if (targetObject.GetType() == typeof(Content))
        {
            //downloadAudio
            Content _content = (Content)targetObject;
            Debug.Log("start download" + index_downObject + "/ "+ _content.audio.sourcelink );
            StartCoroutine(DownloadFile(_content.audio.sourcelink,_content));

        }

        else if(targetObject.GetType() == typeof(Source))
        {
            Source _source = (Source)targetObject;
            switch(_source.sourcetype)
            {
                case "video":
                    Debug.Log("start download" + "/ video" + index_downObject);
                    StartCoroutine(DownloadFile(_source.sourcelink, _source));
                    index_downObject++;
                    break;
                 
                case "bundle":
                    Debug.Log("start download" + "bundle" + index_downObject);

                    bool isExist = false;
                    int existIndex = 0;
                    //번들 중복 체크 
                    for (int i = 0; i < DataSorter.instance.List_bundle.Count; i++)
                    {
                        if (DataSorter.instance.List_bundle[i].name == _source.name)
                        {
                            //path 연결
                            isExist = true;
                            existIndex = i;
                        }

                    }
                    if(isExist)
                    {
                        Debug.Log("bundle has exist just link");
                        datasorter.SetSourceToPath(DataSorter.instance.List_bundle[existIndex].sourcelink, _source);
                        index_downObject++;
                        DownSource = true;
                    }
                    else
                    {
                        Debug.Log("bundle load");
                        StartCoroutine(DownloadFile(_source.sourcelink, _source));
                        //StartCoroutine(LoadBundle(_source));
                    }
                    index_downObject++;

                    break;
            }
          
        }
      
    }


    public IEnumerator StartLoadFiles()
    {
        //yield return new WaitUntil(() => loadSetEnd);
        //loadSetEnd = false;
        //Debug.Log("call load number ->" + datasorter.dataOrder);
        //StartCoroutine(LoadFilesSourceByJson(JsonLink[datasorter.dataOrder], datasorter.dataOrder));

        yield return new WaitUntil(() => audioDownEnd);
        datasorter.sourceLoadEnd = false;
      
        yield  break;
    }


  public  IEnumerator DownSourceFIles(int sourceListIndex)
    {
        Debug.Log(" download call start" + sourceListIndex );
        datasorter.sourceLoadEnd = false;
        switch (sourceListIndex)
        {
            case 0:
                sourceLength = datasorter.nan.data.sources.Length;
                break;

            case 1:
                sourceLength = datasorter.jo.data.sources.Length;
              
                break;
        }

        while (datasorter.sourceCount <= sourceLength)
        {
            Debug.Log(" download call"+ sourceListIndex + "<-sourceorder"+datasorter.sourceCount);
     
            yield return new WaitUntil(() => DownSource);
            DownSource = false;

           datasorter.DownloadSources(tempSourceIndex);
        }
        yield return null;
    }

    //ex version
    public  IEnumerator DownloadFile( string sourcelink,Content content)
    {
     
        string source = base_url + sourcelink;
        string[] files = source.Split('/');
        string file = files[files.Length - 1];
        Debug.Log("downloading...."+ index_downObject);
        string targetPath = Application.persistentDataPath + "/" + file;

        if (!File.Exists(targetPath))
        {
            WWW www = new WWW(source);
            yield return www;

            byte[] bytes = www.bytes;
            File.WriteAllBytes(targetPath, bytes);
            downEnd = true;

           
            DownSource = true;
            datasorter.loadableSourceNumber--;
            Debug.Log("다운로드 완료 ---->"+ index_downObject);
            StartCoroutine(LoadAudios(targetPath, content));
            index_downObject++;
        }
        else
        {
            Debug.Log("패스연결 완료 ---->" + index_downObject);
            Log("패스연결 완료 ---->" + loadCount);

            DownSource = true;
            //save resultpath
            downEnd = true;
            resultpath = targetPath;
            StartCoroutine(LoadAudios(targetPath, content));
            index_downObject++;

        }
      

    }

   //download video
    public IEnumerator DownloadFile( string sourcelink, Source origin_source)
    {
        string source = base_url + sourcelink;
        string[] files = source.Split('/');
        string file = files[files.Length - 1];
        string url;
        string targetPath = Application.persistentDataPath + "/" + file;
       
        if (!File.Exists(targetPath))
        {
          
         
            if(origin_source.sourcetype == "bundle")
            {
#if UNITY_ANDROID
                url = source + ".android";
#elif UNITY_IOS
                url = source + ".ios";
#endif
                Debug.Log(url);
            
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.Send();

                File.WriteAllBytes(targetPath+".unity3d", request.downloadHandler.data);
                Debug.Log("source download 완료-> " + targetPath);
                origin_source.resultlink = targetPath+".unity3d";
                StartCoroutine(LoadBundle(origin_source));
            }
            else
            {
                WWW www = new WWW(source);
                yield return www;
                byte[] bytes = www.bytes;
                File.WriteAllBytes(targetPath, bytes);
                Debug.Log("source download 완료-> " + targetPath);
                datasorter.SetSourceToPath(targetPath, origin_source);
            }
         

        }
        else
        {
            if (origin_source.sourcetype == "bundle")
            {
                origin_source.resultlink = targetPath;
                StartCoroutine(LoadBundle(origin_source));
            }
            else
            {
                Debug.Log("source existed 패스 연결-> " + targetPath);
                datasorter.SetSourceToPath(targetPath, origin_source);
            }

        }
      
        DownSource = true;
    }

   public bool isNeedToDownload;

    public void CheckJsonExist()
    {
        Debug.Log("JJsonLinkLength" + JsonLink.Length);
        for (int i = 0; i < JsonLink.Length; i++)
        {
            Debug.Log("JsonLink: start check" + JsonLink[i]);
            string source = base_url + JsonLink[i];
            string[] files = source.Split('/');
            string file = files[files.Length - 1];

            string targetPath = Application.persistentDataPath + "/" + file;
            Debug.Log("persistentDataPath " + targetPath);

            if (!File.Exists(targetPath))
            {
                Debug.Log("target doesnt have exist ");
                isNeedToDownload = true;
                
            }
            else
            {
                Debug.Log("targetexist ");
                isNeedToDownload = false;
                
            }

            Debug.Log("json file number :" + i + " is need to download?==>" + isNeedToDownload);

        }

        if(!isNeedToDownload)
        {
            Debug.Log("StartCoroutine :LoadJsonAndSort");
            StartCoroutine(LoadJsonAndSort());
        }
        else
        {
            selectmanager.curPannelState = SelectManager.selectPannel.c_01_authorization;
        }
      
    }

    //load json
    public IEnumerator DownloadFile(string sourcelink, int order)
    {
        jsonSort = false;
        string source = base_url + sourcelink;
        string[] files = source.Split('/');
        string file = files[files.Length - 1];

        string targetPath = Application.persistentDataPath + "/" + file;

        if (!File.Exists(targetPath))
        {
            WWW www = new WWW(source);
            yield return www;

            byte[] bytes = www.bytes;
            File.WriteAllBytes(targetPath, bytes);

            Log("다운로드 json ---->" + targetPath);
            Debug.Log("다운로드 json ---->" + targetPath);
            datasorter.OnStartAfterJson(targetPath,order);
        }
        else
        {
            DeleteFile(sourcelink);
            WWW www = new WWW(source);
            yield return www;

            Debug.Log("재 다운로드 json ---->" + targetPath);
            byte[] bytes = www.bytes;
            File.WriteAllBytes(targetPath, bytes);
            datasorter.OnStartAfterJson(targetPath, order);

        }


    }


    void DeleteFile(string sourcelink)
    {
        string source = base_url + sourcelink;
        string[] files = source.Split('/');
        string file = files[files.Length - 1];

        string targetPath = Application.persistentDataPath + "/" + file;
        Debug.Log(" file exists, deleting..."+ file);

        File.Delete(targetPath);
     
    }


    public void CheckBundleExist( Source source)
    {


        string url = source.sourcelink;
        string[] files = url.Split('/');
        string file = files[files.Length - 1];
        string targetPath = Application.persistentDataPath + "/" + file;
#if UNITY_ANDROID
        targetPath += ".android";
#elif UNITY_IOS
        url += ".ios";
#endif
        if((File.Exists(targetPath)))
        {
            Debug.Log("bundle file exist");
        }
        else
        {
             Debug.Log("bundle file exist");
        }
    }

    public IEnumerator LoadBundle( Source source)
    {

        string[] files = source.sourcelink.Split('/');
        string file = files[files.Length - 1];
        string targetPath = Application.persistentDataPath + "/" + file;

        Debug.Log("Request asset bundle");
        string filepath= targetPath+".unity3d";

        Debug.Log("Get content asset bundle");

        var  assetFile = AssetBundle.LoadFromFile(Path.Combine(filepath));
        var loadasset = assetFile.LoadAsset<GameObject>("Assets/NanZo/Prefabs/Stall.prefab");
        yield return loadasset;

        Debug.Log("Instantiate content from prefab" + source.name);
        source.loadObject = Instantiate(loadasset, Vector3.zero, Quaternion.identity);
    
        DontDestroyOnLoad(source.loadObject);
        source.loadObject.SetActive(false);

        Debug.Log("bundle add in list");
        datasorter.SetSourceToPath(filepath, source);

        //add list
        DataSorter.instance.List_bundle.Add(source);

        DownSource = true;
        index_downObject++;
        downEnd = true;
 

        loadSource = true;
    }


    public int loadCount = 0;
    public bool loadEnd;
  
    public IEnumerator LoadAudios(string path, Content content)
    {
       
        string[] files = path.Split('/');
        string file = files[files.Length - 1];
        string targetPath = Application.persistentDataPath + "/" + file;

        yield return new WaitUntil(() => downEnd);
        downEnd = false;
     
        if (File.Exists(targetPath))
        {

            WWW audioLoader = new WWW("file://" + targetPath);
            yield return audioLoader;
            resultClip = audioLoader.GetAudioClip(false, true) as AudioClip;
            Debug.Log( "오디오 로드 완료-->" + targetPath+ content.id);
         
            datasorter.loadableSourceNumber--;
            datasorter.SetSourceToPath(path, content, resultClip);

          
            //  DownSource = true;
        }
        else
        {
         
            Debug.Log("Unable to locate audio.");
        }


      
    }

    //audio -> video -> bundle
   public  IEnumerator LoadSources()
    {

        yield return new WaitUntil(() => loadEnd == true);
       // Log(loadCount + "< -- download call");
        Debug.Log(loadCount + "< -- download call");
        StartCoroutine(DownloadFile(DataSorter.instance.list_DownloadContent[loadCount].audio.sourcelink, DataSorter.instance.list_DownloadContent[loadCount]));
     
   }




    public void Log(string msg)
    {
        Output.text = DateTime.Now + " " + msg + "\n" + Output.text;
        var line = Output.text.Split('\n');
        if (line.Length > 100) Output.text = DateTime.Now + " " + msg + "\n";
        Content.sizeDelta = new Vector2(0, (line.Length * 16) + 20);
    }



}
