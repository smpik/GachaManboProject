using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
	private GameObject ExcludeCanvas;//除外マスで止まった時用のキャンバス
	private GameObject AdChoiceCanvas;//広告動画再生選択時のキャンバス

    // Start is called before the first frame update
    void Start()
    {
		ExcludeCanvas = GameObject.Find("ExcludeCanvas");
		AdChoiceCanvas = GameObject.Find("AdChoiceCanvas");
		SetActiveExcludeCanvas(false);
		SetActiveAdChoiceCanvas(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetActiveExcludeCanvas(bool displayRequest)
	{
		ExcludeCanvas.SetActive(displayRequest);
	}
	public void SetActiveAdChoiceCanvas(bool displayRequest)
	{
		AdChoiceCanvas.SetActive(displayRequest);
	}
	public bool GetActiveStateExcludeCanvas()
	{
		bool ret = false;

		if(GameObject.Find("ExcludeCanvas"))
		{
			ret = true;
		}

		return ret;
	}

}
