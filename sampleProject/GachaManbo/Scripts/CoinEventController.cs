using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinEventController : MonoBehaviour
{
	private enum COIN_PAYOUT_STATE
	{
		DEFAULT = 0,		//通常状態(何もしない状態)
		PAYOUT				//放出中状態
	}
	private enum COIN_PAYOUT_EVENT
	{
		NONE = 0,			//イベントなし
		PAYOUT_START,		//放出開始イベント
		COIN_GENERATE,		//コイン生成イベント
		PAYOUT_END			//すすむアクション終了イベント
	}
	private const int TIME_COIN_GENERATE_WAIT = 5;//コイン生成待ち時間
	private const int NUM_DEFAULT_JACKPOT = 200;//初期ジャックポット枚数

	private EnterCoin EnterCoin;
	private SugorokuController SugorokuController;
	private COIN_PAYOUT_STATE CoinPayoutState;//コイン放出状態
	private COIN_PAYOUT_EVENT CoinPayoutEvent;//コイン放出イベント(状態遷移のイベント)
	private bool CoinPayoutRequest;//コイン放出要求
	private bool CoinEventIsReadyOk;//コインイベント準備OKフラグ

	private int CoinGenerateWaitTimer;//コイン生成待ち時間を保持するタイマー
	private int RestCoins;//放出コインの残り枚数
	private int NumCoinJackpot;//ジャックポット枚数

	//==============================================================================//
	//	初期化処理																	//
	//==============================================================================//
	void Start()
    {
		EnterCoin = GameObject.Find("Main Camera").GetComponent<EnterCoin>();
		SugorokuController = GameObject.Find("SugorokuMasu").GetComponent<SugorokuController>();

		CoinEventIsReadyOk = true;
		NumCoinJackpot = NUM_DEFAULT_JACKPOT;
    }

	//==============================================================================//
	//	Update処理																	//
	//==============================================================================//
	void Update()
    {
        if(CoinPayoutRequest == true)
		{
			decideInput();
			fireEvent();
			transitState();
			readyNextRequest();
		}
    }
	//==================================================//
	/* 状態遷移のInputデータ確定処理						*/
	//==================================================//
	private void decideInput()
	{
		countTimerForCoinGenerateWait();
	}
	private void countTimerForCoinGenerateWait()
	{//コイン生成待ちタイマーの更新
		if(CoinGenerateWaitTimer>0)
		{
			CoinGenerateWaitTimer--;

			if (CoinGenerateWaitTimer == 0)
			{
				countDownRestCoins();//コインを生成するタイミングで残り枚数のカウントダウンを行いたいのでここで判定。
			}
		}
		else//コイン生成待ち時間が終わったら
		{
			setCoinGenerateWaitTimer();//タイマ再セット
		}

	}
	private void countDownRestCoins()
	{
		if (RestCoins > 0)
		{
			RestCoins--;
		}
		else//残り枚数なしなら
		{
			CoinPayoutRequest = false;//要求をクリア
		}
	}
	private void setCoinGenerateWaitTimer()
	{
		CoinGenerateWaitTimer = TIME_COIN_GENERATE_WAIT;
	}
	//==================================================//
	/* 状態遷移の	イベント発行処理							*/
	//==================================================//
	private void fireEvent()
	{
		CoinPayoutEvent = COIN_PAYOUT_EVENT.NONE;//イベントがあれば上書きされる

		if ((CoinPayoutState == COIN_PAYOUT_STATE.DEFAULT) && (CoinPayoutRequest == true))
		{//通常状態、要求あり
			CoinPayoutEvent = COIN_PAYOUT_EVENT.PAYOUT_START;//コイン放出開始イベント
		}
		if((CoinPayoutState == COIN_PAYOUT_STATE.PAYOUT) && (CoinGenerateWaitTimer==0))
		{//放出中状態、コイン生成待ち時間終了
			CoinPayoutEvent = COIN_PAYOUT_EVENT.COIN_GENERATE;//コイン生成イベント
		}
		if((CoinPayoutState == COIN_PAYOUT_STATE.PAYOUT) && (CoinPayoutRequest == false))
		{//放出中状態、要求なし
			CoinPayoutEvent = COIN_PAYOUT_EVENT.PAYOUT_END;//コイン放出終了イベント
		}
	}
	//==================================================//
	/* 状態遷移											*/
	//==================================================//
	private void transitState()
	{
		switch(CoinPayoutEvent)
		{
			case COIN_PAYOUT_EVENT.PAYOUT_START:
				CoinPayoutState = COIN_PAYOUT_STATE.PAYOUT;//放出中状態へ遷移
														   //ほかはなにもしない
				break;
			case COIN_PAYOUT_EVENT.COIN_GENERATE:
				//状態遷移はなし
				EnterCoin.TapButtonCoinEnter();//コイン生成
				break;
			case COIN_PAYOUT_EVENT.PAYOUT_END:
				CoinPayoutState = COIN_PAYOUT_STATE.DEFAULT;//通常状態へ遷移
															//ほかはなにもしない
				break;
			default:
				break;
		}
	}
	//==================================================//
	/* 次の要求が来た時用の準備							*/
	//==================================================//
	private void readyNextRequest()
	{
		if((CoinEventIsReadyOk == false) && (CoinPayoutRequest == false))
		{//コイン放出が終わったら
			CoinEventIsReadyOk = true;

			/* コイン放出がジャックポットによるものだった時の処理	*/
			if(SugorokuController.GetFlagJackpotIsFinished() == false)//ジャックポット終了してない
			{
				SugorokuController.SetFlagJackpotIsFinished();//ジャックポット終了を知らせる
			}
		}
	}
	//==============================================================================//
	//	Setter、Getter																//
	//==============================================================================//
	public void SetCoinPayoutRequest(int restCoins)
	{
		CoinPayoutRequest = true;
		CoinEventIsReadyOk = false;
		RestCoins = restCoins;
	}
	public bool GetCoinEventIsReadyOk()
	{
		return CoinEventIsReadyOk;
	}
	public void JackpotRequest()
	{
		SetCoinPayoutRequest(NumCoinJackpot);//コイン放出(ジャックポット)要求
		SugorokuController.ClearFlagJackpotIsFinished();//ジャックポット終了フラグをクリア
	}
}
