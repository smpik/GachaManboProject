using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCoinGateSensorController : MonoBehaviour
{
	private Vector3 PosStartRay;
	private Vector3 PosEndRay;
	private const float LengthRay = 0.05f;//Rayを飛ばす長さ

	// Start is called before the first frame update
	void Start()
	{
		PosStartRay = GameObject.Find("EnterCoinGateSensor").transform.position;
		PosEndRay = new Vector3(0,0,1);//z軸方向を向けばなんでもいい
	}

	public bool IsCoinNothing()
	{
		bool ret = true;//true=コインなし=コイン投入OK

		/* Sensorの中心からz軸方向にLengthRayだけRayを飛ばし、衝突したオブジェクトの情報をhittedObjInfoに格納する	*/
		RaycastHit hittedObjInfo;
		if (Physics.Raycast(PosStartRay, PosEndRay, out hittedObjInfo, LengthRay))//Rayはオブジェクトの中心から出る
		{
			if (hittedObjInfo.collider.gameObject.tag == "tag_Coin")
			{
				ret = false;/* コインを検知したならコイン投入NG	*/
			}
		}

		return ret;
	}
}
