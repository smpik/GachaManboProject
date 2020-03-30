using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteStockManager : MonoBehaviour
{
	private const uint STOCK_MAX = 5;
	private const uint STOCK_MIN = 0;

	private bool RouletteStockRequest;
	private uint RouletteStock;

	private RouletteController RouletteController;
	private SugorokuController SugorokuController;
	private CoinEventController CoinEventController;

	// Start is called before the first frame update
	void Start()
    {
		RouletteController = GameObject.Find("RouletteMasu").GetComponent<RouletteController>();
		SugorokuController = GameObject.Find("SugorokuMasu").GetComponent<SugorokuController>();
		CoinEventController = GameObject.Find("EnterCoinGate").GetComponent<CoinEventController>();
	}

    // Update is called once per frame
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
		}
	}
	/* ストックをカウント	*/
	private void countRouletteStock()
	{
		if (RouletteStock < STOCK_MAX)
		{
			RouletteStock++;
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
