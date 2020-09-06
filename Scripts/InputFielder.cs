using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFielder : MonoBehaviour
{

    private TouchScreenKeyboard keyboard;
    // Start is called before the first frame update
    void Start()
    {
    
    }


    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 200, 100), "Default"))
        {
        
        }
    }
    // Update is called once per frame
    void Update()
    {
      
    }

    public void PopUpKeyBoard()
    {
       // keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
    }

}
