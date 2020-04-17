using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdMobBanner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		//アプリID、テスト用はca-app-pub-3940256099942544~3347511713
		string appId = "ca-app-pub-3940256099942544~3347511713";

		MobileAds.Initialize(appId);

		RequestBanner();
	}

	private void RequestBanner()
	{

		// 広告ユニットID テスト用は:ca-app-pub-3940256099942544/6300978111
		string adUnitId = "ca-app-pub-3940256099942544/6300978111";

		// Create a 320x50 banner at the top of the screen.
		BannerView bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);

		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();

		// Load the banner with the request.
		bannerView.LoadAd(request);

		// Create a 320x50 banner at the top of the screen.
		//bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
	}
}
