using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCoin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void TapButtonCoinEnter()
	{
		GameObject coin = (GameObject)Resources.Load("Prefabs/Coin");
		Instantiate(coin);
	}
}
