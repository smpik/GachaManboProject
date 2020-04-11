using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinEventStockManager : MonoBehaviour
{
	private struct CoinEventStockLampStruct
	{
		public GameObject OnObject;
		public GameObject OffObject;
		public bool DisplayState;
	}
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
	private const int NUM_COIN_EVENT_MAX = 3;//コイン放出イベントの種類の最大数
	private const int NUM_COIN_EVENT_MIN = 0;//コイン放出イベント最小(もはや意味が分からないがfor文で使用するために無理やり定数定義)
	private const int NUM_STOCK_MAX = 3;
	private const int NUM_STOCK_MIN = 0;
	private const bool ON = true;
	private const bool OFF = false;

	private CoinEventStockLampStruct[,] CoinEventStockLampInfo;//[コイン放出イベントの数,ストックの数]
	private int[] CoinEventStock;//コイン放出イベントストック格納配列

	private CoinEventController CoinEventController;

	//==============================================================================//
	//	初期化関数																	//
	//==============================================================================//
	void Start()
    {
		CoinEventController = GameObject.Find("EnterCoinGate").GetComponent<CoinEventController>();

		generateInstance();
		initCoinEventStockLampInfo();
	}
	/* インスタンス生成	*/
	private void generateInstance()
	{
		CoinEventStock = new int[NUM_COIN_EVENT_MAX] { 0, 0, 0 };//最初はいずれのイベントストックも0

		CoinEventStockLampInfo = new CoinEventStockLampStruct[NUM_COIN_EVENT_MAX, NUM_STOCK_MAX];//ストックランプ情報の生成
	}
	/* ストックランプ情報の初期化	*/
	private void initCoinEventStockLampInfo()
	{
		for(int coinEventId=NUM_COIN_EVENT_MIN;coinEventId<NUM_COIN_EVENT_MAX;coinEventId++)
		{
			for(int lamp=NUM_STOCK_MIN;lamp<NUM_STOCK_MAX;lamp++)
			{
				CoinEventStockLampInfo[coinEventId, lamp].OnObject = GameObject.Find("Coin"+PATTERN_COIN_EVENT[coinEventId]+"EventStock"+(lamp+1)+"ON");//GameObject名を取得(+1しないとGameObject名とずれてしまう)
				CoinEventStockLampInfo[coinEventId, lamp].OffObject = GameObject.Find("Coin"+PATTERN_COIN_EVENT[coinEventId]+"EventStock"+(lamp+1)+"OFF");//GameObject名を取得(+1しないとGameObject名とずれてしまう)
				CoinEventStockLampInfo[coinEventId, lamp].DisplayState = OFF;//初期時の表示状態は消灯
			}
		}
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
			updateCoinEventStockLampDisplayState(id);//ストックランプの表示状態の更新(ストックに変化があったIDのみ)
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
	/* ストックランプの表示状態の更新	*/
	private void updateCoinEventStockLampDisplayState(COIN_EVENT_ID id)
	{
		settingTurnOnLamp(id);//ストックに応じて点灯する必要のあるランプの設定
		outputCoinEventStockLampDisplayState(id);//出力
	}
	private void settingTurnOnLamp(COIN_EVENT_ID id)
	{
		/* 基本消灯(点灯するなら上書きされる)	*/
		for(int lamp=NUM_STOCK_MIN;lamp<NUM_STOCK_MAX;lamp++)
		{
			CoinEventStockLampInfo[(int)id, lamp].DisplayState = OFF;
		}
		/* ストックに応じてつける必要があるならONに設定	*/
		for(int lamp=NUM_STOCK_MIN;lamp<CoinEventStock[(int)id];lamp++)
		{
			CoinEventStockLampInfo[(int)id, lamp].DisplayState = ON;
		}
	}
	private void outputCoinEventStockLampDisplayState(COIN_EVENT_ID id)
	{
		for(int lamp=NUM_STOCK_MIN;lamp<NUM_STOCK_MAX;lamp++)
		{
			CoinEventStockLampInfo[(int)id, lamp].OnObject.SetActive(CoinEventStockLampInfo[(int)id, lamp].DisplayState);
			CoinEventStockLampInfo[(int)id, lamp].OffObject.SetActive(!(CoinEventStockLampInfo[(int)id, lamp].DisplayState));
		}
	}
	//==============================================================================//
	//	Setter、Getter																//
	//==============================================================================//
	public void CountCoinEventStock(COIN_EVENT_ID id)
	{
		CoinEventStock[(int)id]++;//ストック+1
		judgeRequestNecessity(id);
		updateCoinEventStockLampDisplayState(id);//ストックランプの表示状態の更新
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
