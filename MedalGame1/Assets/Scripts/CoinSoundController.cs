using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSoundController : MonoBehaviour
{
	private SoundManager SoundManagerInstance;
    // Start is called before the first frame update
    void Start()
    {
		SoundManagerInstance = GameObject.Find("AudioPlayer").GetComponent<SoundManager>();
    }

	public void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "tag_Pole")//Poleと衝突→鳴らす、削除しない
		{
			SoundManagerInstance.PlaySoundCoinHit();//再生要求
		}
		else if(collision.gameObject.tag == "tag_Coin")//Coinと衝突→鳴らす、削除する
		{
			SoundManagerInstance.PlaySoundCoinHit();//再生要求
			Destroy(GetComponent<CoinSoundController>());//このスクリプトを削除
		}else if(collision.gameObject.tag == "tag_GuidanceStick")//GuidencaStickと衝突→鳴らさない、削除しない
		{
			//なにもしない
		}
	}
}
