using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCoin : MonoBehaviour
{
	private CreditManager CreditManagerInstance;

    // Start is called before the first frame update
    void Start()
    {
		CreditManagerInstance = GameObject.Find("EnterCoinGate").GetComponent<CreditManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void TapButtonCoinEnter()
	{
		if (CreditManagerInstance.IsCoinEnterPermited() == true)//Creditあるか確認
		{   //あるなら
			createCoin();//コイン生成
			CreditManagerInstance.SubtractCredit();//クレジット--
		}
	}
	public void createCoin()
	{
		GameObject coin = (GameObject)Resources.Load("Prefabs/Coin");
		Instantiate(coin);
	}
}
