using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Buble : MonoBehaviour
{

    #region txt area

    private string _myString;
    public string myString
    {
        get { return _myString; }
        set
        {
            if (value != _myString)
            {
                _myString = value;

                //bubble resize
                CheckBubleSize();

            }
          
        }
    }

    [Header("data와 연결해야 하는 string 변수")]
    public string dataString;
    [Header("텍스트 1줄당 pixel 값")]
    public float linePerPixel;

    
    #endregion



    #region component area

    
    //bubble background image
    public RectTransform bgImage;

    //description + messagetxt -> fit on text size
    public RectTransform txt_area;
    //description + messagetxt
    public TextMeshProUGUI ui_const;

    public Color myTextColor;

    //id from content data
    public int myId;
    public int myDialogNumber;
   
    Vector2 bgsize;
    #endregion

    #region bubbletype area

    public enum _bubbletype
    {
        message,
        button,
        messageBtn,
        inputField
    }

    public _bubbletype bubbletype;

    #endregion


    #region messageBtn Area


    //description + messagetxt
    public ButtonRoot Root_buttons;

    //description 
    public RectTransform Root_description;

    public float _buttonEndHeight;

    public float buttonEndHeight
    {
        get { return _buttonEndHeight; }
        set
        {
            _buttonEndHeight = value;
            //create button end and resize button bubbles
            CheckBubleSize();
        }
    }

    #endregion


    #region struct Area

    public struct txtUnit
    {
        public  TextMeshProUGUI txt;
        public string txtString;
        public RectTransform area;
        public RectTransform txtTransform;

    }


    #endregion


    #region event Area

    public delegate void ChangeBubbleSize(float changedHeight);
    //텍스트 크기에 맞춰서 문자열 범위 사이즈 변화시 호출
    public  event ChangeBubbleSize onBubbleResize;

    #endregion

    public string myActionString;
    public StructCollection.actiontype myactionType;
  
    void Awake()
    {
        //set values which each type have
        switch (bubbletype)
        {
            case _bubbletype.message:

                AwakeMessageBuble();
                break;

            case _bubbletype.messageBtn:
                AwakeButtonBuble();
                break;


            case _bubbletype.button:

                AwakeMessageBuble();
                break;

            case _bubbletype.inputField:

             //   AwakeMessageBuble();
                break;
        }
    }


    void AwakeMessageBuble()
    {
        //set first size
        bgsize = bgImage.sizeDelta;

    }

    void AwakeButtonBuble()
    {
      

    }


    void CheckBubleSize()
    {
   

        switch (bubbletype)
        {
            case _bubbletype.message:
                //resize area
                ResizeMessageBubble();

                break;

            case _bubbletype.button:
                //resize area
                ResizeButtonBubble();

                break;

            case _bubbletype.messageBtn:
             //   ResizeButtonBubble(Root_buttons.buttonCount);
                break;

            case _bubbletype.inputField:

           
                break;
        }
    
        
    }

    //vertical - overflow -> check overflow on horizontal
    void ResizeMessageBubble()
    {
        SetUiExpandZero();

        myTextColor = ui_const.color;
        //disabled
        ui_const.color = Color.clear;
        //put data string to txtbox
        ui_const.text = myString;

        //text area component value
        float stringAreaHeight = ui_const.rectTransform.rect.height;
        float changedHeight = LayoutUtility.GetPreferredHeight(ui_const.rectTransform);


        // change value on bgimage + profile 
        float difHeight = (changedHeight - stringAreaHeight);
       

        //set size on text
        txt_area.sizeDelta = new Vector2(txt_area.rect.width, changedHeight);
        ui_const.rectTransform.sizeDelta = new Vector2(txt_area.rect.width, changedHeight);

        //set size and position on profile & bubble backgroundImage
        bgImage.sizeDelta = new Vector2(bgsize.x, bgImage.sizeDelta.y + (difHeight));
        float movedHeight = bgImage.sizeDelta.y;
        onBubbleResize(movedHeight);

    }

    public Text debug_descriptionText;
    //button bubble - action type message
    void ResizeButtonBubble()
    {

        SetUiExpandZero();
        ui_const.text = myString;
        onBubbleResize(bgImage.sizeDelta.y);
    }

    public void ClickButtonBubble()
    {
     
        DataSorter.instance.d_sequencer.clicked_bubble = this.GetComponent<Buble>();
        DataSorter.instance.d_sequencer.clickedButtonId = myId;
      
        switch(myactionType)
        {
            case StructCollection.actiontype.gotoMap:
                DataSorter.instance.d_sequencer.tabManager.OnMapTabClick();
                break;

            case StructCollection.actiontype.gotoCamera:
                DataSorter.instance.d_sequencer.tabManager.OnARTabClick();
                break;
        }

        //click only once
       transform.GetComponentInChildren<Button>().enabled = false;

    }


    public void PutValueInInputField()
    {

        TMP_InputField inputfield = transform.GetComponentInChildren<TMP_InputField>();

        if (inputfield.text != "")
        {
            DialogSequencer.choosedInput = inputfield.text;
            DataSorter.instance.d_sequencer.ActByInputValue();
            ResetInputField();
            this.gameObject.SetActive(false);

        }
    
    }


    public void ResetInputField()
    {
       
        TMP_InputField inputfield = transform.GetComponentInChildren<TMP_InputField>();
        inputfield.text = ("");
        Debug.Log("active off on input field");
        transform.GetComponentInChildren<TMP_InputField>().enabled = true;
     

    }


    public void SetUiExpandZero()
    {
        RectTransform myRt = transform.GetComponent<RectTransform>();
        myRt.offsetMin = new Vector2(0, myRt.offsetMin.y);
        myRt.offsetMax = new Vector2(0, myRt.offsetMax.y);

        if (ui_const != null)
        {
            ui_const.rectTransform.offsetMin = new Vector2(0, ui_const.rectTransform.offsetMin.y);
            ui_const.rectTransform.offsetMax = new Vector2(0, ui_const.rectTransform.offsetMax.y);

        }

        if(txt_area != null)
        {
            txt_area.offsetMin = new Vector2(0, txt_area.offsetMin.y);
            txt_area.offsetMax = new Vector2(0, txt_area.offsetMax.y);
        }
        
    }



    public void ResizeCanvasOnKeyboard()
    {

    }


}
