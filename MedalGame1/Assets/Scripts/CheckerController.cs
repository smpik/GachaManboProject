using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckerController : MonoBehaviour
{
	public GameObject Checker;//チェッカー。処理負荷削減のため、インスペクタから設定する


	private bool Clockwise;//回転方向
	private float CheckerAngle;//内部で保持するチェッカーの角度(直接角度を取得して使うと0=360になったりして難しいから

	private const float ROTATION_SPEED = 1f;//チェッカーの回転速度
	private const float LIMIT_ANGLE = 30;//チェッカーの回転角度上限

	// Start is called before the first frame update
	void Start()
    {
		//Checker = GameObject.Find("Checker");

		Checker.transform.localEulerAngles = new Vector3(0, 0, 0);///チェッカーの角度を初期化
		CheckerAngle = 0f;

		Clockwise = false;//起動初回時は反時計回り
	}

    // Update is called once per frame
    void Update()
    {
		if (CheckerAngle < -LIMIT_ANGLE)//時計回り上限に達していれば(左端?)
		{
			Clockwise = false;//回転方向を反時計回りに変更(右向き?)
		}
		if (CheckerAngle > LIMIT_ANGLE)//反時計回り上限に達していれば(右端?)
		{
			Clockwise = true;//回転方向を時計回りに変更(左向き)
		}

		if (Clockwise)//回転方向が時計回りなら
		{
			Checker.transform.Rotate(0, 0, -ROTATION_SPEED);//チェッカー角度更新(Rotate(0,0,正)でCCWになるらしい)
			CheckerAngle -= ROTATION_SPEED;//内部で保持しているチェッカーの角度も更新
		}
		else//回転方向が反時計回りなら
		{
			Checker.transform.Rotate(0, 0, ROTATION_SPEED);//チェッカー角度更新(Rotate(0,0,正)でCCWになるらしい)
			CheckerAngle += ROTATION_SPEED;//内部で保持しているチェッカーの角度も更新
		}
	}
}
