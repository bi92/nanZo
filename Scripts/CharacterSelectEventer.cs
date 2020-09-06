using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectEventer : MonoBehaviour
{

    [Header("NAN start pannel")]
    public GameObject pannel_startNan;

    [Header("JO start pannel")]
    public GameObject pannel_startJo;


    public GameObject SelectPannel;

  

    public TextTyper[] typerScript = new TextTyper[2];

    [Header(" 0 - nan/ 1 - jo")]
    public GameObject[] startBtns;
    private void Awake()
    {
        SelectPannel.SetActive(true);
        pannel_startNan.gameObject.SetActive(false);
        pannel_startJo.gameObject.SetActive(false);

        
        for (int i = 0; i < typerScript.Length; i++)
        {
            typerScript[i].descriptionTypingEnd += ActivateStartBtn;
        }
        
    }
    public void ActiveNanStartPannel()
    {
        Debug.Log("click nan");
        pannel_startNan.gameObject.SetActive(true);
        SelectPannel.gameObject.SetActive(false);
    }

    public void ActiveJoStartPannel()
    {
        pannel_startJo.gameObject.SetActive(true);
        SelectPannel.gameObject.SetActive(false);
    }

    public void ClickCharacterNan()
    {
        DataSorter.instance.selectedCharacterName = "nan";
        LoadMainScene();
    }
    public void ClickCharacterJo()
    {
        DataSorter.instance.selectedCharacterName = "jo";
        LoadMainScene();
    }

    void LoadMainScene()
    {
        DataSorter.instance.SetSelectDataDialogs();
        SceneManager.LoadScene("S_02_Main");

    }



    public void Debug_prefReset()
    {
        Debug.Log("cur pref" + PlayerPrefs.GetString("isInitApp"));
        PlayerPrefs.DeleteAll();
        Debug.Log("after delete pref" + PlayerPrefs.GetString("isInitApp"));
    }


    public void DebugResetVersion()
    {
        PlayerPrefs.DeleteKey("nan");
    }

    void ActivateStartBtn()
    {
        Debug.Log("activate start btn");
        for (int i = 0; i < startBtns.Length; i++)
        {
            startBtns[i].SetActive(true);
        }
    }

}
