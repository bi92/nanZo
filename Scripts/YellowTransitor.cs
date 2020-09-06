using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowTransitor : MonoBehaviour
{
    public Resetter resetter;
    private void Awake()
    {
        resetter = transform.root.GetComponent<Resetter>();
    }

    public void ActivatePage02()
    {
        resetter.ChangePageToEnd();
    }

}
