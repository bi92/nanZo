using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class TextTyper : MonoBehaviour
{

    TextMeshProUGUI textBox;
    string myText;
    //public string[] textLines;

    [Header("타이핑 속도")]
    public float typingDelay = 0.125f;
    public bool typindisEnd = false;

    public delegate void OnDescriptionEnd();
    public event OnDescriptionEnd descriptionTypingEnd;

    // Start is called before the first frame update
    public enum typerType
    {
        bubble,
        fixedTxt
    }

    public typerType typingType;

    void Awake()
    {
        textBox = GetComponent<TextMeshProUGUI>();
        
       
    }

    private void OnEnable()
    {
        if(typingType == typerType.fixedTxt)
        {
            Debug.Log("cur textbox text" + textBox.text);
            OnStartTyping();
        }
    }



    public void OnStartTyping()
    {
        myText = textBox.text;
        textBox.text = string.Empty;
        StartCoroutine(TypingText(myText, typingDelay));

    }

    IEnumerator TypingText( string mytext, float typingDelay)
    {

        yield return new WaitForSeconds(0.5f);
        foreach (char c in mytext)
        {
            
            textBox.text += c;
            yield return new WaitForSeconds(typingDelay);
        }


        OnTypingEnd();

    }

    public void OnTypingEnd()
    {
        Debug.Log("typing is end");

        switch(typingType)
        {
            case typerType.bubble:
                DialogSequencer.contentTypingEnd = true;
                break;

            case typerType.fixedTxt:
                typindisEnd = true;
                descriptionTypingEnd();
                break;

        }
    

    }

    
}
