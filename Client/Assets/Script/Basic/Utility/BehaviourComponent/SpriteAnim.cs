using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class SpriteAnim : MonoBehaviour
{
    public Sprite[] spriteArray;
    int index = 0;
    Image im;
    public float m_fTimeInt = 0.05f;
    float m_fTime;
    void Start()
    {
        im = this.transform.GetComponent<Image>();
    }
    void OnBecameVisible()
    {
        m_fTime = Time.realtimeSinceStartup;
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (im != null)
        {
            if (Time.realtimeSinceStartup - m_fTime >= m_fTimeInt)
            {
                if (index < spriteArray.Length)
                {
                    im.sprite = spriteArray[index];
                    index++;
                }
                if (index >= spriteArray.Length)
                {
                    index = 0;
                }
                m_fTime = Time.realtimeSinceStartup;
            }
        }
    }
}
