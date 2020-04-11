using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditManager : MonoBehaviour
{
	private const int NUM_CREDIT_DEFAULT = 50;
	private int Credit;

	private Text CoinCreditText;

	//==============================================================================//
	//	初期化処理																	//
	//==============================================================================//
	void Start()
    {
		Credit = NUM_CREDIT_DEFAULT;

		CoinCreditText = GameObject.Find("CoinCreditText").GetComponent<Text>();

		CoinCreditText.text = Credit.ToString();
	}

	//==============================================================================//
	//	Update処理																	//
	//==============================================================================//
	void Update()
    {
        
    }
	//==================================================//
	/* 											*/
	//==================================================//

	//==============================================================================//
	//	getter/setter																//
	//==============================================================================//
	public void AddCredit()
	{
		Credit++;//CREDITを+1
		CoinCreditText.text = Credit.ToString();//UIに出力
	}
	public void SubtractCredit()
	{
		Credit--;//CREDITを-1
		CoinCreditText.text = Credit.ToString();//UIに出力
	}
	/* コインを投入できるか判定(CREDITが0なら投入できない)	*/
	public bool IsCoinEnterPermited()
	{
		bool ret=false;//OKなら上書きされる

		if (Credit > 0)//CREDITあるなら
		{
			ret = true;//コイン投入OK
		}

		return ret;
	}
}
