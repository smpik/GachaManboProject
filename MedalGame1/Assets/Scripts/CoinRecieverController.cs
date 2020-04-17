using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRecieverController : MonoBehaviour
{
	private CreditManager CreditManagerInsatance;
    // Start is called before the first frame update
    void Start()
    {
		CreditManagerInsatance = GameObject.Find("EnterCoinGate").GetComponent<CreditManager>();
    }

	public void OnCollisionEnter(Collision collision)
	{
		Destroy(collision.gameObject);//衝突したコインオブジェクトを削除
		CreditManagerInsatance.AddCredit();//CREDIT追加要求
	}
}
