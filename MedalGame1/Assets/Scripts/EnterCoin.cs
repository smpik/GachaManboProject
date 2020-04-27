using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCoin : MonoBehaviour
{
	private const int TIME_WAIT_COIN_ENTER = 5;//コイン連続投入を制限するための待ち時間

	private CreditManager CreditManagerInstance;
	private EnterCoinGateSensorController EnterCoinGateSensorControllerInstance;
	private GameObject CoinPrefab;

    // Start is called before the first frame update
    void Start()
    {
		CreditManagerInstance = GameObject.Find("EnterCoinGate").GetComponent<CreditManager>();
		EnterCoinGateSensorControllerInstance = GameObject.Find("EnterCoinGateSensor").GetComponent<EnterCoinGateSensorController>();
		CoinPrefab = (GameObject)Resources.Load("Prefabs/Coin");
	}

	public void TapButtonCoinEnter()
	{
		if (EnterCoinGateSensorControllerInstance.IsCoinNothing() == true)
		{
			if (CreditManagerInstance.IsCoinEnterPermited() == true)//Creditあるか確認
			{   //あるなら
				createCoin();//コイン生成
				CreditManagerInstance.SubtractCredit();//クレジット--
			}
		}
	}
	public void createCoin()
	{
		Instantiate(CoinPrefab);
	}
}
