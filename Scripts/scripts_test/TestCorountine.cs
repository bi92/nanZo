using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCorountine : MonoBehaviour
{

    public bool firstOver;
    public float a;
    public void Awake()
    {
       
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("startcoroutine");
            StartCoroutine(DebugSequencerTest());
        }
    }


    IEnumerator DebugSequencerTest()
    {
        StartCoroutine(DebugNewSequencer());
        Debug.Log("call corountine");
        yield return new WaitUntil(() => firstOver);
        Debug.Log("new sequence is start");
    }


    IEnumerator DebugNewSequencer()
    {
        while(a<5)
        {
            yield return new WaitForSeconds(0.6f);
            a += 1;
            Debug.Log("update a plus");
        }

        yield return StartCoroutine(WaitStartCoroutine());

        if (a==5)
        {
            Debug.Log("update a over");
            yield break;
        
        }
        
    }
    IEnumerator WaitStartCoroutine()
    {
        Debug.Log("wait coroutine end");
        firstOver = true;
        yield return null;
    }

}
