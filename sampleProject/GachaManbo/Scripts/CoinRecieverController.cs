using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinRecieverController : MonoBehaviour
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
		Destroy(collision.gameObject);//衝突したコインオブジェクトを削除
		//CREDIT追加要求
	}
}
