using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
	private const float VALUE_ROTATE_SPEED = 0.5f;

	private GameObject Lights;

    // Start is called before the first frame update
    void Start()
    {
		Lights = GameObject.Find("Lights");
    }

    // Update is called once per frame
    void Update()
    {
		Lights.transform.Rotate(0, 0, VALUE_ROTATE_SPEED);
    }
}
