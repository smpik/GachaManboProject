using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCoin : MonoBehaviour
{
	private const int TIME_WAIT_COIN_ENTER = 5;//コイン連続投入を制限するための待ち時間

	private CreditManager CreditManagerInstance;
	private GameObject CoinPrefab;

	private int WaitEnterCoinTimer;//コイン連続投入制限用待ち時間タイマー


    // Start is called before the first frame update
    void Start()
    {
		CreditManagerInstance = GameObject.Find("EnterCoinGate").GetComponent<CreditManager>();
		CoinPrefab = (GameObject)Resources.Load("Prefabs/Coin");
		WaitEnterCoinTimer = 0;
	}

	void Update()
	{
		if(WaitEnterCoinTimer>0)//コイン連続投入防止用待ち時間があるなら
		{
			WaitEnterCoinTimer--;//カウントする
		}
	}

	public void TapButtonCoinEnter()
	{
		if(WaitEnterCoinTimer<=0)//コイン連続投入防止用待ち時間がなしなら(コイン投入してよいなら)
		{
			if (CreditManagerInstance.IsCoinEnterPermited() == true)//Creditあるか確認
			{   //あるなら
				createCoin();//コイン生成
				CreditManagerInstance.SubtractCredit();//クレジット--
				WaitEnterCoinTimer = TIME_WAIT_COIN_ENTER;//コイン連続投入防止用待ち時間をセット
			}
		}
	}
	public void createCoin()
	{
		Instantiate(CoinPrefab);
	}
}
