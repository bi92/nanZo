using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructCollection;
using UnityEngine.Video;

public class ArEventer : MonoBehaviour
{

    Transform myChild;

    VideoPlayer myvp;
    public string myArId;
    public int myOrder;
    bool isplaying;
    double myPlaytime;
    private void Awake()
    {
        myChild = transform.GetChild(0);
        isplaying = false;

        if (myChild.GetComponent<VideoPlayer>()!=null)
        {
            myvp = myChild.GetComponent<VideoPlayer>();
        }

        myPlaytime = 0;
    }


    public void ActiveChild()
    {

        myChild = transform.GetChild(0);
        myChild.gameObject.SetActive(true);
        for (int i = 0; i < myChild.childCount; i++)
        {
            myChild.GetChild(i).gameObject.SetActive(true);
        }


        if (myChild.GetComponents<VideoPlayer>() != null)
        {
            if (!isplaying)
            {
                myvp = myChild.GetComponent<VideoPlayer>();
                myvp.transform.GetComponent<MeshRenderer>().enabled = false;

                myvp.time = myPlaytime;
                Debug.Log("playbacktime ->" + myvp.playbackSpeed);

                myvp.Play();
                StartCoroutine(OnVisibleVideo());
                //isplaying = true;
            }

        }

    }

    public void OffVideo()
    {
        if(myvp != null)
        {
            if (!isplaying)
            {
                myPlaytime = myvp.time;
                myvp.Stop();

            }
        }
      
    }

    IEnumerator OnVisibleVideo()
    {
        yield return new WaitForSeconds(0.1f);
        myvp.transform.GetComponent<MeshRenderer>().enabled = true;

    }

    
     
}
