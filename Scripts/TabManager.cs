using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using TMPro;
using NinevaStudios.GoogleMaps.Internal;

public class TabManager : MonoBehaviour
{
    GameObject tab_dialog;
    GameObject ui_dialogActive;
    GameObject ui_mapActive;

    GameObject tab_map;
    
    [Header("대화창")]
    public GameObject pannel_dialog;

    [Header("대화창 active off")]
    public GameObject[] dialogUiComponents;

    [Header("지도창")]
    public GameObject pannel_map;

    [Header("탭창")]
    public GameObject pannel_tab;

    [Header("AR_camera")]
    public VuforiaBehaviour AR_camera;

    public UnityEngine.UI.Image baseBg;
    [Header("Ar camera 나가기 버튼")]
    public GameObject pannel_cameraExit;
    public enum tabState
    {
        load,
        dialogMode,
        mapMode,
        arMode
        
    }

    bool isMapInit;
    [Header("Dialog text color")]
    public Color txtColor;
    public tabState _curTabState;
    public  tabState curTabState
    {
        get { return _curTabState; }
        set
        {
            if(value!= curTabState)
            {
                _curTabState =value ;
                OnTabStateChanged();
            }
        }
    }

    public GpsManager _gpsmanager;
    // Start is called before the first frame update
    void Awake()
    {
        isMapInit = false;
        tab_dialog = pannel_tab.transform.GetChild(0).gameObject;
        ui_dialogActive = tab_dialog.transform.GetChild(0).gameObject;

        tab_map = pannel_tab.transform.GetChild(1).gameObject;
        ui_mapActive = tab_map.transform.GetChild(0).gameObject;
        pannel_map.SetActive(false);
        //default is dialog mode
        curTabState = tabState.dialogMode;

    }

    void OnTabStateChanged()
    {
        Debug.Log("tab value changed"+ curTabState);
        switch(curTabState)
        {
            case tabState.load:
                break;

            case tabState.dialogMode:
                ChangeToDialogMode();
                break;

            case tabState.mapMode:
                ChangeToMapMode();

                break;

             case tabState.arMode:
                ChangeToArMode();
                break;
        }
    }
      

    void ChangeToDialogMode()
    {
        ui_dialogActive.SetActive(true);
        ui_mapActive.SetActive(false);
        if (_gpsmanager.map!= null)
        {
            _gpsmanager.map.IsVisible = false;
        }
        pannel_cameraExit.SetActive(false);
        pannel_dialog.SetActive(true);
        pannel_map.SetActive(false);
        AR_camera.enabled = false;
        ActivateDialogPannel();

    }


    void ChangeToMapMode()
    {
        if(!isMapInit)
        {
            isMapInit = true;
            //start init on gps manager
            _gpsmanager.InitMap();
          
        }
        ui_dialogActive.SetActive(false);
        ui_mapActive.SetActive(true);

        if (_gpsmanager.map != null)
        {
            _gpsmanager.map.IsVisible = true;
        }

        AR_camera.enabled = false;
        pannel_map.SetActive(true);
        pannel_cameraExit.SetActive(false);

        if (Application.platform == RuntimePlatform.Android)
        {
            GoogleMapsSceneHelper.Instance.OnApplicationPause(false);
        }

    }



    void ChangeToArMode()
    {
        pannel_cameraExit.SetActive(true);

        ClearDialogPannel();

        ui_dialogActive.SetActive(false);
        ui_mapActive.SetActive(false);

        if (_gpsmanager.map != null)
        {
            _gpsmanager.map.IsVisible = false;
        }

        pannel_map.SetActive(false);
        AR_camera.enabled = true;
    }


    public void ClearDialogPannel()
    {
        UnityEngine.UI.Image[] dialogImages = pannel_dialog.GetComponentsInChildren<UnityEngine.UI.Image>();
        TextMeshProUGUI[] dialogTexts = pannel_dialog.GetComponentsInChildren<TextMeshProUGUI>();
        baseBg.enabled = false;
        for (int i = 0; i < dialogImages.Length; i++)
        {
            dialogImages[i].enabled = false;
        }
        for (int i = 0; i < dialogTexts.Length; i++)
        {
            dialogTexts[i].color = Color.clear;
        }

        for (int i = 0; i < dialogUiComponents.Length; i++)
        {
            dialogUiComponents[i].SetActive(false);
        }
        

    }


    void ActivateDialogPannel()
    {
        UnityEngine.UI.Image[] dialogImages = pannel_dialog.GetComponentsInChildren<UnityEngine.UI.Image>();
        TextMeshProUGUI[] dialogTexts = pannel_dialog.GetComponentsInChildren<TextMeshProUGUI>();
        baseBg.enabled = true;

        for (int i = 0; i < dialogImages.Length; i++)
        {
            dialogImages[i].enabled = true;
        }
        for (int i = 0; i < dialogTexts.Length; i++)
        {
            dialogTexts[i].color = dialogTexts[i].GetComponent<TxtColorHolder>().myColor;
        }
        for (int i = 0; i < dialogUiComponents.Length; i++)
        {
            dialogUiComponents[i].SetActive(true);
        }

    }
    // tab button action
    public void OnCameraOutClick()
    {
     
        // ar camera 
        curTabState = tabState.dialogMode;
        DataSorter.instance.d_sequencer.waitContentHold = false;

        pannel_tab.SetActive(true);
        Debug.Log("camera out click");

    }

    // tab button action
    public void OnDialogTabClick()
    {
       
        curTabState = tabState.dialogMode;
        Debug.Log("dialogClick");
        
    }

    public void OnARTabClick()
    {
        pannel_tab.SetActive(false);
        curTabState = tabState.arMode;
        Debug.Log("dialogClick");
        DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].trigger.isEnter = true;

    }


    public void OnMapTabClick()
    {
      
        Debug.Log("mapClick"+ curTabState);
        curTabState = tabState.mapMode;
    }

}
