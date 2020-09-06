using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SubmitInputField : InputField
{
    [Serializable]
    public class KeyboardDoneEvent : UnityEvent { }

    [SerializeField]
    private KeyboardDoneEvent m_keyboardDone = new KeyboardDoneEvent();

    public KeyboardDoneEvent onKeyboardDone
    {
        get { return m_keyboardDone; }
        set { m_keyboardDone = value; }
    }


    // Start is called before the first frame update
    public InputField myfield;
    void Awake()
    {
        myfield = transform.GetComponentInChildren<InputField>();

    }

    bool CheckKey = false;
    IEnumerator CheckInputFIeld()
    {
        while(CheckKey)
        {
            //Debug.Log("++++++++check+++++++++++++++");
            if (Input.GetKeyDown(KeyCode.Return))
            {
                Debug.Log("+++++++++keycode press return++++++++++++++++");
            }
          //  m_keyboardDone.Invoke();
        }
        Debug.Log("inputfield select");
      
        yield return null;
    }
   
    public void CheckKeyValue()
    {
        if(!CheckKey)
        {
            CheckKey = true;
            Debug.Log("+++++++++CheckKey+"+ CheckKey);
           // StartCoroutine(CheckInputFIeld());
        }
     
        //Event currnetKeyboard = Event.current;
        //Debug.Log("currentKeytype" + currnetKeyboard.keyCode);
        //if (currnetKeyboard.keyCode == KeyCode.Return)
        //{
        //    Debug.Log("input key press return values");
        //}
    }

    public void CheckKeyboardEnd()
    {
        CheckKey = false;
        Event currnetKeyboard = Event.current;
        Debug.Log("edit end");
 
        Debug.Log("keyboard value" + m_Keyboard);
        Debug.Log("currentKeytype" + currnetKeyboard.keyCode);

        if (currnetKeyboard.keyCode == KeyCode.Return )
        {
            CheckInputValue();
        }

        if (currnetKeyboard.keyCode == KeyCode.Delete)
        {
            Debug.Log("delete key");
        }
    }


    void CheckInputValue()
    {
        Debug.Log("input key press return values");
    }

    


}