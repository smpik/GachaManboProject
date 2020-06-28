using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideHoleController : MonoBehaviour
{
	public void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "tag_Coin")
		{
			Destroy(collision.gameObject);//衝突したコインオブジェクトを削除
			Debug.Log("横穴機能してる");
		}
	}
}