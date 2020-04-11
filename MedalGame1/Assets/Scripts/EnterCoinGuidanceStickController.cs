using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterCoinGuidanceStickController : MonoBehaviour
{
	private GameObject Stick1;//入口誘導棒1
	private GameObject Stick2;//入口誘導棒2

	private bool Clockwise;//回転方向
	private float StickAngle;//内部で保持する誘導棒の角度(直接誘導棒の角度を取得して使うと0=360になったりして難しいから

	private const float ROTATION_SPEED = 1;//入口誘導棒の回転速度
	private const float LIMIT_ANGLE = 30;//誘導棒の角度上限

    // Start is called before the first frame update
    void Start()
    {
		Stick1 = GameObject.Find("PivotForStick1");
		Stick2 = GameObject.Find("PivotForStick2");

		Stick1.transform.localEulerAngles = new Vector3(0,0,0);///誘導棒の角度を初期化
		StickAngle = 0f;

		Clockwise = false;//起動初回時は反時計回り
	}

    // Update is called once per frame
    void Update()
    {
        if(StickAngle < -LIMIT_ANGLE)//時計回り上限に達していれば(左端?)
		{
			Clockwise = false;//回転方向を反時計回りに変更(右向き?)
		}
		if(StickAngle > LIMIT_ANGLE)//反時計回り上限に達していれば(右端?)
		{
			Clockwise = true;//回転方向を時計回りに変更(左向き)
		}

		if (Clockwise)//回転方向が時計回りなら
		{
			Stick1.transform.Rotate(0, 0, -ROTATION_SPEED);//誘導棒角度更新
			Stick2.transform.Rotate(0, 0, -ROTATION_SPEED);//Rotate(0,0,正)でCCWになるらしい
			StickAngle -= ROTATION_SPEED;//内部で保持している誘導棒の角度も更新
		}
		else//回転方向が反時計回りなら
		{
			Stick1.transform.Rotate(0, 0, ROTATION_SPEED);//誘導棒角度更新
			Stick2.transform.Rotate(0, 0, ROTATION_SPEED);//Rotate(0,0,正)でCCWになるらしい
			StickAngle += ROTATION_SPEED;//内部で保持している誘導棒の角度も更新
		}
	}
}
