using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
	private GameObject ExcludeCanvas;//除外マスで止まった時用のキャンバス
	private GameObject AdChoiceCanvas;//ジャックポットチャンス用広告動画再生選択時のキャンバス
	private GameObject MoreCreditCanvas;//コインゲットキャンバス(クレジットが0になった時用)
	private GameObject AdChoiceCanvas2;//コインゲット用リワード再生選択時のキャンバス

	public AdMobReward2 ad2;	// コインゲット用リワードのインスタンス。処理負荷削減のためインスペクタから設定

	// Start is called before the first frame update
	void Start()
    {
		ExcludeCanvas = GameObject.Find("ExcludeCanvas");
		AdChoiceCanvas = GameObject.Find("AdChoiceCanvas");
		MoreCreditCanvas = GameObject.Find("MoreCreditCanvas");
		AdChoiceCanvas2 = GameObject.Find("AdChoiceCanvas2");
		SetActiveExcludeCanvas(false);
		SetActiveAdChoiceCanvas(false);
		SetActiveMoreCreditCanvas(false);
		SetActiveAdChoiceCanvas2(false);
	}

	/*
	 * 外れマス除外用リワードの表示非表示
	 */
	public void SetActiveExcludeCanvas(bool displayRequest)
	{
		ExcludeCanvas.SetActive(displayRequest);
	}
	public void SetActiveAdChoiceCanvas(bool displayRequest)
	{
		AdChoiceCanvas.SetActive(displayRequest);
	}
	public bool GetActiveStateExcludeCanvas()
	{
		bool ret = false;

		if(GameObject.Find("ExcludeCanvas"))
		{
			ret = true;
		}

		return ret;
	}

	/**
	 * コインゲットキャンバスの表示非表示
	 */
	public void SetActiveMoreCreditCanvas(bool displayRequest)
	{
		if (displayRequest)
		{
			/* 表示するときは、広告の読み込みができているときだけ	*/
			if (ad2.IsRewardReady())
			{
				MoreCreditCanvas.SetActive(displayRequest);
			}
		}
		else// 表示しないときは、そのままでいい
		{
			MoreCreditCanvas.SetActive(displayRequest);
		}
		
	}

	/**
	 * コインゲット用リワード再生選択用キャンバスの表示非表示
	 */
	public void SetActiveAdChoiceCanvas2(bool displayRequest)
	{
		AdChoiceCanvas2.SetActive(displayRequest);
	}
	
}
