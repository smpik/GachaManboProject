using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolusionController : MonoBehaviour
{
	void Awake()
	{
		float screenRate = (float)1024 / Screen.height;
		if (screenRate > 1) screenRate = 1;
		int width = (int)(Screen.width * screenRate);
		int height = (int)(Screen.height * screenRate);
		Screen.SetResolution(width, height, true, 15);
	}
}
