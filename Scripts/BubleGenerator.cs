using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructCollection;
using UnityEngine.UI;

public class BubleGenerator : MonoBehaviour
{

    [Header("message prefab")]
    public GameObject messagePrefab;

    [Header("button prefab")]
    public GameObject buttonPrefab;
    //generate bubble
    public Transform rootMessageAnchor;

    //0 -  exMessageSpace, 1- curMessageSpace
    public static Transform[] bubbleSpace = new Transform[2];
   
    public GameObject curBubble;
    public RectTransform exBubble;
    public RectTransform scrollArea;

    ScrollRect scrollAreaRect;

   public RectTransform curDialogBg;

    [Header("DIALOG BACKGROUND IMAGE")]
    public GameObject dialogBgPrefab;

    public delegate void ContentTypingEnd();
    public event ContentTypingEnd isTypingEnd;

    void Awake()
    {
       // isTypingEnd += GenerateEnd;
        //get messageSpace transfrom
        for (int i = 0; i < 1; i++)
        {
            bubbleSpace[i] = rootMessageAnchor.GetChild(i);
        }

        scrollAreaRect = transform.GetComponentInChildren<ScrollRect>();
    }


    GameObject newBubbleObject;
    public void InstantiateMessage(Content contentData, string dataString)
    {
        actiontype type = actiontype.notSet;
        string actionValue = null;
        switch(contentData.type)
        {
            case contentType.message:
                newBubbleObject = messagePrefab;
                break;

            case contentType.button:
                newBubbleObject = buttonPrefab;
                
                break;

        }

        //register ex message for reposition after instantiate curmessage
        if (curBubble != null)
        {
            exBubble = curBubble.GetComponent<RectTransform>();
        }

        //instantiate exmessage
        curBubble = Instantiate(newBubbleObject);
        Buble curMessageBubble = curBubble.GetComponent<Buble>();

        if(contentData.type == contentType.button)
        {
            curMessageBubble.myactionType = contentData.button.type;
            
        }
        //register curmessage to curspace
        curBubble.transform.SetParent(bubbleSpace[0]);

        //link on bubble size
        curMessageBubble.onBubbleResize += RepositionBubblePanel;
        curMessageBubble.myString = dataString;

        curMessageBubble.myId = contentData.id;
        curMessageBubble.myDialogNumber = contentData.dialogNumber;

    }

    //used only input field message -> not connect content list only for ui arrange
    public void InstantiateMessage(contentType type, string dataString)
    {

        switch (type)
        {
            case contentType.message:
                newBubbleObject = messagePrefab;
                break;

            case contentType.button:
                newBubbleObject = buttonPrefab;
                break;

        }

        if (curBubble != null)
        {
            exBubble = curBubble.GetComponent<RectTransform>();
        }
      

        //instantiate exmessage
        curBubble = Instantiate(newBubbleObject);

        Buble curMessageBubble = curBubble.GetComponent<Buble>();
        //register curmessage to curspace
        curBubble.transform.SetParent(bubbleSpace[0]);

        //resize bubble bg
        float bgsize = curDialogBg.sizeDelta.y - inputFieldSize-10;

        curDialogBg.offsetMax = new Vector2(0, bgsize);
        curDialogBg.offsetMin = new Vector2(0, 0);
        curDialogBg.transform.localPosition = Vector3.zero;

      


        //link on bubble size
        curMessageBubble.onBubbleResize += RepositionBubblePanel;

        //set text area to right anchor(input answer only)
        curMessageBubble.ui_const.alignment = TMPro.TextAlignmentOptions.MidlineRight; 
        curMessageBubble.myString = dataString;


    }


    public float exHeight;
    // call when bubble resize is over
    bool overedViewportDefault;
    void RepositionBubblePanel( float movedHeight)
    {
       
        RectTransform curRect = curBubble.GetComponent<RectTransform>();
        curRect.localScale = new Vector3(1, 1, 1);
        float yposition;
        Buble curBubleComponent = curBubble.GetComponent<Buble>();

        if (exBubble == null)
        {
            exHeight = 0;
            yposition = 0;
        }
        else
        {
            exHeight = exBubble.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            yposition = Mathf.Abs(exBubble.transform.localPosition.y);
        }

        if (DialogSequencer.curContentIndex == 0)
        {
            GameObject dialogBg = Instantiate(dialogBgPrefab);
            curDialogBg = dialogBg.GetComponent<RectTransform>();
            curDialogBg.parent = curBubble.transform;
            curDialogBg.transform.SetAsFirstSibling();
            curDialogBg.transform.localPosition = Vector3.zero;

            Debug.Log("content index is zero, will instantiate dialog prefabs");
            if (DataSorter.instance.curDialogIndex != 0)
            {
                yposition = Mathf.Abs(exBubble.transform.localPosition.y - 30);
            }
            Debug.Log("reset local position" + curDialogBg.transform.localPosition);
           // curDialogBg.sizeDelta = new Vector2(1, 1);
            curDialogBg.localScale = new Vector3(1,1,1);

        }

        curBubble.GetComponent<Buble>().SetUiExpandZero();
        curBubble.transform.localPosition = new Vector3(0, (yposition + exHeight)*-1, 0);
        float bgsize = curDialogBg.sizeDelta.y + curBubble.GetComponent<Buble>().bgImage.sizeDelta.y;

      //  exHeight = curBubble.GetComponent<Buble>().bgImage.sizeDelta.y+30;
        exHeight = curBubble.GetComponent<Buble>().bgImage.sizeDelta.y+10;
        float scrollAreaSizeY = Mathf.Abs(bubbleSpace[0].GetChild(bubbleSpace[0].childCount - 1).transform.localPosition.y) + exHeight;
       
        if (scrollArea.offsetMax.y <= scrollAreaSizeY)
        {
            overedViewportDefault = true;
         
        }

        if(overedViewportDefault)
        {
            
            scrollArea.offsetMax = new Vector2(0, scrollAreaSizeY);
            scrollArea.offsetMin = new Vector2(0, 0);

        }

        //bgsize = curDialogBg.sizeDelta.y + curBubble.GetComponent<Buble>().bgImage.sizeDelta.y;

        Debug.Log("cur background's size" + curBubble.GetComponent<Buble>().bgImage.sizeDelta.y + curBubble.GetComponent<Buble>().myId);
        curDialogBg.offsetMax = new Vector2(0, bgsize);
        curDialogBg.offsetMin = new Vector2(0, 0);
        curDialogBg.transform.localPosition = Vector3.zero;

        //broke the link
        curBubleComponent.onBubbleResize -= RepositionBubblePanel;
      

        if (curBubleComponent.bubbletype ==Buble._bubbletype.message)
        {
            curBubleComponent.ui_const.color = curBubleComponent.myTextColor;

            //start typing text 
            curBubleComponent.ui_const.GetComponent<TextTyper>().OnStartTyping();
         
        }
        else
        {

        }

    }

    float inputFieldSize = 60;
    public void MoveUpScrollSpace()
    {
        exHeight = inputFieldSize;
        float yposition = Mathf.Abs(curBubble.transform.localPosition.y);

        float bgsize = curDialogBg.sizeDelta.y + exHeight+ 10;
        float scrollAreaSizeY = bgsize + yposition;
        if (scrollArea.offsetMax.y <= scrollAreaSizeY)
        {
            overedViewportDefault = true;

        }

        if (overedViewportDefault)
        {
         
            scrollAreaSizeY = 30 + exHeight + scrollArea.offsetMax.y;
            Debug.Log("scroll will change ==>"+ scrollAreaSizeY);
            scrollArea.offsetMax = new Vector2(0, scrollAreaSizeY);
            scrollArea.offsetMin = new Vector2(0, 0);
         

        }
        curDialogBg.offsetMax = new Vector2(0, bgsize);
        curDialogBg.offsetMin = new Vector2(0, 0);
        curDialogBg.transform.localPosition = Vector3.zero;

    }


     void GenerateEnd()
     {
        Debug.Log("type is end will call next generate");

     }

}
