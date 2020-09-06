using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TxtColorHolder : MonoBehaviour
{

    public Color myColor;

    private void Awake()
    {
        TextMeshProUGUI myTMesh = GetComponent<TextMeshProUGUI>();
        myColor = myTMesh.color;
    }


}
