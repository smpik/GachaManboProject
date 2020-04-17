using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;//これを入れないとEventArgsあたりにエラーが出る
using GoogleMobileAds.Api;//これは必須

public class AdMobReward : MonoBehaviour
{
	/********************************************************************************/
	/* 内部変数																		*/
	/********************************************************************************/
	private const string adUnitId = "ca-app-pub-3940256099942544/5224354917";//テスト用id:ca-app-pub-3940256099942544/5224354917

	private const int NUM_EXCLUDE_MAX = 5;//リワード用、除外マスの最大数
	/********************************************************************************/
	/* 内部変数																		*/
	/********************************************************************************/
	private RewardedAd rewardedAd;//リワードを読み込むためのインスタンスを生成用
	private RouletteController RouletteControllerInstance;
	private UIController UIControllerInstance;

	// Start is called before the first frame update
	void Start()
	{
		this.rewardedAd = new RewardedAd(adUnitId);//インスタンス生成

		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the rewarded ad with the request.
		this.rewardedAd.LoadAd(request);

		// Called when an ad request has successfully loaded.
		this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
		// Called when an ad request failed to load.
		this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
		// Called when an ad is shown.
		this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
		// Called when an ad request failed to show.
		this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
		// Called when the user should be rewarded for interacting with the ad.
		this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
		// Called when the ad is closed.
		this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

		RouletteControllerInstance = GameObject.Find("RouletteMasu").GetComponent<RouletteController>();
		UIControllerInstance = GameObject.Find("Main Camera").GetComponent<UIController>();
	}

	/********************************************************************************/
	/* 関数名	: 動画再生															*/
	/* 備考		: 動画を見るボタンタップで呼ばれる。									*/
	/********************************************************************************/
	public void ShowAdMovie()
	{
		if (this.rewardedAd.IsLoaded())//広告の読み込みが完了していれば
		{
			this.rewardedAd.Show();//広告を表示
			UIControllerInstance.SetActiveAdChoiceCanvas(false);//広告動画再生選択用キャンバスを閉じる
		}
	}

	/********************************************************************************/
	/* AdLoaded (広告の読み込みが完了すると呼ばれる。)								*/
	/********************************************************************************/
	public void HandleRewardedAdLoaded(object sender, EventArgs args)
	{
		Debug.Log("HandleRewardedAdLoaded event received");
	}

	/********************************************************************************/
	/* FailedToLoad (広告の読み込みが失敗すると呼ばれる。)							*/
	/********************************************************************************/
	public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
	{
		Debug.Log("HandleRewardedAdFailedToLoad event received with message: " + args.Message);
	}

	/********************************************************************************/
	/* AdOpening (広告が画面いっぱいに表示されると呼ばれる。)						*/
	/* 必要に応じて、アプリの音声出力やゲームループを一時停止することができる。		*/
	/********************************************************************************/
	public void HandleRewardedAdOpening(object sender, EventArgs args)
	{
		Debug.Log("HandleRewardedAdOpening event received");
		//一時停止
	}

	/********************************************************************************/
	/* FailedToShow (広告の表示に失敗すると呼ばれる。)								*/
	/********************************************************************************/
	public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
	{
		Debug.Log("HandleRewardedAdFailedToShow event received with message: " + args.Message);
		//一時停止解除
	}

	/********************************************************************************/
	/* AdClosed (ユーザが「閉じる」アイコンまたは「戻る」ボタンをタップして、		*/
	/* 動画リワード広告を閉じると呼ばれる。											*/
	/* ゲームの一時停止を再開するのに適したタイミング。								*/
	/********************************************************************************/
	public void HandleRewardedAdClosed(object sender, EventArgs args)
	{
		Debug.Log("HandleRewardedAdClosed event received");
		//一時停止解除
		UIControllerInstance.SetActiveExcludeCanvas(false);//除外用キャンバスを閉じる(この関数コール時(動画途中で閉じた後)は再度動画を再生できないため)
	}

	/********************************************************************************/
	/* EarnedReward (動画を視聴したユーザに報酬を付与するときに呼ばれる。)			*/
	/********************************************************************************/
	public void HandleUserEarnedReward(object sender, Reward args)
	{
		string type = args.Type;
		double amount = args.Amount;
		Debug.Log("HandleRewardedAdRewarded event received for " + amount.ToString() + " " + type);

		UIControllerInstance.SetActiveExcludeCanvas(false);//除外用キャンバスを閉じる(広告を再生を拒否した後でも再度広告再生選択をできるようにリワード時までは閉じない)
		giveReward();//リワードを与える処理をコール
	}
	private void giveReward()
	{
		for(int i=0; i<NUM_EXCLUDE_MAX; i++)//NUM_EXCLUDE_MAX分、除外する。
		{//ExcludeMasuCounterが1増えるたびに、、、という処理の仕方なので、この(直接NUM_EXCLUDE_MAXを代入しない)書き方にする
			RouletteControllerInstance.IncrementExcludedMasuCounter();
		}
	}

	/********************************************************************************/
	/* リワード準備判定																*/
	/********************************************************************************/
	public bool IsRewardReady()
	{
		bool ret = false;

		if(this.rewardedAd.IsLoaded()==true)
		{
			ret = true;
		}

		return ret;
	}
}