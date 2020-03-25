using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class debugCode : MonoBehaviour
{
	private SugorokuController sc;

    // Start is called before the first frame update
    void Start()
    {
		sc = GameObject.Find("SugorokuMasu").GetComponent<SugorokuController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void reqStep1()
	{
		sc.JudgeSugorokuStart("1StepON");
	}

	public void reqStep2()
	{
		sc.JudgeSugorokuStart("2StepON");
	}

	public void reqStep3()
	{
		sc.JudgeSugorokuStart("3StepON");
	}


}
