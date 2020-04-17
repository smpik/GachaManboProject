using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCoin : MonoBehaviour
{
	private CreditManager CreditManagerInstance;
	private GameObject CoinPrefab;

    // Start is called before the first frame update
    void Start()
    {
		CreditManagerInstance = GameObject.Find("EnterCoinGate").GetComponent<CreditManager>();
		CoinPrefab = (GameObject)Resources.Load("Prefabs/Coin");
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
		Instantiate(CoinPrefab);
	}
}
