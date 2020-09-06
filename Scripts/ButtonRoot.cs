using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//create buttons and grab data link
public class ButtonRoot : MonoBehaviour
{

    //총 버튼 갯수 - 데이터와 연결
    public int buttonCount;

    public string[] debug_buttonText;

    //prefab which create button type
    public GameObject buttonPrefab;

    
    //get valuelink
    txtUnit description;

 
    //use only inside of this script
    buttonUnit[] _buttons;

    public Buble bubbleScr;
    buttonUnit[] buttons
    {
        get { return _buttons; }
        set
        {
            _buttons = value;

            //creat button and connect data
            CreateButtonOnScene();
            ConnectObjectAndData();
        }

    }


    public struct txtUnit
    {
        public Text txt;
        public string txt_string;
        public RectTransform area;
        public RectTransform txt_transform;

    }

  
    public struct buttonUnit
    {
        public Text _text;
        public RectTransform area;
        public Button btn;

    }

    // get button data
    void Awake()
    {
       
        //갯수만큼 생성
        buttons = new buttonUnit[buttonCount];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateButtonOnScene()
    {

        for (int i = 0; i < buttonCount; i++)
        {
            GameObject createdUnit = Instantiate(buttonPrefab);
            createdUnit.transform.parent = transform;
            createdUnit.name = ("button_"+i);

            createdUnit.transform.SetSiblingIndex(i);
            
        }

        
    }


    void ConnectObjectAndData()
    {

        for (int i = 0; i < buttonCount; i++)
        {
            buttons[i]._text = transform.GetChild(i).GetComponentInChildren<Text>();
            buttons[i].area = transform.GetChild(i).GetComponent<RectTransform>();
            buttons[i].btn = transform.GetChild(i).GetComponent<Button>();

        }

        float ySpacePixel = buttons[0].area.sizeDelta.y;
        //set values to json data value

        for (int i = 0; i < buttonCount; i++)
        {
            buttons[i].area.anchoredPosition = new Vector2(0, ySpacePixel*(buttonCount-i-1));
            buttons[i]._text.text = debug_buttonText[i];
            
        }
        bubbleScr.buttonEndHeight =( buttons[0].area.anchoredPosition.y + buttons[0].area.rect.height);

    }


}
