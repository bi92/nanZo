using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructCollection;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using Vuforia;

public class DialogSequencer : MonoBehaviour
{

    public static BubleGenerator bubbleGenerator;

    [Header("dataCollector의 string 값과 연결")]
    public string messageString;

    public DataCollection selectData;
    private Content _curcontent;
    //현재 생성되는 content
    public Content curContent
    {
        get { return _curcontent; }
        set
        {

            if (_curcontent != value)
            {
                _curcontent = value;
                setContentDelay();
                DataSorter.instance.setOriginValue = true;
            }
            else
            {
                Debug.Log("_curcontent type is null");
            }

        }
    }

    [Header("현재 컨텐츠 배열 값")]
    //현재 재생 컨텐츠배열 인덱스값
    public int curOrderIndex;
    //현재 재생되어야 하는 id값
    int curOrderId;

    public float curTime;

    public bool isPlayDialog;
    public float delayTime;

    //기다리고 있는 버튼 id값
    int waitButtonId;
    public enum DialogState
    {
        playDialog,
        waitButton,
        waitInput,
        waitTrigger,
        endDialog
    }
    public DialogState curDialogState;

    private int _clickedButtonId;

    [Header("인풋 입력 창")]
    public GameObject inputPannel;
    public int clickedButtonId
    {
        get { return _clickedButtonId; }
        set
        {
            if (_clickedButtonId != value)
            {
                _clickedButtonId = value;

                CheckWaitButtonIsRight();
            }
        }
    }

    public enum InputResultState
    {
        match,
        nonMatch,
        empty
    }

    public int nonMatchedInputActionIndex;
    public int MatchedInputActionIndex;

    public InputResultState curInputResult;
    Buble ui_inputField;

    public static TabManager tabmanager;
    public Buble clicked_bubble;

    [Header("audioPlayer")]
    public Transform audioPlayer;
    AudioSource[] a_sources;
    public SourceLoader loader;
    public TabManager tabManager;

    [Header("Dialog title text component")]
    public TextMeshProUGUI txt_dialogTitle;

    public string[] ARContentOnId ;
    public ArEventer[] list_ArContent;
    // Start is called before the first frame update

    public bool waitContentHold = false;

    public static int curContentIndex;

    public static bool contentTypingEnd;
    private void OnEnable()
    {
        bubbleGenerator = this.GetComponent<BubleGenerator>();
        curInputResult = InputResultState.empty;
        curDialogState = DialogState.waitTrigger;
        ui_inputField = inputPannel.GetComponentInChildren<Buble>();
        inputPannel.SetActive(false);
        tabmanager = tabManager;
        a_sources = audioPlayer.GetComponentsInChildren<AudioSource>();
        ARContentOnId = new string[2];

        for (int i = 0; i < ARContentOnId.Length; i++)
        {
            ARContentOnId[i] = "null";
        }

        VuforiaRuntime.Instance.InitVuforia();
        //init vuforia

        //get bundle
        for (int i = 0; i < DataSorter.instance.List_bundle.Count; i++)
        {

            for (int j = 0; j < list_ArContent.Length; j++)
            {
                if (list_ArContent[j].myArId == DataSorter.instance.List_bundle[i].name)
                {
                    GameObject arBundles = Instantiate(DataSorter.instance.List_bundle[i].loadObject);
                    arBundles.transform.parent = list_ArContent[i].transform.GetChild(0);
                    arBundles.transform.localPosition = Vector3.zero;
                    arBundles.transform.localRotation = Quaternion.identity;
                    arBundles.transform.localScale = Vector3.one;
                    Debug.Log("budle" + list_ArContent[i].myArId + " - is loaded on scene");
                }

            }

        }
        contentTypingEnd = true;

    }

    public Animator yellowTransition;
    public bool isYellowTransitionEnd;
    // Update is called once per frame
    void Update()
    {

        if(DataSorter.instance.setOriginValue)
        {
            switch (curDialogState)
            {
                case DialogState.waitTrigger:
                    WaitingTrigger();
                    break;

                case DialogState.playDialog:
                    PlayingDialog();
                    break;

                case DialogState.waitButton:
                    WaitingButton();
                    break;

                case DialogState.waitInput:
                    WaitingInput();
                    break;

                case DialogState.endDialog:
                    //stop sequencer
                    EndDialog();
                    break;
            }

        }
    
    }

    public static bool isEnterTriggerArea = false;
    private void WaitingTrigger()
    {

        //추후에 trigger로 on off
        if (Input.GetKeyDown(KeyCode.Space))
        {

            isEnterTriggerArea = true;

        }
       
       
        CheckTriggerArea(out DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].trigger);

    }

    bool lastContentPlay = false;
    bool playEnding = false;
    private void EndDialog()
    {

        int dialogLength = DataSorter.instance.curDialogs.Length - 1;
        int contentLength = DataSorter.instance.curDialogs[dialogLength].contents.Length - 1;

       if(waitContentHold == false && !lastContentPlay)
       {

            for (int i = 0; i < a_sources.Length; i++)
            {
               if( a_sources[i].isPlaying)
               {
                    return;
               }
               else 
               {
                    if (i == a_sources.Length-1)
                    {
                        lastContentPlay = true;
                        break;
                    }
               }

            }

       }
        if (!playEnding && lastContentPlay)
        {

            playEnding = true;

            Debug.Log("last audio play end");

            tabManager.curTabState = TabManager.tabState.dialogMode;
            yellowTransition.SetTrigger("yellowfadeout");
        }

        if(isYellowTransitionEnd)
        {
            //move to next scene
            SceneManager.LoadScene("S_03_Ending");
        }

    }

    private void PlayingDialog()
    {

        if (!waitContentHold)
        {
            //type effect is end
            if(contentTypingEnd)
            {
                //run dialog sequecer
                curTime += Time.deltaTime;
                if (curTime >= delayTime)
                {
                    Debug.Log("generate content");
                    contentTypingEnd = false;
                    CallContentByType();
                    curTime = 0;
                
                }

            }
            

        }

    }

    public bool buttonClicked;
    private void WaitingButton()
    {
        Content nextContent = null;

        if (!waitContentHold)
        {

            if (curOrderIndex < DataSorter.instance.curContentList.Length - 1)
            {

                //   Debug.Log(" isEnterTriggerArea is on and play next dialog");
                curOrderIndex++;
                nextContent = DataSorter.instance.curContentList[curOrderIndex];
                curContent = nextContent;
                curDialogState = DialogState.playDialog;

            }
            else
            {
                Debug.Log("dialog is end .sequencer will wait next trigger ");
                DataSorter.instance.curDialogIndex++;
                curDialogState = DialogState.waitTrigger;
                //turn off trigger enter bool state
                //   isEnterTriggerArea = false;

                LoadNextContentList(DataSorter.instance.curDialogIndex);
            }

        }

        curTime = 0;

    }

    //사용자가 집어넣은 인풋값
    public static string choosedInput
    {
        get { return _choosedInput; }
        set
        {
            if (value != _choosedInput)
            {
                _choosedInput = value;
                //text message about _choosedinput
                bubbleGenerator.InstantiateMessage(contentType.message, _choosedInput);

            }
        }
    }


    public static string _choosedInput;
    int choosedOptionIndex;

    private void WaitingInput()
    {


    }


    //트리거 안에 있는지 체크  - > 체크할때 inenter밖으로 빼기 
    void CheckTriggerArea(out Trigger origin)
    {
        //trigger가 enterTrigger 인지 체크 
        Trigger curtrigger = DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].trigger;
      
        if (!curtrigger.isEnter)
        {
            switch (curtrigger.u_type)
            {
                case _triggertype.notSet:
                    Debug.Log("trigger type is null--->" + DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex]);

                    break;

                case _triggertype.time:
                    delayTime = curtrigger.type.time.delay/1000;
                    txt_dialogTitle.SetText(DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].title);
                    curtrigger.isEnter = true;
                    Debug.Log("start dialog" + DataSorter.instance.curDialogIndex + "번째 다이얼로그");
                    curDialogState = DialogState.playDialog;
                    //wait triggerdelaytime
                    break;

                case _triggertype.location:

                    if (isEnterTriggerArea)
                    {
                        txt_dialogTitle.SetText(DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].title);
                        curtrigger.isEnter = true;
                        Debug.Log("start dialog" + DataSorter.instance.curDialogIndex + "번째 다이얼로그");
                        Debug.Log(isEnterTriggerArea + " isEnterTriggerArea is on and play next dialog");
                        curDialogState = DialogState.playDialog;

                        tabmanager.curTabState = TabManager.tabState.dialogMode;
                    }

                    //check gps bool values
                    break;

                case _triggertype.marker:
                    curtrigger.isEnter = true;

                    //for (int i = 0; i < eventerAr.Count; i++)
                    //{
                    //    if(eventerAr[i].myArId == curtrigger.type.marker.name)
                    //    {
                    //        eventerAr[i].transform.root.gameObject.SetActive(true);
                    //    }
                    //    else
                    //    {
                    //        Debug.Log("turn off ar" + eventerAr[i].transform.root.name);
                    //        eventerAr[i].transform.root.gameObject.SetActive(false);
                    //    }
                    //}
                    txt_dialogTitle.SetText(DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].title);
                   
                    if (isEnterTriggerArea)
                    {
                        Debug.Log("start dialog" + DataSorter.instance.curDialogIndex + "번째 다이얼로그 marker");
                        curDialogState = DialogState.playDialog;
                        isEnterTriggerArea = false;
                    }

                    break;


            }

        }
        else if(curtrigger.isEnter)
        {

            curDialogState = DialogState.playDialog;
        }
     
        origin = curtrigger;

    }


    int findedIndexByid = 0;
    Content FindContentById(int choosedId)
    {
        Content matchedContent = null;
        findedIndexByid = 0;

        for (int i = 0; i < DataSorter.instance.curDialogs.Length; i++)
        {
        
            for (int j = 0; j < DataSorter.instance.curDialogs[i].contents.Length; j++)
            {
                if (DataSorter.instance.curDialogs[i].contents[j].id == choosedId)
                {
                  
                    matchedContent = DataSorter.instance.curDialogs[i].contents[j];
                    
                    Debug.Log("matched content id ---- > " + matchedContent.id);
                    findedIndexByid = j;
                    matchedContent.dialogNumber = i;

                    return matchedContent;
                 
                }
                
            }
           
        }

        if(matchedContent==null)
        {
        
            Debug.Log("there's no matched content id in this dialog"+ choosedId);
         
        }
       
        return matchedContent;

    }

    void ActByActionType(actiontype type, StructCollection.Action _actionData)
    {
        Content nextContent = null;
       
        switch (type)
        {
            case actiontype.gotoCamera:
                Debug.Log("go to camera action!" + _actionData.gotoCamera);
                tabmanager.curTabState = TabManager.tabState.arMode;
                break;

            case actiontype.gotoMap:
                Debug.Log("gotoMap spot name : " + _actionData.gotoMap);
                Debug.Log("tabmanager : " + tabmanager.gameObject.name);
                tabmanager.curTabState = TabManager.tabState.mapMode;
                break;

            case actiontype.gotoContent:
                Debug.Log("go to content id code ---->" + _actionData.gotoContent);
                int choosedNextContentId = _actionData.gotoContent;
                nextContent = FindContentById(choosedNextContentId);
                curContent = nextContent;
                curOrderIndex = findedIndexByid;
              
                //sssssssss
                DataSorter.instance.curDialogIndex = nextContent.dialogNumber;
                curContentIndex++;
                Debug.Log("새로운 curContent 내용" + curContent.type + curContent.id);
                break;
            

        }
       
        if(curDialogState != DialogState.endDialog)
        {
            curDialogState = DialogState.waitTrigger;

        }

    }



    #region generateContent
    
    void GenerateContent(Content origin,_message message)
    {
        Debug.Log("generate message"+ origin.id);
        //instantiate + messageString <- data string (text only)
        bubbleGenerator.InstantiateMessage(origin, message.txt_const.ko);

    }

    void GenerateContent(Content origin,StructCollection.Button button)
    {
        Debug.Log("generate button"+ origin.id);
        waitButtonId = origin.id;

        bubbleGenerator.InstantiateMessage(origin, button.option.ko);
    }

    void GenerateContent(Audio audio)
    {
       
        Debug.Log("play audio"+curContent.audio.sourcelink);

        if (curContent.audio.audioClip != null)
        {
            Debug.Log("clip->" + audio.sourcelink);
        }
        else
        {
            Debug.Log("audio clip is null -> "+ audio.sourcelink);
        }

        for (int i = 0; i < a_sources.Length; i++)
        {
            if(a_sources[i].isPlaying == false)
            {
                a_sources[i].Stop();
                a_sources[i].clip = audio.audioClip;
                Debug.Log("a_sources->" + a_sources[i]+i);
                a_sources[i].Play();
                break;
            }
            //if every audiosource is playing -> stop first audiosource and play
            else if(i==a_sources.Length-1)
            {
                a_sources[0].Stop();
                a_sources[0].clip = audio.audioClip;
                a_sources[0].Play();
                break;
            }
        }
        
    }


    void GenerateContent(_input input)
    {
        inputPannel.SetActive(true);
        ui_inputField.ResetInputField();
        Debug.Log("generate input");

        //move up scroll space
        bubbleGenerator.MoveUpScrollSpace();

    }

    void GenerateContent(Vibrate vibrate)
    {
        //vibrate action
        Handheld.Vibrate();
        Debug.Log("generate vibrate");

    }


    #endregion



    void CallContentByType()
    {
        contentType dataType = curContent.type;
        waitContentHold = curContent.hold;
        Debug.Log("curContent" + curContent.type);
      
        switch (dataType)
        {

            case contentType.message:
                GenerateContent(curContent, curContent.message);
                curOrderIndex++;
                curContentIndex++;
                break;

            case contentType.button:
               
                GenerateContent(curContent,curContent.button);
                contentTypingEnd = true;
                curContentIndex++;
                curOrderIndex++;
                //curDialogState = DialogState.waitButton;
                //return;
                break;

            case contentType.audio:
                GenerateContent(curContent.audio);
                contentTypingEnd = true;
                curOrderIndex++;
                break;

            case contentType.input:
                GenerateContent(curContent.input);
                curDialogState = DialogState.waitInput;
                return;
               // break;

            case contentType.vibrate:
                contentTypingEnd = true;
                GenerateContent(curContent.vibrate);
                curOrderIndex++;
                break;
        }

        waitContentHold = curContent.hold;
        Debug.Log("waitContentHold ->" + waitContentHold);
        CheckNextContent();

    }


    void CheckNextContent()
    {
     

        //wait next marker when 1st content is set target
        if (curOrderIndex >= DataSorter.instance.curContentList.Length)
        {

            Debug.Log("dialog is end .sequencer will wait next trigger -> curDialogIndex : "+ DataSorter.instance.curDialogIndex);
            Trigger curtrigger = DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].trigger;
            //if (curtrigger.u_type == _triggertype.marker)
            //{
            //    for (int i = 0; i < ARContentOnId.Length; i++)
            //    {
            //        if (ARContentOnId[i] == curtrigger.type.marker.name)
            //        {
            //            //move next;
            //            DataSorter.instance.curDialogIndex++;
            //            break;
            //        }

            //    }

            //}
            //else
            //{
              
            //}

            DataSorter.instance.curDialogIndex++;
            Debug.Log("curDialogIndex++ : " + DataSorter.instance.curDialogIndex);
            if (DataSorter.instance.curDialogIndex >= DataSorter.instance.curDialogs.Length)
            {
                Debug.Log("dialog mode call change to endDialog");
                DataSorter.instance.curDialogIndex -= 1;
                curDialogState = DialogState.endDialog;
                return;

            }
            else
            {
                Debug.Log("load new dialog list"+ DataSorter.instance.curDialogIndex);
                LoadNextContentList(DataSorter.instance.curDialogIndex);
           
            }

        }
        else
        {
            //다음 생성 아이디
            curContent = DataSorter.instance.curContentList[curOrderIndex];
            Debug.Log("curContent is set" + curContent.id);
        }

    }

    void setContentDelay()
    {
        contentType dataType = curContent.type;
    
        switch (dataType)
        {

            case contentType.message:
                 delayTime = curContent.message.delay;
                break;

            case contentType.button:
                delayTime = curContent.button.delay;
                break;

            case contentType.audio:
                delayTime = curContent.audio.delay;
                break;

            case contentType.input:
                delayTime = curContent.input.delay;
                break;

            case contentType.vibrate:
                delayTime = curContent.vibrate.delay;
                break;
        }

        delayTime = delayTime / 1000;
      //  Debug.Log("delaytime"+delayTime);
        if(delayTime<0)
        {
            delayTime = 0;
        }


    }

    public void LoadNextContentList( int dialogindex)
    {
        if(dialogindex >= DataSorter.instance.curDialogs.Length)
        {
            Debug.Log("sequence is end");
            curDialogState = DialogState.endDialog;
        }
        else
        {
            curContentIndex = 0;
            Debug.Log("curContentIndex is zero->"+ curContentIndex);
            //content index
            curOrderIndex = 0;
            curInputResult = InputResultState.empty;
            curDialogState = DialogState.waitTrigger;
            curTime = 0;

            curContent = DataSorter.instance.curContentList[curOrderIndex];
           
            inputPannel.SetActive(false);

        }

    }
    public Content clickedBtn;
    bool CheckWaitButtonIsRight()
    {
        if (clickedButtonId == waitButtonId)
        {

            Content clickButton = FindContentById(clicked_bubble.myId);
            clickedBtn = clickButton;
            Debug.Log("clickButtonType " + clickButton.type);
            Debug.Log("clickButton " + clickButton.type + clickButton.button.action + clickButton.button.type);
            //button root 면 해당 
            ActByActionType(clickButton.button.type, clickButton.button.action);

            Trigger curTrigger = DataSorter.instance.curDialogs[DataSorter.instance.curDialogIndex].trigger;

            if (curTrigger.u_type == _triggertype.time)
            {
                waitContentHold = false;
            }

        

            return true;
        }

        else
        {
            Debug.Log("button is not match" + "origin is " + waitButtonId);
            return false;
        }

    }

    //check if input value has edit, return values that true or false or nonmatched
    public void ActByInputValue()
    {
       
        for (int i = 0; i < curContent.input.options.Length; i++)
        {
         
            if (curContent.input.options[i].keyword == choosedInput)
            {
                choosedOptionIndex = i;
                Debug.Log("matched this option" + curContent.input.options[i].keyword);
                curInputResult = InputResultState.match;
                break;
                
            }
            else
            {
                curInputResult = InputResultState.nonMatch;
                if (curContent.input.options[i].keyword == "nonMatched")
                {
                    nonMatchedInputActionIndex = i;
                    Debug.Log("nonmatch index" + curContent.input.options[i].action.gotoContent);
                    break;

                }
            }
        }

        //act by input value
        switch(curInputResult)
        {

            case InputResultState.match:

                Debug.Log("match this --> " + curContent.input.options[choosedOptionIndex].keyword);
                //go actions
                ActByActionType(curContent.input.options[choosedOptionIndex].type, curContent.input.options[choosedOptionIndex].action);
               
                break;

            case InputResultState.nonMatch:
                Debug.Log("어떤 형태로도 맞지 않는다.--> " + curContent.input.options[nonMatchedInputActionIndex].keyword);
                ActByActionType(curContent.input.options[nonMatchedInputActionIndex].type, curContent.input.options[nonMatchedInputActionIndex].action);
               
                break;

            case InputResultState.empty:
                Debug.LogError("Input result is empty , check Json keyword is right ");
                break;
        }

        curContentIndex++;
        //sssssss
        curDialogState = DialogState.playDialog;
    }


    public void ClickDebugTrigger()
    {
        isEnterTriggerArea = true;
     
    }

    public IEnumerator LoadAudios(string path)
    {
        string[] files = path.Split('/');
        string file = files[files.Length - 1];

        string targetPath = Application.persistentDataPath + "/" + file;

        if (System.IO.File.Exists(targetPath))
        {

            WWW audioLoader = new WWW("file://"+targetPath);

            yield return audioLoader;
            AudioClip resultClip = audioLoader.GetAudioClip(false, true) as AudioClip;
;
            Debug.Log(resultClip + "오디오 로드 완료-->" + targetPath);
            //link data to sorter
            a_sources[2].clip = resultClip;
            a_sources[2].Play();

        }
        else
        {
      
        }

    }


    public void SetArTargetToImage()
    {
        Debug.Log(selectData.data.sources.Length+"set ar target");
        int sourceLength = selectData.data.sources.Length;
        int matchedSourceIndex = 0;
        int matchedImageIndex = 0;

        GameObject[] arImages = GameObject.FindGameObjectsWithTag("imageTarget");
        list_ArContent = new ArEventer[arImages.Length];
        for (int i = 0; i < arImages.Length; i++)
        {
            list_ArContent[i] = arImages[i].GetComponent<ArEventer>();

        }

        for (int i = 0; i < list_ArContent.Length; i++)
        {
            for (int e = 0; e < sourceLength; e++)
            {
                if (list_ArContent[i].myArId == selectData.data.sources[e].name)
                {
                    matchedSourceIndex = e;
                    matchedImageIndex = i;
                    break;
                }
                
            }

            Debug.Log("image set" + (selectData.data.sources[matchedSourceIndex].name));

            switch (selectData.data.sources[matchedSourceIndex].sourcetype)
            {
                case "video":
                    list_ArContent[matchedImageIndex].GetComponentInChildren<VideoPlayer>().url = selectData.data.sources[matchedSourceIndex].resultlink;
                    break;

                case "bundle":
                    Debug.Log("bundle set on target");
                    //Debug.Log("bundle set"+ (selectData.data.sources[matchedSourceIndex].loadObject));

                    //GameObject arbundle = Instantiate(selectData.data.sources[matchedSourceIndex].loadObject);
                    //arbundle.transform.parent = list_ArContent[matchedImageIndex].transform.GetChild(0);
                    //arbundle.transform.localPosition = Vector3.zero;
                    //arbundle.transform.localRotation = Quaternion.identity;
                    //arbundle.transform.localScale = Vector3.one;

                    break;
            }


        }
     
    }

}
