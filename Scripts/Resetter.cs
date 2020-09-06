using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Resetter : MonoBehaviour
{


    public float waitTime = 5f;

    [Header("credit 창")]
    public GameObject pannel_credit;

    [Header("end page 창")]
    public GameObject pannel_endPage;

    
    public static bool isCreditEnd = false;

    public Animator yellowTransition;

    public TextTyper[] creditTexts = new TextTyper[4];
    int typenumber;
    private void Awake()
    {
        pannel_credit.gameObject.SetActive(true);
        pannel_endPage.gameObject.SetActive(false);

     
      //  StartCoroutine(MoveToEndPage(waitTime));
    }

    IEnumerator MoveToEndPage( float waitTime)
    {
        yield return new WaitUntil(() => isCreditEnd);

        yield return new WaitForSeconds(waitTime);
        yellowTransition.SetTrigger("movetoend");
      
        
    }

    public void ChangePageToEnd()
    {
        pannel_credit.SetActive(false);
        pannel_endPage.SetActive(true);
    }

    public void MoveToSelectScene()
    {
        SceneManager.LoadScene("S_01_SelectScene");

    }

    void MoveNextTyping()
    {
        if(typenumber<creditTexts.Length-1)
        {
            //creditTexts[typenumber].descriptionTypingEnd -= MoveNextTyping;
            typenumber++;
            creditTexts[typenumber].transform.gameObject.SetActive(true);
            creditTexts[typenumber].descriptionTypingEnd += MoveNextTyping;

        }
        else
        {
            Debug.Log("credit is end");
            StartCoroutine(MoveToEndPage(1));
        }
    }

   public void StartCredit()
    {
        creditTexts[0].transform.gameObject.SetActive(true);
        creditTexts[0].descriptionTypingEnd += MoveNextTyping;

        typenumber = 0;
    }

}
