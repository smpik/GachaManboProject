using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
	private GameObject ExcludeCanvas;//除外マスで止まった時用のキャンバス

    // Start is called before the first frame update
    void Start()
    {
		ExcludeCanvas = GameObject.Find("ExcludeCanvas");
		SetActiveExcludeCanvas(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void SetActiveExcludeCanvas(bool displayRequest)
	{
		ExcludeCanvas.SetActive(displayRequest);
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
