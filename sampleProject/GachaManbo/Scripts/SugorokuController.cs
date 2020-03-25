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
		DECIDED_ME,				//Me(自分)で確定イベント
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
	private const int NUM_FLASH_MAX = 4;//点滅切り替え回数(点滅アクション後は店頭で固定するため、切り替え回数は偶数にしとかないと瞬灯しちゃう)
	private const int TIME_WAIT_OUT_TARGET = 40;//OUT対象マスを更新するまでの待ち時間
	private const int TIME_FLASH_CHANGE = 30;//点滅を切り替える(ON⇔OFF)までの待ち時間
	
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
	private int FlashTimer;//点滅時間を保持するタイマー
	private int OutTargetUpdateWaitTimer;//OUT対象マスの更新までの待ち時間を保持するタイマー
	private int OutTargetMasuId;//OUT対象マスを指定するためのID
	private bool FlagOutTargetUpdate;//OUT対象マスの更新があったことを示すためのフラグ
	private int FlashCounter;//点滅切り替えをあと何回するかを保持するカウンタ

	private bool FlagMoveIsFinished;//すすむアクション終了を示すフラグ(decideInputでセットし、prepareNextActionByInsideにてクリアする)
	private bool FlagFlashIsFinished;
	private bool FlagOutIsFinished;

	//==============================================================================//
	//	初期化処理																	//
	//==============================================================================//
	void Start()
    {
		generateStructInstance();       //各構造体のインスタンス生成

		initMasuInfo();                 //各マスの情報初期化
		initRequest();                  //RequestInfoの初期化

		SugorokuNowPosition = NUM_MASU_FIRST;
		FlagMoveIsFinished = false;
		FlagFlashIsFinished = false;
		FlagOutIsFinished = false;

		MasuInfo[NUM_MASU_FIRST].DisplayState = ON;		//Startマスは最初から点灯
		setActiveByMasuDisplayState(NUM_MASU_FIRST);	//Startマスは最初から点灯
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
        if(isRequestComing() == true)//要求があれば
		{
			decideInput();//状態遷移のInputデータの確定処理
			for(int masu=NUM_MASU_FIRST; masu<NUM_MASU_MAX; masu++)
			{
				fireEvent(masu);//イベント発行
				transitState(masu);//状態遷移
				setActiveByMasuDisplayState(masu);//出力処理
			}
			prepareNextActionByInside();//各状態終了後の処理(次状態の設定など)
		}
	}
	//==================================================//
	/* 要求存在確認										*/
	//==================================================//
	private bool isRequestComing()
	{
		bool ret = false;

		if ((Request.Move == true)
			|| (Request.Flash == true)
			|| (Request.OutAction == true))
		{
			ret = true;
		}

		return ret;
	}
	//==================================================//
	/* 状態遷移のInputデータの確定処理						*/
	//==================================================//
	private void decideInput()
	{
		if(Request.Move==true)
		{
			decideInputByMove();
		}
		if(Request.Flash==true)
		{
			decideInputByFlash();
		}
		if(Request.OutAction == true)
		{
			decideInputByOutAction();
		}
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
		else
		{
			clearMoveRequest();
			FlagMoveIsFinished = true;//すすむアクション終了フラグセット
		}
	}
	/* 点滅アクションのInputの確定処理	*/
	private void decideInputByFlash()
	{
		if (FlashCounter > 0)//点滅切り替え回数がまだ残ってる
		{
			countTimerForFlash();//点滅タイマの更新
		}
		else//点滅切り替え回数なし
		{
			clearFlashRequest();//点滅要求をクリア
			FlagFlashIsFinished = true;//点滅アクション終了フラグをセット
		}
	}
	private void countTimerForFlash()
	{
		if (FlashTimer > 0)//点滅切り替え待ち時間中
		{
			FlashTimer--;
		}
		else//点滅切り替え待ち時間終了
		{
			setFlashTimer();//点滅待ち時間の再セット
			FlashCounter--;//点滅カウンタを1減らす(本関数をコールするのはFlashCounter>0のときのみなのでガード不要)
		}
	}
	private void setFlashTimer()
	{
		FlashTimer = TIME_FLASH_CHANGE;
	}
	/* OutアクションのInputの確定処理	*/
	private void decideInputByOutAction()
	{
		countTimerForOutAction();//OUT対象マス更新待ちタイマの更新
		updateOutTargetMasu();//OUT対象マスの更新
	}
	private void countTimerForOutAction()
	{
		if (OutTargetUpdateWaitTimer>0)//OUT対象マス更新待ち時間ちゅう
		{
			OutTargetUpdateWaitTimer--;
		}
		else//OUT対象マス更新待ち時間終了
		{
			setOutTargetUpdateWaitTimer();//待ち時間を再セット
		}
	}
	private void updateOutTargetMasu()
	{
		FlagOutTargetUpdate = false;//OUT対象マス更新なし(更新があれば上書きする)

		if (OutTargetUpdateWaitTimer<=0)//更新待ち時間が終わったなら
		{
			SugorokuNowPosition--;//現在地更新(下のif文内に入れるとStartマスまで現在地を戻せないためここで処理する)

			if (SugorokuNowPosition > NUM_MASU_FIRST)//まだOUTにする必要のあるマスがあるなら(STARTマスはOUTにしない)
			{
				OutTargetMasuId--;//OUT対象マスを更新
				FlagOutTargetUpdate = true;//OUT対象マス更新あり(上書き)
			}
			else//もうOUTにするマスがないなら(STARTマスまで帰ってきているなら)
			{
				clearOutActionRequest();//OUTアクション要求クリア
				FlagOutIsFinished = true;//Outアクション終了フラグをセット
			}
		}
		else//まだ待ち時間が終わってないなら
		{
			//なにもしない
		}
	}
	private void setOutTargetUpdateWaitTimer()
	{
		OutTargetUpdateWaitTimer = TIME_WAIT_OUT_TARGET;//OUT対象マスを更新するまでの待ち時間をセット
	}
	//==================================================//
	/*	イベント発行										*/
	//==================================================//
	private void fireEvent(int masu)
	{
		fireEventByMove(masu);
		fireEventByFlash(masu);
		fireEventByOutAction(masu);
	}
	/* すすむアクションのイベント発行処理	*/
	private void fireEventByMove(int masu)
	{
		MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.NONE;//イベントがあれば上書きされる

		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.DEFAULT) && (Request.Move == true))
		{   //通常状態、すすめる要求あり
			MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.MOVE_START;//すすむ開始イベント発行
		}
		if ( ((MasuInfo[masu].ActionState == ACTION_PATTERN.MOVE) && (SugorokuNowPosition == masu) && (SugorokuMoveCounter != 0))
			|| ((MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.MOVE_START) && (SugorokuNowPosition==masu) && (SugorokuMoveCounter!=0)) )
		{   //すすめる状態、すごろく現在地が自分と同じ、すすめるカウンタはまだ残ってる
			//または、すすむ開始イベント発行時に、自分の番来たイベントも成立してた時
			//(すすむアクション初回、マスの更新あるのにすすむ開始イベントで、1周期使ってしまい、最初のマス更新だけ、自分の番来たイベントが発行されないため)
			MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.MY_TURN_COMING;//自分の番来たイベント発行
		}
		if ( ((MasuInfo[masu].ActionState == ACTION_PATTERN.MOVE) && (SugorokuNowPosition == masu) && (SugorokuMoveCounter == 0))
			|| ( (MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.MOVE_START) && (SugorokuNowPosition == masu) && (SugorokuMoveCounter == 0)) )
		{   //すすめる状態、すごろく現在地が自分と同じ、すすめるカウンタ終了
			//または、すすむ開始イベント発行時に、自分で確定イベントも成立してた時(1すすむのときとかに必要になる)
			MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.DECIDED_ME;//自分で確定イベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.MOVE) && (Request.Move == false))
		{   //すすめる状態、すすめる要求なし
			MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.MOVE_END;//すすむ終了イベント発行
		}
	}
	/* 点滅アクションのイベント発行処理	*/
	private void fireEventByFlash(int masu)
	{
		MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.NONE;//イベントがあれば上書きされる

		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.DEFAULT) && (Request.Flash == true))
		{   //通常状態、点滅要求あり
			MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.FLASH_START;//点滅開始イベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.FLASH) && (FlashTimer==0) && (FlashCounter>0) && (masu == SugorokuNowPosition))
		{   //点滅状態、点滅タイマタイムアップ、点滅カウンタまだ残ってる、止まったマスのみ
			MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.CHANGE_ON_OFF;//点滅切り替えイベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.FLASH) && (Request.Flash == false))
		{   //点滅状態、点滅要求なし
			MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.FLASH_END;//点滅終了イベント発行
		}
	}
	/* OutアクションのInputの確定処理	*/
	private void fireEventByOutAction(int masu)
	{
		MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.NONE;//イベントがあれば上書きされる

		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.DEFAULT) && (Request.OutAction == true) && (masu == SugorokuNowPosition))
		{   //通常状態、OUTアクション要求あり、OUTマスのみ
			MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.OUT_ACTION_START;//OUTアクション開始イベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.OUT_ACTION) && (FlagOutTargetUpdate == true) && (OutTargetMasuId == masu))
		{   //OUTアクション状態、OUT対象マス更新あり、OUT対象マスが自分
			MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.MY_TURN_COMING;//自分にOUT来たイベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.OUT_ACTION) && (Request.OutAction == false))
		{   //OUTアクション状態、OUTアクション要求なし
			MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.OUT_ACTION_END;//OUTアクション終了イベント発行
		}
	}
	//==================================================//
	/*	状態遷移											*/
	//==================================================//
	private void transitState(int masu)
	{   //マスのアクション状態に応じた状態遷移処理を実行
		switch(MasuInfo[masu].ActionState)
		{
			case ACTION_PATTERN.DEFAULT:
				transitStateByDefault(masu);
				break;
			case ACTION_PATTERN.MOVE:
				transitStateByMove(masu);
				break;
			case ACTION_PATTERN.FLASH:
				transitStateByFlash(masu);
				break;
			case ACTION_PATTERN.OUT_ACTION:
				transitStateByOutAction(masu);
				break;
			default:
				break;
		}
	}
	/* 通常アクションの状態遷移	*/
	private void transitStateByDefault(int masu)
	{
		if (MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.MOVE_START)//すすむ開始イベントなら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.MOVE;//すすむアクションに遷移
			//表示状態は変化なし
		}
		if(MasuInfo[masu].Event.FlashEvent == MASU_FLASH_EVENT.FLASH_START)//点滅開始なら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.FLASH;//点滅アクションに遷移
			//表示状態は変化なし
		}
		if(MasuInfo[masu].Event.OutActionEvent == MASU_OUT_ACTION_EVENT.OUT_ACTION_START)//OUT開始なら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.OUT_ACTION;//OUTアクションに遷移
			//表示状態は変化なし
		}
		/* 各アクション初回時から、開始イベント以外も成立してしまうものたち		*/
		if(MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.MY_TURN_COMING)//すすむアクション、自分の番来たイベントなら(すすむアクション初回時に点灯するマスが当てはまる)
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.MOVE;//すすむアクションに遷移
			MasuInfo[masu].DisplayState = ON;//表示状態はON
		}
		if(MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.DECIDED_ME)//すすむアクション、自分で確定イベントなら(すすむアクション初回時に確定するマスが当てはまる(つまり1すすむのときのみ))
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.MOVE;//すすむアクションに遷移
			MasuInfo[masu].DisplayState = ON;//表示状態はON
		}
	}
	/* すすむアクションの状態遷移	*/
	private void transitStateByMove(int masu)
	{
		if(MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.MY_TURN_COMING)//自分の番来たイベントなら
		{
			//アクション状態は変化なし
			MasuInfo[masu].DisplayState = ON;//表示状態をON
		}
		if (MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.DECIDED_ME)//自分で確定イベントなら
		{
			//アクション状態は変化なし
			MasuInfo[masu].DisplayState = ON;//表示状態をON
		}
		if (MasuInfo[masu].Event.MoveEvent == MASU_MOVE_EVENT.MOVE_END)//すすむ終了イベントなら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;//通常状態へ遷移
			//表示状態は変化なし
		}
	}
	/* 点滅アクションの状態遷移	*/
	private void transitStateByFlash(int masu)
	{
		if(MasuInfo[masu].Event.FlashEvent == MASU_FLASH_EVENT.CHANGE_ON_OFF)//点滅切り替えイベントなら
		{
			//アクション状態は変化なし
			MasuInfo[masu].DisplayState = !(MasuInfo[masu].DisplayState);//表示状態を反転
		}
		if(MasuInfo[masu].Event.FlashEvent == MASU_FLASH_EVENT.FLASH_END)//点滅おわりイベントなら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;//通常状態へ遷移
			//表示状態は変化なし
		}
	}
	/* Outアクションの状態遷移	*/
	private void transitStateByOutAction(int masu)
	{
		if(MasuInfo[masu].Event.OutActionEvent == MASU_OUT_ACTION_EVENT.MY_TURN_COMING)//OUT来たイベントなら
		{
			//アクション状態は変化なし
			MasuInfo[masu].DisplayState = OFF;//表示状態はOFF
		}
		if(MasuInfo[masu].Event.OutActionEvent == MASU_OUT_ACTION_EVENT.OUT_ACTION_END)//OUT終わりイベントなら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;//通常状態へ遷移
			MasuInfo[masu].DisplayState = OFF;//表示状態はOFF
		}
	}
	//==================================================//
	/*	出力処理											*/
	//==================================================//
	private void setActiveByMasuDisplayState(int masu)
	{
		/* DisplayStateはNameONを基準に点灯/消灯としているためNameOFFへの出力は反転させればよい	*/
		MasuInfo[masu].NameOnObject.SetActive(MasuInfo[masu].DisplayState);//NameOnObjectへの出力
		MasuInfo[masu].NameOffObject.SetActive(!MasuInfo[masu].DisplayState);//NameOffObjectへの出力
	}
	//==================================================//
	/*	各アクション終了後のクラス内部からの次アクションの設定*/
	//==================================================//
	private void prepareNextActionByInside()
	{
		prepareNextActionAfterMove();
		prepareNextActionAfterFlash();
		prepareNextActionAfterOutAction();
	}
	/* すすむアクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterMove()
	{
		if(FlagMoveIsFinished == true)
		{
			FlagMoveIsFinished = false;//すすむアクション終了フラグクリア

			setFlashRequest();
		}
	}
	/* 点滅アクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterFlash()
	{
		if(FlagFlashIsFinished == true)
		{
			FlagFlashIsFinished = false;//点滅アクション終了フラグクリア

			if (judgeStopMasu() == true)//止まったマスがOUTマスなら
			{
				setOutActionRequest();//OutActionの準備
			}
		}
	}
	private bool judgeStopMasu()
	{   /* 表示状態がONになってる最後のマス(=インデックス最大のマス=止まったマス)がOUTマスだったらOutAction要求	*/

		bool ret = false;

		/* 止まったマスがOUTマスか判定	*/
		switch(SugorokuNowPosition)
		{
			case 2://02_Out
			case 5://05_Out
			case 9:
			case 10:
				ret = true;
				break;
			default:
				break;
		}

		return ret;
	}
	/* Outアクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterOutAction()
	{
		//なし
		FlagOutIsFinished = false;//ほかのアクション状態の真似してるだけ
	}
	//==============================================================================//
	//	setter/getter																//
	//==============================================================================//
	//==================================================//
	/*	要求												*/
	//==================================================//
	public void JudgeSugorokuStart(string rouletteResult)
	{	/* ルーレット結果がすすむマスだったらすごろく要求する	*/
		switch(rouletteResult)
		{
			case "1StepON":
				Request.Move = true;		//すすむアクション要求をセット
				SugorokuMoveCounter = 1;	//すすめるカウンタに1をセット
				break;
			case "2StepON":
				Request.Move = true;		//すすむアクション要求をセット
				SugorokuMoveCounter = 2;	//すすめるカウンタに2をセット
				break;
			case "3StepON":
				Request.Move = true;		//すすむアクション要求をセット
				SugorokuMoveCounter = 3;	//すすめるカウンタに3をセット
				break;
			default:
				break;//それ以外の結果では何もしない
		}
	}
	private void setMoveRequest()
	{

	}
	private void setFlashRequest()
	{
		Request.Flash = true;
		FlashTimer = TIME_FLASH_CHANGE;
		FlashCounter = NUM_FLASH_MAX;
	}
	private void setOutActionRequest()
	{
		Request.OutAction = true;
		OutTargetMasuId = SugorokuNowPosition+1;//OUTアクションは、更新されたOutTargetMasuIdしかOUTにしないので+1しないと最初のマスをOUTにできない。。。
												//(+1しないと止まったマス(最初にOUTにしたいマス)からOUTできない)
		OutTargetUpdateWaitTimer = TIME_WAIT_OUT_TARGET;
	}
	private void clearMoveRequest()
	{
		Request.Move = false;
	}
	private void clearFlashRequest()
	{
		Request.Flash = false;
	}
	private void clearOutActionRequest()
	{
		Request.OutAction = false;
	}

	//==================================================//
	/*	テスト用											*/
	//==================================================//
	public void SetRequestOutAction_Test(bool input)
	{
		Request.OutAction = input;
	}
	public bool GetRequestOutAction_Test()
	{
		bool ret = Request.OutAction;
		return ret;
	}
	public void SetRequestMove_Test(bool input)
	{
		Request.Move = input;
	}
	public void SetRequestFlash_Test(bool input)
	{
		Request.Flash = input;
	}
	public void SetMasuInfoActionState_Test(int masu, int input)
	{
		switch(input)
		{
			case 0:
				MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;
				break;
			case 1:
				MasuInfo[masu].ActionState = ACTION_PATTERN.MOVE;
				break;
			case 2:
				MasuInfo[masu].ActionState = ACTION_PATTERN.FLASH;
				break;
			case 3:
				MasuInfo[masu].ActionState = ACTION_PATTERN.OUT_ACTION;
				break;
			default:
				break;
		}
	}
	public int GetMasuInfoEventMoveEvent_Test(int masu)
	{
		int ret = 0;

		switch(MasuInfo[masu].Event.MoveEvent)
		{
			case MASU_MOVE_EVENT.NONE:
				ret = 0;
				break;
			case MASU_MOVE_EVENT.MOVE_START:
				ret = 1;
				break;
			case MASU_MOVE_EVENT.MY_TURN_COMING:
				ret = 2;
				break;
			case MASU_MOVE_EVENT.DECIDED_ME:
				ret = 3;
				break;
			case MASU_MOVE_EVENT.MOVE_END:
				ret = 4;
				break;
		}

		return ret;
	}
	public int GetMasuInfoEventFlashEvent_Test(int masu)
	{
		int ret = 0;

		switch (MasuInfo[masu].Event.FlashEvent)
		{
			case MASU_FLASH_EVENT.NONE:
				ret = 0;
				break;
			case MASU_FLASH_EVENT.FLASH_START:
				ret = 1;
				break;
			case MASU_FLASH_EVENT.CHANGE_ON_OFF:
				ret = 2;
				break;
			case MASU_FLASH_EVENT.FLASH_END:
				ret = 3;
				break;
		}

		return ret;
	}
	public int GetMasuInfoEventOutActionEvent_Test(int masu)
	{
		int ret = 0;

		switch (MasuInfo[masu].Event.OutActionEvent)
		{
			case MASU_OUT_ACTION_EVENT.NONE:
				ret = 0;
				break;
			case MASU_OUT_ACTION_EVENT.OUT_ACTION_START:
				ret = 1;
				break;
			case MASU_OUT_ACTION_EVENT.MY_TURN_COMING:
				ret = 2;
				break;
			case MASU_OUT_ACTION_EVENT.OUT_ACTION_END:
				ret = 3;
				break;
		}

		return ret;
	}
	public void SetMasuInfoMoveEvent_Test(int masu,int input)
	{
		switch (input)
		{
			case 0:
				MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.NONE;
				break;
			case 1:
				MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.MOVE_START;
				break;
			case 2:
				MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.MY_TURN_COMING;
				break;
			case 3:
				MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.DECIDED_ME;
				break;
			case 4:
				MasuInfo[masu].Event.MoveEvent = MASU_MOVE_EVENT.MOVE_END;
				break;
			default:
				break;
		}
	}
	public void SetMasuInfoFlashEvent_Test(int masu, int input)
	{
		switch (input)
		{
			case 0:
				MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.NONE;
				break;
			case 1:
				MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.FLASH_START;
				break;
			case 2:
				MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.CHANGE_ON_OFF;
				break;
			case 3:
				MasuInfo[masu].Event.FlashEvent = MASU_FLASH_EVENT.FLASH_END;
				break;
			default:
				break;
		}
	}
	public void SetMasuInfoOutActionEvent_Test(int masu,int input)
	{
		switch (input)
		{
			case 0:
				MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.NONE;
				break;
			case 1:
				MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.OUT_ACTION_START;
				break;
			case 2:
				MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.MY_TURN_COMING;
				break;
			case 3:
				MasuInfo[masu].Event.OutActionEvent = MASU_OUT_ACTION_EVENT.OUT_ACTION_END;
				break;
			default:
				break;
		}
	}
	public void SetMasuInfoDisplayState_Test(int masu,bool input)
	{
		MasuInfo[masu].DisplayState = input;
	}
	public int GetMasuInfoActionState_Test(int masu)
	{
		int ret = 0;

		switch(MasuInfo[masu].ActionState)
		{
			case ACTION_PATTERN.DEFAULT:
				ret = 0;
				break;
			case ACTION_PATTERN.MOVE:
				ret = 1;
				break;
			case ACTION_PATTERN.FLASH:
				ret = 2;
				break;
			case ACTION_PATTERN.OUT_ACTION:
				ret = 3;
				break;
			default:
				break;
		}

		return ret;
	}
	public bool GetMasuInfoDisplayState_Test(int masu)
	{
		return MasuInfo[masu].DisplayState;
	}
}
