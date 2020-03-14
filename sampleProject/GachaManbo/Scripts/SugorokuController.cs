using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SugorokuController : MonoBehaviour
{
	//==============================================================================//
	//	定義																			//
	//==============================================================================//
	//==================================================//
	/* const,enum定義									*/
	//==================================================//
	public enum ACTION_PATTERN//各アクションは終了したら通常に必ず戻す(順番とかを作ると汎用性が下がる)
	{
		DEFAULT = 0,    //通常
		MOVE,			//すすむ
		FLASH,			//点滅
		OUT_ACTION		//Outアクション
	}
	private enum MASU_MOVE_EVENT
	{
		NONE = 0,				//イベントなし
		MOVE_START,				//すすむアクション開始イベント
		MY_TURN_COMING,			//自分の番きたイベント
		MOVE_END				//すすむアクション終了イベント
	}
	private enum MASU_FLASH_EVENT
	{
		NONE = 0,//イベントなし
		FLASH_START,//点滅アクション開始イベント
		CHANGE_ON_OFF,//点滅切り替えイベント
		FLASH_END//点滅アクション終了イベント
	}
	private enum MASU_OUT_ACTION_EVENT
	{
		NONE=0,//イベントなし
		OUT_ACTION_START,//Outアクション開始イベント
		MY_TURN_COMING,//自分の番来たイベント
		OUT_ACTION_END//Outアクション終了イベント
	}
	private readonly string[] MASU_NAME = //各マスのGameObject名(これらに+"ON"とかして使う。配列宣言時にconstで固定定数使えないらしい)
		{	"00_Start",
			"01_Coin10EventStock+1",
			"02_Out",
			"03_Coin10EventStock+1",
			"04_Coin10EventStock+1",
			"05_Out",
			"06_Coin20EventStock+1",
			"07_Coin50EventStock+1",
			"08_Chance",
			"09_Out",
			"10_Out",
			"11_Goal"
		};
	private const bool OFF = false; //消灯
	private const bool ON = true;   //点灯
	private const int NUM_MASU_FIRST = 0;       //最初のマスの番号
	private const int NUM_MASU_MAX = 12;         //マスの数
	
	//==================================================//
	/* 構造体定義											*/
	//==================================================//
	private struct EventStruct//イベントを取りまとめた構造体。イベント増えたらここに追加
	{
		public MASU_MOVE_EVENT MoveEvent;
		public MASU_FLASH_EVENT FlashEvent;
		public MASU_OUT_ACTION_EVENT OutActionEvent;
	}
	private struct MasuInfoStruct
	{
		public GameObject NameOnObject;     //ONのほうのオブジェクト
		public GameObject NameOffObject;    //OFFのほうのオブジェクト
		public ACTION_PATTERN ActionState;  //状態1。大きい状態(すごろくマスのふるまいに何が要求されているか)
		public bool DisplayState;           //状態2。小さい状態(点灯/消灯。DisplayState=true ⇒ NameON=true、NameOFF=false)
		public EventStruct Event;           //イベント
	}
	private struct RequestInfoStruct    //すごろくマスの動作に対する要求
	{
		public bool Move;    //すすむアクション要求
		public bool Flash;   //点滅アクション要求
		public bool OutAction;       //Outアクション要求
	}
	//==================================================//
	/* 変数実態定義										*/
	//==================================================//
	private MasuInfoStruct[] MasuInfo;
	private RequestInfoStruct Request;

	private int SugorokuNowPosition;//すごろく現在地
	private int SugorokuMoveCounter;//すすめるカウンタ

	//==============================================================================//
	//	初期化処理																	//
	//==============================================================================//
	void Start()
    {
		generateStructInstance();       //各構造体のインスタンス生成

		initMasuInfo();                 //各マスの情報初期化
		initRequest();                  //RequestInfoの初期化
	}
	/* 各構造体のインスタンス生成	*/
	private void generateStructInstance()
	{
		MasuInfo = new MasuInfoStruct[NUM_MASU_MAX];
		Request = new RequestInfoStruct();
	}
	/* 各マスの情報初期化	*/
	private void initMasuInfo()
	{
		for(int masu=NUM_MASU_FIRST; masu<NUM_MASU_MAX; masu++)
		{
			initMasuInfoNameOnObject(masu);
			initMasuInfoNameOffObject(masu);
			initMasuInfoActionState(masu);
			initMasuInfoDisplayState(masu);
			initMasuInfoEvent(masu);
		}
	}
	private void initMasuInfoNameOnObject(int masu)
	{
		MasuInfo[masu].NameOnObject = GameObject.Find(MASU_NAME[masu] + "ON");
	}
	private void initMasuInfoNameOffObject(int masu)
	{
		MasuInfo[masu].NameOffObject = GameObject.Find(MASU_NAME[masu] + "OFF");
	}
	private void initMasuInfoActionState(int masu)
	{
		MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;
	}
	private void initMasuInfoDisplayState(int masu)
	{
		MasuInfo[masu].DisplayState = OFF;
	}
	private void initMasuInfoEvent(int masu)
	{
		MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.NONE;
		MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.NONE;
		MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.NONE;
	}
	/* 要求初期化		*/
	private void initRequest()
	{
		Request.Move = false;
		Request.Flash = false;
		Request.OutAction = false;
	}

	//==============================================================================//
	//	Update処理																	//
	//==============================================================================//
	void Update()
    {
        //要求があれば
		//状態遷移のInputデータの確定処理
		//イベント発行
		//状態遷移
		//出力処理
		//各状態終了後の処理(次状態の設定など)
    }

	//==================================================//
	/* 要求存在確認										*/
	//==================================================//
	private bool isRequestComing()
	{
		bool ret = false;

		return ret;
	}
	//==================================================//
	/* 状態遷移のInputデータの確定処理						*/
	//==================================================//
	private void decideInput()
	{
		decideInputByMove();
		decideInputByFlash();
		decideInputByOutAction();
	}
	/* すすむアクションのInputの確定処理	*/
	private void decideInputByMove()
	{
		moveSugoroku();//すごろく現在地の更新
	}
	private void moveSugoroku()
	{
		if(SugorokuMoveCounter>0)//すすめるカウンタ0より上なら
		{
			if (SugorokuNowPosition<NUM_MASU_MAX)//まだ進む先のマスがあるなら
			{
				SugorokuNowPosition++;//すごろく現在地の更新
				SugorokuMoveCounter--;//すすめるカウンタの更新
			}
		}
	}
	/* 点滅アクションのInputの確定処理	*/
	private void decideInputByFlash()
	{

	}
	private void countTimerForFlash()
	{

	}
	/* OutアクションのInputの確定処理	*/
	private void decideInputByOutAction()
	{

	}
	private void countTimerForOutAction()
	{

	}
	private void updateOutTargetMasu()
	{

	}
	//==================================================//
	/*	イベント発行										*/
	//==================================================//
	private void fireEvent()
	{

	}
	/* すすむアクションのInputの確定処理	*/
	private void fireEventByMove()
	{

	}
	/* 点滅アクションのInputの確定処理	*/
	private void fireEventByFlash()
	{

	}
	/* OutアクションのInputの確定処理	*/
	private void fireEventByOutAction()
	{

	}
	//==================================================//
	/*	状態遷移											*/
	//==================================================//
	private void transitState()
	{

	}
	/* すすむアクションの状態遷移	*/
	private void transitStateByMove()
	{

	}
	/* 点滅アクションの状態遷移	*/
	private void transitStateByFlash()
	{

	}
	/* Outアクションの状態遷移	*/
	private void transitStateByOutAction()
	{

	}
	//==================================================//
	/*	出力処理											*/
	//==================================================//
	private void setActiveByMasuDisplayState()
	{

	}
	//==================================================//
	/*	各アクション終了後のクラス内部からの次アクションの設定*/
	//==================================================//
	private void prepareNextActionByInside()
	{

	}
	/* すすむアクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterMove()
	{

	}
	/* 点滅アクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterFlash()
	{

	}
	private void judgeStopMasu()
	{	/* 表示状態がONになってる最後のマス(=インデックス最大のマス=止まったマス)がOUTマスだったらOutAction要求	*/

	}
	/* Outアクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterOutAction()
	{

	}
	//==============================================================================//
	//	setter/getter																//
	//==============================================================================//
	//==================================================//
	/*	要求												*/
	//==================================================//
	public void JudgeSugorokuStart(string rouletteResult)
	{	/* ルーレット結果がすすむマスだったらすごろく要求する	*/

	}
	private void setMoveRequest()
	{

	}
	private void setFlashRequest()
	{

	}
	private void setOutActionRequest()
	{

	}
	private void clearMoveRequest()
	{

	}
	private void clearFlashRequest()
	{

	}
	private void clearOutActionRequest()
	{

	}
}
