using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSoundController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "tag_Pole")//Poleと衝突→鳴らす、削除しない
		{
			SoundManager soundManagerInstance = GameObject.Find("AudioPlayer").GetComponent<SoundManager>();
			soundManagerInstance.PlaySoundCoinHit();//再生要求
		}
		else if(collision.gameObject.tag == "tag_Coin")//Coinと衝突→鳴らす、削除する
		{
			SoundManager soundManagerInstance = GameObject.Find("AudioPlayer").GetComponent<SoundManager>();
			soundManagerInstance.PlaySoundCoinHit();//再生要求
			Destroy(GetComponent<CoinSoundController>());//このスクリプトを削除
		}
	}
}
