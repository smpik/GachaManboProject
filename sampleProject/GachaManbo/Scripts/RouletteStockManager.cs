using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteStockManager : MonoBehaviour
{
	private struct RouletteStockLampStruct
	{
		public GameObject OnObject;
		public GameObject OffObject;
		public bool DisplayState;
	}
	private const int STOCK_MAX = 3;
	private const int STOCK_MIN = 0;
	private const bool ON = true;
	private const bool OFF = false;

	private bool RouletteStockRequest;
	private int RouletteStock;

	private RouletteStockLampStruct[] RouletteStockLampInfo;

	private RouletteController RouletteController;
	private SugorokuController SugorokuController;
	private CoinEventController CoinEventController;

	//==============================================================================//
	//	初期化処理																	//
	//==============================================================================//
	void Start()
    {
		RouletteController = GameObject.Find("RouletteMasu").GetComponent<RouletteController>();
		SugorokuController = GameObject.Find("SugorokuMasu").GetComponent<SugorokuController>();
		CoinEventController = GameObject.Find("EnterCoinGate").GetComponent<CoinEventController>();

		generateInstance();
		initRouletteStockLampInfo();
	}
	/* インスタンス生成	*/
	private void generateInstance()
	{
		RouletteStockLampInfo = new RouletteStockLampStruct[STOCK_MAX];
	}
	/* ストックランプの初期化	*/
	private void initRouletteStockLampInfo()
	{
		for(int lamp=STOCK_MIN;lamp<STOCK_MAX;lamp++)
		{
			RouletteStockLampInfo[lamp].OnObject = GameObject.Find("RouletteStock" + (lamp + 1)+"ON");//GameObject名の取得(+1しないとGameObject名とずれる)
			RouletteStockLampInfo[lamp].OffObject = GameObject.Find("RouletteStock" + (lamp + 1) + "OFF");//GameObject名の取得(+1しないとGameObject名とずれる)
			RouletteStockLampInfo[lamp].DisplayState = OFF;//初期時の表示状態は消灯
		}
	}

	//==============================================================================//
	//	Update処理																	//
	//==============================================================================//
	void Update()
    {
        if(RouletteStockRequest)
		{	//ストック要求があれば	
			countRouletteStock();//ストックをカウント
		}

		if(RouletteStock > STOCK_MIN)
		{   //ストックがあるなら
			if (isRouletteRequestOk() == true)
			{   //ルーレット要求が許可されているなら
				RouletteController.SetRouletteRequest();//ルーレット要求
				decrementRouletteStock();//要求したのでストックを減らす
				Debug.Log("ルーレット要求");
			}
		}
    }
	/* ストックを減らす	*/
	private void decrementRouletteStock()
	{
		if(RouletteStock > STOCK_MIN)
		{	//FSとして0以上のときにしか行わない(そもそも本関数が呼ばれるのは0以上のときだが。。。)
			RouletteStock--;
			updateRouletteStockLampDisplayState();//ストックランプの表示状態更新
		}
	}
	/* ストックをカウント	*/
	private void countRouletteStock()
	{
		if (RouletteStock < STOCK_MAX)
		{
			RouletteStock++;
			updateRouletteStockLampDisplayState();//ストックランプの表示状態更新
		}

		ClearRouletteStockRequest();//ストックをカウントしたのでストック要求をクリア
	}
	/* ルーレット要求許可判定	*/
	private bool isRouletteRequestOk()
	{
		bool ret = false;

		bool roulette = RouletteController.GetRouletteIsReadyOk();//ルーレットが準備OKかを取得
		bool sugoroku = SugorokuController.GetSugorokuIsReadyOk();//すごろくが準備OKかを取得
		bool coinEvent = CoinEventController.GetCoinEventIsReadyOk();//コインイベントが準備OKかを取得

		if((roulette==true)
			&&(sugoroku==true)
			&&(coinEvent==true) )
		{
			ret = true;
		}

		return ret;
	}
	/* ストックランプの表示状態更新	*/
	private void updateRouletteStockLampDisplayState()
	{
		settingTurnOnLamp();//ストックに応じて点灯する必要のあるランプを設定する
		outputRouletteLampDisplayState();//出力する
	}
	private void settingTurnOnLamp()
	{
		/* 基本消灯(つける必要があるなら上書きされる)	*/
		for(int lamp=STOCK_MIN;lamp<STOCK_MAX;lamp++)
		{
			RouletteStockLampInfo[lamp].DisplayState = OFF;
		}
		/* ストックに応じてつける必要があるならONに設定	*/
		for(int lamp=STOCK_MIN;lamp<RouletteStock;lamp++)
		{
			RouletteStockLampInfo[lamp].DisplayState = ON;
		}
	}
	private void outputRouletteLampDisplayState()
	{
		for(int lamp=STOCK_MIN;lamp<STOCK_MAX;lamp++)
		{
			RouletteStockLampInfo[lamp].OnObject.SetActive(RouletteStockLampInfo[lamp].DisplayState);
			RouletteStockLampInfo[lamp].OffObject.SetActive(!(RouletteStockLampInfo[lamp].DisplayState));
		}
	}
	//==============================================================================//
	//	Setter、Getter																//
	//==============================================================================//
	/* ストック要求	*/
	public void SetRouletteStockRequest()
	{
		RouletteStockRequest = true;
	}
	public void ClearRouletteStockRequest()
	{
		RouletteStockRequest = false;
	}
}
