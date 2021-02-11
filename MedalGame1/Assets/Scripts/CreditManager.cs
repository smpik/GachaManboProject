using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditManager : MonoBehaviour
{
	private const int NUM_CREDIT_DEFAULT = 100;
	private int Credit;

	private Text CoinCreditText;
	private UIController UIControllerInstance;

	//==============================================================================//
	//	初期化処理																	//
	//==============================================================================//
	void Start()
    {
		Credit = NUM_CREDIT_DEFAULT;

		CoinCreditText = GameObject.Find("CoinCreditValueText").GetComponent<Text>();
		UIControllerInstance = GameObject.Find("Main Camera").GetComponent<UIController>();

		CoinCreditText.text = Credit.ToString();
	}

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

		/* コインがなくなった時の処理 */
		if (Credit == 0)
		{
			UIControllerInstance.SetActiveMoreCreditCanvas(true);	//コインゲットキャンバスを表示する
		}
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

	/**
	 * コイン増やす処理(コインゲットリワード視聴後に呼ばれる)
	 */
	 public void AddManyCredit(int addValue)
	{
		Credit += addValue;//CREDITを増やす
		CoinCreditText.text = Credit.ToString();//UIに出力
	}
}
