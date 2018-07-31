using UnityEngine;
using System.Collections;

public class AnimationSpriteSheet : MonoBehaviour {

	public int uvX = 4;  
	public int uvY = 2; 
	public float fps = 30.0f;
	public float endTime	=	-1;
	private float internalTime;
	private float internalTime2;
	private float sucureTime;

	// Use this for initialization
	void Start () {
		internalTime = Time.time;
		internalTime2 = Time.time;
	}



	// Update is called once per frame
	void Update () {

		if(endTime!=-1)
		{
			if(Time.time-internalTime2>=endTime)
			{
				reSet();
			}
		}

		sucureTime += Time.time - internalTime;
		internalTime = Time.time;
		
		int index  = (int)(sucureTime * fps);
		
		index = index % (uvX * uvY);
		
		
		Vector2 size = new Vector2 (1.0f / (float)uvX, 1.0f / (float)uvY);
		
		int uIndex = index % uvX;
		int vIndex = index / uvX;
		Vector2 offset = new Vector2 ((float)uIndex * size.x, 1.0f - size.y - (float)vIndex * size.y);
		
		GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", offset);
		GetComponent<Renderer>().material.SetTextureScale ("_MainTex", size);
	}

	private void reSet()
	{
		GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", new Vector2(0,0));
		GetComponent<Renderer>().material.SetTextureScale ("_MainTex", new Vector2(1,1));
		internalTime2	=	Time.time;
	}
}
