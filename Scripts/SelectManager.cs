using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Vuforia;


public class SelectManager : MonoBehaviour
{
    public GameObject panel_00_logo;
    public GameObject panel_01_authorization;
    public GameObject panel_02_resource;
    
    public TextMeshProUGUI _fileDescription;
    public int loadedFile;
    public Slider loadingSlider;
    public SourceLoader loader;

    string isInitApp;
    public enum selectPannel
    {
        c_00_logo,
        c_CheckNextState,
        c_01_authorization,
        c_02_resource,
        c_03_selectCharacter,
        c_waitJsonDown
    }

    public selectPannel curPannelState
    {
        get { return _curPannelState;}
        set
        {
            if(value != _curPannelState)
            {
                _curPannelState = value;
                ChangePannel();
            }
        }

    }
    
    public selectPannel _curPannelState;

    
    public enum gpsPermission
    {
        notSet,
        agree,
        disagree
    }
    gpsPermission curGpsPermission;

    public enum cameraPermission
    {
        notSet,
        agree,
        disagree
    }
    cameraPermission curCameraPermission;
    bool gpsPopClicked;
    bool cameraPopClicked;
    bool pushOnce;


    public enum OsMode
    {
        android,
        ios
    }

    public OsMode curDeviceMode;
    private void Awake()
    {
        curPannelState = selectPannel.c_00_logo;

        gpsPopClicked = false;
        cameraPopClicked = false;
        pushOnce = false;
        panel_01_authorization.SetActive(false);
        panel_02_resource.SetActive(false);

        curGpsPermission = gpsPermission.notSet;
        curCameraPermission = cameraPermission.notSet;

      
        pushOnce = false;

        //get os status
#if UNITY_ANDROID

        curDeviceMode = OsMode.android;

#elif UNITY_IOS

           curDeviceMode = OsMode.ios;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

        //check init status
        isInitApp = PlayerPrefs.GetString("isInitApp");
        Debug.Log("curDeviceMode? =====>" + curDeviceMode);
        CheckPermissionState();
        StartCoroutine(DelayOnLogo());

    }

   

    IEnumerator DelayOnLogo()
    {
        yield return new WaitForSeconds(1);

        // before start load, check json exist
        //  loader.CheckJsonExist();

        //download json without check
        StartCoroutine(loader.LoadJsonAndSort());
     
    }
    IEnumerator FadeOut( Transform root)
    {
        root.gameObject.SetActive(false);
        curPannelState = selectPannel.c_01_authorization;
        yield return null;

    }

    void ChangePannel()
    {
        Debug.Log("cur pannel changed");
        switch(curPannelState)
        {
            case selectPannel.c_00_logo:
                panel_00_logo.SetActive(true);
                break;

            case selectPannel.c_01_authorization:
                CheckPermissionState();
                if(!skipPermission)
                {
                    panel_00_logo.SetActive(false);
                    panel_01_authorization.SetActive(true);
                }
                else
                {
                    if (loader.isNeedToDownload)
                    {
                        Debug.Log("++++++++need to download+++++++++");

                        //start from jsonsort
                        //StartCoroutine(loader.LoadJsonAndSort());
                    }
                    else
                    {
                        Debug.Log("+++++++++don't need to download json +++++++");
                        SourceLoader.isPermissionChecked = true;

                    }

                    //check loading or select
                    curPannelState = selectPannel.c_CheckNextState;
                }
              
             
                break;

            case selectPannel.c_CheckNextState:
                CheckNextPannel();
                break;

            case selectPannel.c_02_resource:
                panel_00_logo.SetActive(false);
                panel_01_authorization.SetActive(false);
                panel_02_resource.SetActive(true);
                //start download resource
                break;

            case selectPannel.c_03_selectCharacter:
                //load characterselect pannel
                SceneManager.LoadScene("S_01_SelectScene");
                // panel_02_resource.SetActive(false);

                break;
            case selectPannel.c_waitJsonDown:

                break;

        }
    }


    //private void Update()
    //{
    //    if(Input.touchCount>0)
    //    {
    //        Touch touch = Input.GetTouch(0);
    //        if (touch.phase == TouchPhase.Began)
    //        {
    //            Debug.Log("start Check json");
    //            loader.CheckJsonExist();
    //            //추후에  연결
    //           // StartCoroutine(loader.LoadJsonAndSort());

    //        }

    //    }
    //    if (Input.GetKeyDown(KeyCode.P) )
    //    {
    //        loader.CheckJsonExist();
    //        //추후에  연결
    //       // StartCoroutine(loader.LoadJsonAndSort());
    //    }

    //    if (Input.GetKeyDown(KeyCode.G))
    //    {
    //        gpsPopClicked = true;
    //    }

    //    if (Input.GetKeyDown(KeyCode.C))
    //    {
    //        cameraPopClicked = true;
    //    }

    //    if (Input.GetKeyDown(KeyCode.KeypadEnter))
    //    {
    //        Debug.Log("start loading pannel");
    //        curPannelState = SelectManager.selectPannel.c_CheckNextState;
    //    }

    //}

    void CheckNextPannel()
    {
        Debug.Log("start CheckNextPannel");

        Debug.Log("check next pannel cur dowload need? --->" + loader.isNeedToDownload);
        if (SourceLoader.isPermissionChecked)
        {

            if (DataSorter.instance.list_DownloadObject.Count == 0)
            {
                Debug.Log("download end skip loading");
                //move to select pannel
                curPannelState = selectPannel.c_03_selectCharacter;
            }
            else
            {
                Debug.Log("start download");

                //start loading bar
                InitLoadingBar();
                //get loading 
                curPannelState = selectPannel.c_02_resource;
                Debug.Log("다운로드 숫자" + DataSorter.instance.list_DownloadObject.Count + "<- 다운로드 시작 ");
            }

        }
        else
        {
            if (loader.isNeedToDownload)
            {
                SourceLoader.isPermissionChecked = true;
                loader.isNeedToDownload = false;
                Debug.Log("start download json files");
                StartCoroutine(loader.LoadJsonAndSort());

                curPannelState = selectPannel.c_waitJsonDown;
            }
        }
        

    }

    public bool isLoadingEnd = false;
    //animate resource loading bar
    public void UpdateLoadingBar()
    {
        int curLoadCount = (SourceLoader.instance.index_downObject);
        if (!isLoadingEnd)
        {
            if (loadingCur < loadingMax)
            {
                loadingSlider.value = curLoadCount;
                loadingCur = curLoadCount;
                _fileDescription.text = "Downloading..." + loadingCur + "/" + loadingMax;
            }
            else
            {
                _fileDescription.text = "Downloading..." + loadingMax + "/" + loadingMax;
            }
        }
        else
        {
        
            //100%
            loadingSlider.value = loadingMax;
            //load characterselect pannel
            SceneManager.LoadScene("S_01_SelectScene");
            Debug.Log("loading bar animation is end");
         
        }
    
    }

    bool skipPermission = false;
   void CheckPermissionState()
   {

        Debug.Log(" is app initialized? ->" + isInitApp);

        switch(curDeviceMode)
        {
            case OsMode.android:

                if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    curGpsPermission = gpsPermission.agree;
                    gpsPopClicked = true;
                }

                if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    //start loading json
                    curCameraPermission = cameraPermission.agree;
                    cameraPopClicked = true;
                }


                break;


            case OsMode.ios:
                if(isInitApp=="true")
                {
                    Debug.Log(" isInitApp true initialized? ");

                    curGpsPermission = gpsPermission.agree;
                    gpsPopClicked = true;
                    //start loading json
                    curCameraPermission = cameraPermission.agree;
                    cameraPopClicked = true;

                }
                break;
        }



        if (curGpsPermission == gpsPermission.agree && curCameraPermission == cameraPermission.agree)
        {
            skipPermission = true;
        }
        else
        {
            skipPermission = false;
        }


        Debug.Log(gpsPopClicked + "----gpsclicked?" + cameraPopClicked + "-------cameraClicked?");

    }



    public IEnumerator PushPermissionPopUp()
    {
        if (!pushOnce)
        {
            PopGpsPermission();
        }

        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => Application.isFocused == true);
     
        Debug.Log("application focus ? ->" + Application.isFocused);
        GetGpsPermission();

        yield return new WaitUntil(() => gpsPopClicked);
        pushOnce = false;

        if (!cameraPopClicked)
        {
            PopCameraPermission();
        }

        yield return new WaitForSeconds(0.2f);
        Debug.Log("application focus ? ->" + Application.isFocused);
        yield return new WaitUntil(() => Application.isFocused == true);

        GetCameraPermission();
        yield return new WaitUntil(() => cameraPopClicked);

        if(loader.isNeedToDownload)
        {
            Debug.Log("++++++++need to download+++++++++");
        
            //start from jsonsort
            //StartCoroutine(loader.LoadJsonAndSort());
        }
        else
        {
            Debug.Log("+++++++++don't need to download json +++++++");
            SourceLoader.isPermissionChecked = true;
       
        }


        if (curDeviceMode == OsMode.ios)
        {

            PlayerPrefs.SetString("isInitApp", "true");
        }


        //check loading or select
        curPannelState = selectPannel.c_CheckNextState;

        yield return null;

    }

   
    void  PopGpsPermission()
    {


        switch(curDeviceMode)
        {
            case OsMode.android:

                if (curGpsPermission != gpsPermission.agree)
                {
                    if (!pushOnce)
                    {
                        Debug.Log("ask GPS permission on unity+++++++++++++");
                        Permission.RequestUserPermission(Permission.FineLocation);


                        pushOnce = true;
                    }

                }
                else
                {
                    pushOnce = true;
                    gpsPopClicked = true;
                }

                break;


            case OsMode.ios:
                if (!pushOnce)
                {
                    if (isInitApp != "true")
                    {

                        Debug.Log("ask GPS permission on ios mode+++++++++++++");
                        Input.location.Start();

                    }
                    else
                    {
                      
                        gpsPopClicked = true;
                    }


                    pushOnce = true;
                }



                break;
        }




    }



    void GetGpsPermission()
    {


        switch (curDeviceMode)
        {
            case OsMode.android:

                if (pushOnce)
                {
                    if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                    {
                        curGpsPermission = gpsPermission.agree;

                    }
                    else if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                    {
                        curGpsPermission = gpsPermission.disagree;

                    }
                    if (curGpsPermission == gpsPermission.agree || curGpsPermission == gpsPermission.disagree)
                    {
                        Debug.Log("GPS permission clicked and get++++++++++");
                        gpsPopClicked = true;
                     
                    }
                }

                break;


            case OsMode.ios:

                Debug.Log("GPS permission clicked and get++++++++++");
                gpsPopClicked = true;
                curGpsPermission = gpsPermission.agree;

                break;

        }


    }


    public void PopCameraPermission()
    {

        switch (curDeviceMode)
        {
            case OsMode.android:
                if (curCameraPermission != cameraPermission.agree)
                {
                    if (!pushOnce)
                    {


                        Debug.Log("ask CAMERA permission on unity+++++++++++++++");
                        Permission.RequestUserPermission(Permission.Camera);
                        pushOnce = true;
                    }

                }
                else
                {
                    cameraPopClicked = true;
                    pushOnce = true;
                }
                break;

            case OsMode.ios:
                if (isInitApp!="true")
                {
                    if (!pushOnce)
                    {
                        Debug.Log("ask CAMERA permission on ios+++++++++++++++");

                        //vuforia init
                        VuforiaRuntime.Instance.InitVuforia();
                        pushOnce = true;
                    }

                }
                else
                {
                    cameraPopClicked = true;
                    pushOnce = true;
                }

                break;
        }



    }


    public void GetCameraPermission()
    {


        switch (curDeviceMode)
        {
            case OsMode.android:
                if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    curCameraPermission = cameraPermission.agree;

                }
                else if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    curCameraPermission = cameraPermission.disagree;

                }
                if (curCameraPermission == cameraPermission.agree || curCameraPermission == cameraPermission.disagree)
                {

                    Debug.Log("get CAMERA clicked++++++++++++++++");
                    cameraPopClicked = true;

                }


                break;

            case OsMode.ios:
                Debug.Log("get CAMERA clicked++++++++++++++++");
                cameraPopClicked = true;
                curCameraPermission = cameraPermission.agree;

                break;

        }


    }


    int loadingMin;
    int loadingMax = 40;
    int loadingCur;

   public void InitLoadingBar()
   {
        Debug.Log("list_DownloadObject count =="+ DataSorter.instance.list_DownloadObject.Count);
        loadingMin =0;
        loadingMax =DataSorter.instance.list_DownloadObject.Count;
        loadingSlider.minValue = loadingMin;
        loadingSlider.maxValue = loadingMax;
        loadingSlider.value = 0;
        _fileDescription.text = "Downloading..." + loadingMin + "/" + loadingMax;
        SourceLoader.instance.OnLoadingCountChanged += UpdateLoadingBar;
        Debug.Log("start coroutine downsourceSequencer");
       
        //downloadStart
        SourceLoader.instance.DownSource = true;

        StartCoroutine(SourceLoader.instance.DownSourceSequencer());

    }


    public void ClickPermissionRequest()
    {
        Debug.Log("request button click");
        StartCoroutine(PushPermissionPopUp());
       
    
    }

    void GpsAuthorOnIos()
    {
        
    }
  
}
