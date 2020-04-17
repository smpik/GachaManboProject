using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButtonController : MonoBehaviour
{
	private GameObject TitleCanvas;

    // Start is called before the first frame update
    void Start()
    {
		TitleCanvas = GameObject.Find("TitleCanvas");
    }

	/* StartButtonがタップされた時の処理	*/
	public void TapStartButton()
	{
		TitleCanvas.SetActive(false);//TitleCanvasを閉じる
		//SE再生(やりたいなら。。。)
	}
}
