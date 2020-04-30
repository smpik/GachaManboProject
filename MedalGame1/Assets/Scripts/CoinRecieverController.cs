using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRecieverController : MonoBehaviour
{
	private CreditManager CreditManagerInsatance;
	private SoundManager SoundManagerInstance;
    // Start is called before the first frame update
    void Start()
    {
		CreditManagerInsatance = GameObject.Find("EnterCoinGate").GetComponent<CreditManager>();
		SoundManagerInstance = GameObject.Find("AudioPlayer").GetComponent<SoundManager>();
    }

	public void OnCollisionEnter(Collision collision)
	{
		Destroy(collision.gameObject);//衝突したコインオブジェクトを削除
		CreditManagerInsatance.AddCredit();//CREDIT追加要求
		SoundManagerInstance.PlaySoundCoinFall();//コイン落下時のSE再生
	}
}
