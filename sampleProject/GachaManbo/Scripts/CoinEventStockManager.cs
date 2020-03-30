using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinEventStockManager : MonoBehaviour
{
	public enum COIN_EVENT_ID
	{
		COIN_EVENT_PATTERN_0 = 0,//コイン10放出イベント
		COIN_EVENT_PATTERN_1,//コイン20放出イベント
		COIN_EVENT_PATTERN_2//コイン50放出イベント
	}
	private readonly int[] PATTERN_COIN_EVENT = //コイン放出枚数格納配列(IDごとにコイン放出枚数を決定)
		{
			10,
			20,
			50
		};
	private const int NUM_COIN_EVENT_MAX = 3;//コイン放出イベントの種類の数
	private const int NUM_STOCK_MAX = 3;

	private int[] CoinEventStock;//コイン放出イベントストック格納配列

	private CoinEventController CoinEventController;

	//==============================================================================//
	//	初期化関数																	//
	//==============================================================================//
	void Start()
    {
		CoinEventController = GameObject.Find("EnterCoinGate").GetComponent<CoinEventController>();

		CoinEventStock = new int[NUM_COIN_EVENT_MAX] { 0, 0, 0 };//最初はいずれのイベントストックも0
	}
	//==============================================================================//
	//	private関数																	//
	//==============================================================================//
	private void judgeRequestNecessity(COIN_EVENT_ID id)
	{
		if(CoinEventStock[(int)id]>=NUM_STOCK_MAX)//ストックがたまったら
		{
			requestCoinEvent(id);//コイン放出イベント要求
			CoinEventStock[(int)id] = 0;//ストックのリセット
		}
		else//たまってないなら
		{
			//なにもしない
		}
	}
	private void requestCoinEvent(COIN_EVENT_ID id)
	{
		bool ready = CoinEventController.GetCoinEventIsReadyOk();//コイン放出イベント要求してもよいか確認のためのフラグを取得
		if (ready == true)//コインイベントの準備OKなら
		{
			CoinEventController.SetCoinPayoutRequest(PATTERN_COIN_EVENT[(int)id]);//コイン放出イベント要求
		}
	}
	//==============================================================================//
	//	Setter、Getter																//
	//==============================================================================//
	public void CountCoinEventStock(COIN_EVENT_ID id)
	{
		CoinEventStock[(int)id]++;//ストック+1
		judgeRequestNecessity(id);
		Debug.Log("コイン放出イベントパターン" + id + "のストック：現在" + CoinEventStock[(int)id]);
	}
	public void JudgeRouletteResultIsCoinEventStock(string rouletteResult)
	{/* ルーレット結果がストック+1マスか	*/
		switch(rouletteResult)
		{
			case "Coin10EventStock+1ON":
				CountCoinEventStock(COIN_EVENT_ID.COIN_EVENT_PATTERN_0);//コイン放出イベントパターン0のストック+1
				break;
			case "Coin20EventStock+1ON":
				CountCoinEventStock(COIN_EVENT_ID.COIN_EVENT_PATTERN_1);//コイン放出イベントパターン1のストック+1
				break;
			case "Coin50EventStock+1ON":
				CountCoinEventStock(COIN_EVENT_ID.COIN_EVENT_PATTERN_2);//コイン放出イベントパターン2のストック+1
				break;
			default:
				break;
		}
	}
}
