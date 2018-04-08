//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections;
//using System.Collections.Generic;

//class UI_Talk : UIBehaviour
//{
//    public Text text;
//    public Image image;




//    IEnumerator ShowTalk(string talk)
//    {
//        StartCoroutine(SetText(talk));
//        float time = Time.realtimeSinceStartup;
//        yield return new WaitForSeconds(1.5f);
//        while (Time.realtimeSinceStartup - time <= 3.5f)
//        {
//            float delt_alph = Mathf.Lerp(1, 0, (Time.realtimeSinceStartup - time) / 3.5f);
//            GetComponent<CanvasGroup>().alpha = delt_alph;
//            yield return null;
//        }
//        GetComponent<CanvasGroup>().alpha = 1.0f;
//        gameObject.SetActive(false);
//    }

//    IEnumerator SetText(string talk)
//    {
//        int length = talk.Length;
//        for (int i = (int)(length * 0.4f); i < length; i++)
//        {
//            text.text = (talk.Substring(0, i));
//            yield return new WaitForSeconds(0.15f);
//        }

//    }


//    public void Show(string talk)
//    {
//        gameObject.SetActive(true);
//        StartCoroutine(ShowTalk(talk));
//    }
//}
