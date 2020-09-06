using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaConnector : MonoBehaviour
{
    
    public RectTransform safePannel;
    Rect LastSafeArea = new Rect(0, 0, 0, 0);

    
    // Start is called before the first frame update

    void Awake()
    {
        Refresh();

    }

    void Update()
    {
        
       Refresh();
    }

    void Refresh()
    {
        Rect curSafeArea = GetSafeArea();
        
        if (LastSafeArea != curSafeArea)
            ApplySafeArea(curSafeArea);
    }

    Rect GetSafeArea()
    {
        return Screen.safeArea;
    }


    void ApplySafeArea(Rect r)
    {
      
        LastSafeArea = r;

        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        safePannel.anchorMin = anchorMin;
        safePannel.anchorMax = anchorMax;
        Debug.Log("my screen safe area ->" + LastSafeArea.height + "/" + LastSafeArea.width);
        Debug.Log("full size  area ->" + Screen.height +"/" + Screen.width);
    }



}
