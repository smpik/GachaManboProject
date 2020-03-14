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

	// Start is called before the first frame update
	void Start()
    {
		RouletteController = GameObject.Find("RouletteMasu").GetComponent<RouletteController>();
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
			if (RouletteController.GetPermitRouletteRequest() == true)
			{   //ルーレット要求が許可されているなら
				RouletteController.SetRouletteRequest();//ルーレット要求
				decrementRouletteStock();//要求したのでストックを減らす
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
