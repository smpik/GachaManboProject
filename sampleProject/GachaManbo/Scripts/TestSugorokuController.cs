using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;//おまじない
using System.Reflection;//private関数をコールするためのおまじないのおまじない

public class TestSugorokuController : MonoBehaviour
{
	/* テストケース		//INPUT								//期待値								*/
	[TestCase(0,0,0,0)]	//すすめるカウンタ=0、すごろく現在地=0、すすめるカウンタ=0、すごろく現在地=0
	[TestCase(1,0,0,1)]
	[TestCase(1,12,1,12)]
	public void TestMoveSugoroku(int input1,int input2,int expected1,int expected2)
	{   //input1 = expected1 = すすめるカウンタ、input2 = expected2 = すごろく現在地

		/* privateなメソッドをコールするための準備	*/
		System.Type typeOfTargetClass = typeof(SugorokuController);//コールしたメソッドをもつクラスの型を取得
		object instanceOfTargetType = System.Activator.CreateInstance(typeOfTargetClass);
		MethodInfo targetMethodInfo = typeOfTargetClass.GetMethod("moveSugoroku", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//コールしたいメソッドの属性を取得

		/* inputを設定するための準備	*/
		FieldInfo targetInput1Info = typeOfTargetClass.GetField("SugorokuMoveCounter", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//input1の準備
		FieldInfo targetInput2Info = typeOfTargetClass.GetField("SugorokuNowPosition", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//input2の準備

		/* privateな変数をテストケースの値に設定	*/
		targetInput1Info.SetValue(instanceOfTargetType, input1);//input1を設定
		targetInput2Info.SetValue(instanceOfTargetType, input2);//input2を設定

		/* テスト対象のメソッドをコール */
		targetMethodInfo.Invoke(instanceOfTargetType, null);
		/* テスト結果を取得	*/
		int result1 = (int)targetInput1Info.GetValue(instanceOfTargetType);//result1の取得
		int result2 = (int)targetInput2Info.GetValue(instanceOfTargetType);//result2の取得

		/* テスト結果の確認	*/
		Assert.AreEqual(expected1, result1);//すすめるカウンタの確認
		Assert.AreEqual(expected2, result2);//すごろく現在地
	}


	//==============================================================================================================//
	//	TestUpdateOutTargetMasu																						//
	//	input1		：OutTargetUpdateWaitTimer																		//
	//	input2		：OutTargetMasuId																				//
	//	input3		：Request.OutAction																				//
	//	expected1	：OutTargetMasuId																				//
	//	expected2	：FlagOutTargetUpdate																			//
	//	expected3	：Request.OutAction																				//
	//==============================================================================================================//
	[TestCase(0, 0, true, 0, false, false)]
	[TestCase(0, 1, true, 0, true, true)]
	[TestCase(1, 0, true, 0, false, true)]
	[TestCase(1, 1, true, 1, false, true)]
	public void TestUpdateOutTargetMasu(int input1,int input2,bool input3,int expected1,bool expected2,bool expected3)
	{
		/* privateなメソッドをコールするための準備	*/
		System.Type typeOfTargetClass = typeof(SugorokuController);//コールしたいメソッドをもつクラスの型を取得
		object instanceOfTargetType = System.Activator.CreateInstance(typeOfTargetClass);
		MethodInfo targetMethodInfo = typeOfTargetClass.GetMethod("updateOutTargetMasu", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//コールしたいメソッドの属性を取得

		/* inputを設定するための準備	*/
		FieldInfo targetInput1Info = typeOfTargetClass.GetField("OutTargetUpdateWaitTimer", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//input1の準備
		FieldInfo targetInput2Info = typeOfTargetClass.GetField("OutTargetMasuId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//input2の準備
		/* input3の準備 */
		System.Type typeOfRequestStruct = typeOfTargetClass.GetNestedType("RequestInfoStruct", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//Requestの型を取得
		object instanceOfRequestStruct = System.Activator.CreateInstance(typeOfRequestStruct);//取得した構造体の型から構造体のインスタンスを生成
		FieldInfo targetInput3Info = typeOfRequestStruct.GetField("OutAction", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//アクセスしたいメンバを取得

		/* resultを取得するための準備	*/
		//result1はOutTargetMasuIdであり、input2と同じなので、targetInput2Infoを使いまわすため省略
		FieldInfo targetResult2Info = typeOfTargetClass.GetField("FlagOutTargetUpdate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);//result2の準備
		//result3はRequest.OutActionであり、input3と同じなので、targetInput3Infoを使いまわすため省略

		/* privateな変数をテストケースの値に設定	*/
		targetInput1Info.SetValue(instanceOfTargetType, input1);//input1を設定
		targetInput2Info.SetValue(instanceOfTargetType, input2);//input2を設定
		targetInput3Info.SetValue(instanceOfRequestStruct, input3);//input3を設定

		/* テスト対象のメソッドをコール */
		targetMethodInfo.Invoke(instanceOfTargetType, null);

		/* テスト結果を取得	*/
		int result1 = (int)targetInput2Info.GetValue(instanceOfTargetType);//result1の取得
		bool result2 = (bool)targetResult2Info.GetValue(instanceOfTargetType);//result2の取得
		bool result3 = (bool)targetInput3Info.GetValue(instanceOfRequestStruct);//result3の取得

		/* テスト結果の確認	*/
		Assert.AreEqual(expected1, result1);//OutTargetMasuIdの確認
		Assert.AreEqual(expected2, result2);//FlagOutTargetUpdateの確認
		Assert.AreEqual(expected3, result3);//Request.OutActionの確認
	}
}
