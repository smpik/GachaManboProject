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
		OUT_ACTION,		//Outアクション
		GOAL_ACTION		//GOALアクション
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
	private enum MASU_GOAL_ACTION_EVENT
	{
		NONE=0,//イベントなし
		GOAL_START,//GOALアクション開始イベント
		CHANGE_ON_OFF,//点滅切り替えイベント
		GOAL_END//GOALアクション終了イベント
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
	private const int TIME_FLASH_CHANGE_FOR_GOAL = 20;//GOALアクションにて用いる点滅を切り替えるまでの待ち時間
	
	//==================================================//
	/* 構造体定義											*/
	//==================================================//
	private struct EventStruct//イベントを取りまとめた構造体。イベント増えたらここに追加
	{
		public MASU_MOVE_EVENT MoveEvent;
		public MASU_FLASH_EVENT FlashEvent;
		public MASU_OUT_ACTION_EVENT OutActionEvent;
		public MASU_GOAL_ACTION_EVENT GoalActionEvent;
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
		public bool GoalAction;//GOALアクション要求
	}
	//==================================================//
	/* 変数実態定義										*/
	//==================================================//
	private MasuInfoStruct[] MasuInfo;
	private RequestInfoStruct Request;

	private int SugorokuNowPosition;//すごろく現在地
	private int SugorokuMoveCounter;//すすめるカウンタ
	private int FlashTimer;//点滅時間を保持するタイマー
	private int FlashTimerForGoal;//GOALアクションにて用いる点滅時間を保持するタイマー
	private int OutTargetUpdateWaitTimer;//OUT対象マスの更新までの待ち時間を保持するタイマー
	private int OutTargetMasuId;//OUT対象マスを指定するためのID
	private bool FlagOutTargetUpdate;//OUT対象マスの更新があったことを示すためのフラグ
	private int FlashCounter;//点滅切り替えをあと何回するかを保持するカウンタ

	private bool FlagMoveIsFinished;//すすむアクション終了を示すフラグ(decideInputでセットし、prepareNextActionByInsideにてクリアする)
	private bool FlagFlashIsFinished;
	private bool FlagOutIsFinished;
	private bool FlagGoalIsFinished;

	private bool FlagJackpotIsFinished;//ジャックポット終了を示すフラグ(ジャックポット中のみfalse。他クラスからアクセッサを介してセットする)

	private bool SugorokuIsReadyOk;//すごろくの準備OKを表す(これがfalseのときルーレット要求はできない)

	private CoinEventStockManager CoinEventStockManager;
	private CoinEventController CoinEventController;
	private UIController UIController;

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
		FlagGoalIsFinished = false;

		FlagJackpotIsFinished = true;//初期時ジャックポット中ではないはず

		MasuInfo[NUM_MASU_FIRST].DisplayState = ON;		//Startマスは最初から点灯
		setActiveByMasuDisplayState(NUM_MASU_FIRST);    //Startマスは最初から点灯

		SugorokuIsReadyOk = true;//初期時はすごろく準備OK(ルーレット要求可)

		CoinEventStockManager = GameObject.Find("EnterCoinGate").GetComponent<CoinEventStockManager>();
		CoinEventController = GameObject.Find("EnterCoinGate").GetComponent<CoinEventController>();
		UIController = GameObject.Find("Main Camera").GetComponent<UIController>();
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
		Request.GoalAction = false;
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
			|| (Request.OutAction == true)
			|| (Request.GoalAction == true))
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
		if(Request.GoalAction == true)
		{
			decideInputByGoalAction();
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
	/* GOALアクションのInput確定処理	*/
	private void decideInputByGoalAction()
	{
		if (FlagJackpotIsFinished == false)//ジャックポット中なら
		{
			countTimerForGoal();//GOALアクション用点滅タイマの更新
		}
		else//ジャックポット終了なら
		{
			clearGoalActionRequest();//GOALアクション要求をクリア
			FlagGoalIsFinished = true;//GOALアクション終了フラグをセット
		}
	}
	private void countTimerForGoal()
	{
		if(FlashTimerForGoal > 0)//カウント中
		{
			FlashTimerForGoal--;
		}
		else//タイムアップ(点滅切り替えタイミング
		{
			FlashTimerForGoal = TIME_FLASH_CHANGE_FOR_GOAL;//タイマリセット
		}
	}
	//==================================================//
	/*	イベント発行										*/
	//==================================================//
	private void fireEvent(int masu)
	{
		fireEventByMove(masu);
		fireEventByFlash(masu);
		fireEventByOutAction(masu);
		fireEventByGoalAction(masu);
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
	/* Outアクションのイベント発行処理	*/
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
	/* GOALアクションのイベント発行処理	*/
	private void fireEventByGoalAction(int masu)
	{
		MasuInfo[masu].Event.GoalActionEvent = MASU_GOAL_ACTION_EVENT.NONE;//イベントがあれば上書きされる

		if((MasuInfo[masu].ActionState == ACTION_PATTERN.DEFAULT) && (Request.GoalAction == true))
		{	//通常状態、GOAL要求あり
			MasuInfo[masu].Event.GoalActionEvent = MASU_GOAL_ACTION_EVENT.GOAL_START;//GOALアクション開始イベント発行
		}
		if((MasuInfo[masu].ActionState == ACTION_PATTERN.GOAL_ACTION) && (FlashTimerForGoal==0))
		{	//GOALアクション状態、GOALアクション用点滅タイマタイムアップ
			MasuInfo[masu].Event.GoalActionEvent = MASU_GOAL_ACTION_EVENT.CHANGE_ON_OFF;//点滅切り替えイベント発行
		}
		if((MasuInfo[masu].ActionState == ACTION_PATTERN.GOAL_ACTION) && (Request.GoalAction == false))
		{	//GOALアクション状態、GOAL要求なし
			MasuInfo[masu].Event.GoalActionEvent = MASU_GOAL_ACTION_EVENT.GOAL_END;//GOALアクション終了イベント発行
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
			case ACTION_PATTERN.GOAL_ACTION:
				transitStateByGoalAction(masu);
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
		if(MasuInfo[masu].Event.GoalActionEvent == MASU_GOAL_ACTION_EVENT.GOAL_START)//GOAL開始なら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.GOAL_ACTION;//GOALアクションに遷移
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
	/* GOALアクションの状態遷移	*/
	private void transitStateByGoalAction(int masu)
	{
		if(MasuInfo[masu].Event.GoalActionEvent == MASU_GOAL_ACTION_EVENT.CHANGE_ON_OFF)//点滅切り替えイベント発行なら
		{
			//アクション状態は変化なし
			MasuInfo[masu].DisplayState = !(MasuInfo[masu].DisplayState);//表示状態を反転
		}
		if(MasuInfo[masu].Event.GoalActionEvent == MASU_GOAL_ACTION_EVENT.GOAL_END)//GOALアクション終了イベントなら
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;//通常状態へ遷移
			MasuInfo[masu].DisplayState = ON;//表示状態はON固定
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
		prepareNextActionAfterGoalAction();
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
	/* 点滅アクション終了時の次アクションの準備(止まったマスによる各要求もここで行う)	*/
	private void prepareNextActionAfterFlash()
	{
		if(FlagFlashIsFinished == true)
		{
			FlagFlashIsFinished = false;//点滅アクション終了フラグクリア

			switch (getTypeOfStopMasu())//OutAction、Goalアクションの準備、を本関数内に記載したい(階層的な話)ので本処理を関数化しない(しても実動作は問題なし)
			{
				case "Out":
					setOutActionRequest();//OutActionの準備
					break;
				case "Coin10EventStock+1"://次アクションはなし
					setSugorokuIsReadyOk();//すごろく準備OKフラグをセット
					CoinEventStockManager.CountCoinEventStock(CoinEventStockManager.COIN_EVENT_ID.COIN_EVENT_PATTERN_0);//ストック+1要求
					break;
				case "Coin20EventStock+1"://次アクションはなし
					setSugorokuIsReadyOk();//すごろく準備OKフラグをセット
					CoinEventStockManager.CountCoinEventStock(CoinEventStockManager.COIN_EVENT_ID.COIN_EVENT_PATTERN_1);//ストック+1要求
					break;
				case "Coin50EventStock+1"://次アクションはなし
					setSugorokuIsReadyOk();//すごろく準備OKフラグをセット
					CoinEventStockManager.CountCoinEventStock(CoinEventStockManager.COIN_EVENT_ID.COIN_EVENT_PATTERN_2);//ストック+1要求
					break;
				case "Chance"://次アクションはなし
					setSugorokuIsReadyOk();//すごろく準備OKフラグをセット
					UIController.SetActiveExcludeCanvas(true);//除外キャンバス表示要求
					break;
				case "Goal":
					CoinEventController.JackpotRequest();//ジャックポット要求
					setGoalActionRequest();//GoalActionの準備
					break;
				default://次のアクションがないとき(すごろく全体としてアクション終了のとき)
					setSugorokuIsReadyOk();//すごろく準備OKフラグをセット
					break;
			}
		}
	}
	private string getTypeOfStopMasu()
	{   /* 現在地をが何マスか(種類)を返す	*/

		string ret = "";

		/* 止まったマスを判定	*/
		switch(SugorokuNowPosition)
		{
			case 1://01_Coin10EventStock+1
				ret = "Coin10EventStock+1";
				break;
			case 2://02_Out
				ret = "Out";
				break;
			case 3://03_Coin10EventStock+1
				ret = "Coin10EventStock+1";
				break;
			case 4://04_Coin10EventStock+1
				ret = "Coin10EventStock+1";
				break;
			case 5://05_Out
				ret = "Out";
				break;
			case 6://06_Coin20EventStock+1
				ret = "Coin20EventStock+1";
				break;
			case 7://07_Coin50EventStock+1
				ret = "Coin50EventStock+1";
				break;
			case 8://08_Chance
				ret = "Chance";
				break;
			case 9://09_out
				ret = "Out";
				break;
			case 10://10_Out
				ret = "Out";
				break;
			case 11://11_Goal
				ret = "Goal";
				break;
			default:
				break;
		}

		return ret;
	}
	/* Outアクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterOutAction()
	{
		if (FlagOutIsFinished == true)
		{
			FlagOutIsFinished = false;
			setSugorokuIsReadyOk();//すごろく準備OKフラグをセット(OUTアクション終了ということは次のアクションはもうないということだから)
			CoinEventController.AddJackpot();//ジャックポット枚数の追加
		}
	}
	/* GOALアクション終了時の次アクションの準備	*/
	private void prepareNextActionAfterGoalAction()
	{
		if (FlagGoalIsFinished == true)
		{
			FlagGoalIsFinished = false;
			setOutActionRequest();
		}
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
				clearSugorokuIsReadyOk();	//すごろく準備OKフラグをクリア(すごろく全体としてアクション開始だから)
				setMoveRequest();			//すすむアクション要求をセット
				SugorokuMoveCounter = 1;	//すすめるカウンタに1をセット
				break;
			case "2StepON":
				clearSugorokuIsReadyOk();   //すごろく準備OKフラグをクリア(すごろく全体としてアクション開始だから)
				setMoveRequest();			//すすむアクション要求をセット
				SugorokuMoveCounter = 2;	//すすめるカウンタに2をセット
				break;
			case "3StepON":
				clearSugorokuIsReadyOk();   //すごろく準備OKフラグをクリア(すごろく全体としてアクション開始だから)
				setMoveRequest();			//すすむアクション要求をセット
				SugorokuMoveCounter = 3;	//すすめるカウンタに3をセット
				break;
			default:
				break;//それ以外の結果では何もしない
		}
	}
	public void SetFlagJackpotIsFinished()
	{//ジャックポット終了したら呼ばれる
		FlagJackpotIsFinished = true;
	}
	public void ClearFlagJackpotIsFinished()
	{
		FlagJackpotIsFinished = false;
	}
	public bool GetFlagJackpotIsFinished()
	{
		return FlagJackpotIsFinished;
	}
	public bool GetSugorokuIsReadyOk()
	{
		return SugorokuIsReadyOk;
	}
	private void setSugorokuIsReadyOk()
	{
		SugorokuIsReadyOk = true;
	}
	private void clearSugorokuIsReadyOk()
	{
		SugorokuIsReadyOk = false;
	}
	private void setMoveRequest()
	{
		Request.Move = true;
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
	private void setGoalActionRequest()
	{
		Request.GoalAction = true;
		FlashTimerForGoal = TIME_FLASH_CHANGE_FOR_GOAL;
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
	private void clearGoalActionRequest()
	{
		Request.GoalAction = false;
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
