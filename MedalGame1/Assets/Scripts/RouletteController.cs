using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteController : MonoBehaviour
{
	/* enum、マクロ、構造体											*/
	public enum ACTION_PATTERN//各アクションは終了したら通常に必ず戻す(順番とかを作ると汎用性が下がる)
	{
		DEFAULT = 0,	//通常
		ROULETTE,		//ルーレット
		WAIT			//待ち
	}
	private const bool OFF = false;	//消灯
	private const bool ON = true;	//点灯
	private enum MASU_TURN_OFF_EVENT
	{
		NONE = 0,				//イベントなし
		TURN_OFF,				//消灯イベント
	}
	private enum MASU_ROULETTE_EVENT
	{
		NONE = 0,				//イベントなし
		ROULETTE_START,			//ルーレット開始イベント
		ROULETTE_END,			//ルーレット終了イベント
		ROULETTE_TURN_COMING,	//ルーレット自分の番来たイベント
		ROULETTE_TURN_END		//ルーレット自分の番終わりイベント
	}
	private enum MASU_WAIT_EVENT
	{
		NONE = 0,
		WAIT_START,
		WAIT_END
	}
	private struct EventStruct//イベントを取りまとめた構造体。イベント増えたらここに追加
	{
		public MASU_TURN_OFF_EVENT TurnOffEvent;
		public MASU_ROULETTE_EVENT RouletteEvent;
		public MASU_WAIT_EVENT WaitEvent;
	}
	private struct MasuInfoStruct
	{
		public GameObject NameOnObject;		//ONのほうのオブジェクト
		public GameObject NameOffObject;    //OFFのほうのオブジェクト
		public GameObject NameExcludeObject;//除外表現用のオブジェクト
		public ACTION_PATTERN ActionState;	//状態1。大きい状態(ルーレットマスのふるまいに何が要求されているか)
		public bool DisplayState;			//状態2。小さい状態(点灯/消灯。DisplayState=true ⇒ NameON=true、NameOFF=false)
		public bool Excluded;				//除外されているかを示すフラグ(trueなら除外されているマス)
		public EventStruct Event;			//イベント
	}
	private struct RequestInfoStruct	//ルーレットマスの動作に対する要求
	{
		public bool TurnOff;	//消灯要求
		public bool Roulette;   //ルーレット要求
		public bool Wait;		//待ち要求
	}
	private const int NUM_MASU_FIRST = 0;		//最初のマスの番号
	private const int NUM_MASU_MAX = 7;			//マスの数
	private const int NUM_EXCLUDE_MAX = 5;		//除外できるマスの最大
	private const uint TIME_TURN_ON = 5;		//ルーレットで自分の番が来たときに光らせる時間
	private const uint TIME_RANDOM_MIN = 70;	//ルーレット時間をランダムに決める際の下限値
	private const uint TIME_RANDOM_MAX = 150;   //ルーレット時間をランダムに決める際の上限値
	private const uint TIME_WAIT = 100;//ルーレット後、次のルーレット開始までの待ち時間

	private readonly string[] MASU_NAME =//ルーレットする順番に定義する
		{   "Fault",
			"1Step",
			"Coin10EventStock+1",
			"2Step",
			"Coin20EventStock+1",
			"3Step",
			"Coin50EventStock+1"
		};


	/* 変数実体定義													*/
	private MasuInfoStruct[] MasuInfo;

	private RequestInfoStruct Request;

	private SugorokuController SugorokuControllerInstance;
	private CoinEventStockManager CoinEventStockManagerInstance;

	private int RouletteOnMasuIdThisCycle;//今周期に光らせるマスのID
	private int RouletteOnMasuIdBeforeCycle;//前周期で光らせたマスのID
	private uint RouletteTimer;//ルーレット残り時間を示すタイマー
	private uint TurnOnTimer;//ルーレットで自分の番が来たときに光らせる時間を保持するタイマー
	private uint WaitTimer;//ルーレット終了後、次のルーレット開始までの待ち時間を保持するタイマー
	private int ExcludedMasuCounter;//除外されたマスの数を数えるカウンタ
	private bool[] ExcludedMasuList;//除外されたマスリスト(trueが除外されている)
	private bool RouletteIsReadyOk;//ルーレット準備OKフラグ(RouletteStockControllerに見せるやつ。これがfalseだとルーレット要求できない)

	// Start is called before the first frame update
	void Start()
    {
		SugorokuControllerInstance = GameObject.Find("SugorokuMasu").GetComponent<SugorokuController>();
		CoinEventStockManagerInstance = GameObject.Find("EnterCoinGate").GetComponent<CoinEventStockManager>();

		generateStructInstance();		//各構造体のインスタンス生成

		initMasuInfo();					//各マスの情報初期化
		initRequest();                  //RequestInfoの初期化

		/* 各内部変数の初期化 */
		RouletteOnMasuIdThisCycle = NUM_MASU_MAX;
		RouletteOnMasuIdBeforeCycle = NUM_MASU_MAX;
		TurnOnTimer = TIME_TURN_ON;
		ExcludedMasuList = new bool[NUM_MASU_MAX];
		for(int i=NUM_MASU_FIRST;i<NUM_MASU_MAX;i++)
		{
			ExcludedMasuList[i] = false;
		}
		RouletteIsReadyOk = true;//初期時はルーレット要求許可

		resetDisplayStateByExclude();//初期時は除外用表示はしない
	}

    // Update is called once per frame
    void Update()
    {
		if (isRequestComing())//もし要求あるなら
		{
			decideInput();//Input確定処理
			for(int masu = NUM_MASU_FIRST; masu < NUM_MASU_MAX; masu++)
			{
				fireEvent(masu);//イベント発行
				transitionDisplayState(masu);//状態遷移
				setActiveByMasuDisplayState(masu);//出力処理
			}
			settingRequestByInside();//各ActionState終了後の処理
		}
	}

	//==============================================================================//
	//	初期化処理																	//
	//==============================================================================//
	/* MasuInfoのインスタンス生成	*/
	private void generateStructInstance()
	{
		MasuInfo = new MasuInfoStruct[NUM_MASU_MAX];
		Request = new RequestInfoStruct();
	}
	//==================================================//
	/* MasuInfoの情報を初期化する							*/
	//==================================================//
	private void initMasuInfo()
	{
		for (int masu = NUM_MASU_FIRST; masu < NUM_MASU_MAX; masu++)
		{
			initMasuInfoNameOnObject(masu);		//NameOnObjectの初期化
			initMasuInfoNameOffObject(masu);    //NameOffObjectの初期化
			initMasuInfoNameExcludeObject(masu);//NameExcludeObjectの初期化
			initMasuInfoActionState(masu);		//ActionStateの初期化
			initMasuInfoDisplayState(masu);		//DisplayStateの初期化
			initMasuInfoExcluded(masu);			//Excludedの初期化
			initMasuInfoEvent(masu);			//Eventの初期化
		}
	}
	/* MasuInfoActionStateの初期化	*/
	private void initMasuInfoActionState(int masu)
	{
		MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;
	}
	/* MasuInfoDisplayStateの初期化	*/
	private void initMasuInfoDisplayState(int masu)
	{
		MasuInfo[masu].DisplayState = OFF;
	}
	/* MasuInfoExcludedの初期化	*/
	private void initMasuInfoExcluded(int masu)
	{
		MasuInfo[masu].Excluded = false;
	}
	/* MasuInfoEventの初期化	*/
	private void initMasuInfoEvent(int masu)
	{
		MasuInfo[masu].Event.TurnOffEvent = MASU_TURN_OFF_EVENT.NONE;
		MasuInfo[masu].Event.RouletteEvent = MASU_ROULETTE_EVENT.NONE;
		MasuInfo[masu].Event.WaitEvent = MASU_WAIT_EVENT.NONE;
	}
	/* MasuInfoNameOnObjectの初期化	*/
	private void initMasuInfoNameOnObject(int masu)
	{
		MasuInfo[masu].NameOnObject = GameObject.Find(MASU_NAME[masu] + "ON");
	}
	/* MasuInfoNameOffObjectの初期化	*/
	private void initMasuInfoNameOffObject(int masu)
	{
		MasuInfo[masu].NameOffObject = GameObject.Find(MASU_NAME[masu] + "OFF");
	}
	/* MasuInfoNameExcludeObjectの初期化	*/
	private void initMasuInfoNameExcludeObject(int masu)
	{
		MasuInfo[masu].NameExcludeObject = GameObject.Find(MASU_NAME[masu] + "EXCLUDE");
	}
	//==================================================//
	/* Requestの各メンバを初期化する						*/
	//==================================================//
	private void initRequest()
	{
		Request.TurnOff = false;
		Request.Roulette = false;
		Request.Wait = false;
	}
	//==============================================================================//
	//	Update処理																	//
	//==============================================================================//
	//==================================================//
	/* 要求が発生しているか確認を行う						*/
	//==================================================//
	private bool isRequestComing()
	{
		bool ret = false;

		if( (Request.TurnOff == true)
			|| (Request.Roulette == true)
			|| (Request.Wait == true))
		{	//どれかひとつでも要求が発生していればtrue
			ret = true;
		}

		return ret;
	}
	//==================================================//
	/* 要求ごとのInput確定処理							*/
	//==================================================//
	private void decideInput()
	{
		//消灯要求時はInput設定必要なし(全マス対象だから特に下準備なし)

		if(Request.Roulette)
		{
			decideInputByRoulette();    //ルーレット要求によるInput確定処理(ルーレット継続するか、光らせるマスを更新する)
		}

		if(Request.Wait)
		{
			decideInputByWait();
		}
	}
	/* ルーレット要求によるInput確定処理(ルーレット継続するか、光らせるマスを更新する)	*/
	private void decideInputByRoulette()
	{
		countTimerForRoulette();

		excludeMasu();//除外処理

		RouletteOnMasuIdBeforeCycle = RouletteOnMasuIdThisCycle;//前回光らせたマスIDの更新

		/* 光らせるマス更新	*/
		if (TurnOnTimer <= 0)//光らせる時間(点灯時間)が終わったなら
		{
			updateRouletteOnMasuAvoidExcludedMasu();//除外マスを避けて光らせるマスを更新する
			setTurnOnTimer();//点灯時間タイマをリセット
		}

	}
	/* ルーレット用のタイマーカウント	*/
	private void countTimerForRoulette()
	{
		if (RouletteTimer > 0)
		{
			RouletteTimer--;
		}
		if(TurnOnTimer > 0)
		{
			TurnOnTimer--;
		}
	}
	/* 除外処理	*/
	private void excludeMasu()
	{
		/* 各マスが除外の対象になっていないかチェックする	*/
		/* 流れ：チェック対象のマス名取得 → 除外マスカウンタの値を見る → チェック対象マス名が除外マスカウンタの値に割り当てられている除外マス名と一致するか	*/
		for (int masu = NUM_MASU_FIRST; masu < NUM_MASU_MAX; masu++)
		{
			string name = MasuInfo[masu].NameOnObject.name;

			switch (ExcludedMasuCounter)
			{
				case 0:
					/* 除外マスカウンタが0 = マスの除外なし	*/
					MasuInfo[masu].Excluded = false;
					break;
				case 1:
					/* 除外マスカウンタが1なら1すすむマスを除外	*/
					if (name == "1StepON")//ON、OFFどちらでもいい(1すすむマスだと判別できれば)
					{
						MasuInfo[masu].Excluded = true;
					}
					break;
				case 2:
					/* 除外マスカウンタが2なら2すすむマスも除外	*/
					if( (name == "1StepON") || (name == "2StepON"))
					{
						MasuInfo[masu].Excluded = true;
					}
					break;
				case 3:
					/* 除外マスカウンタが3ならコイン50イベントストック+1マスも除外	*/
					if ((name == "1StepON") || (name == "2StepON")
						|| (name == "Coin50EventStock+1ON"))
					{
						MasuInfo[masu].Excluded = true;
					}
					break;
				case 4:
					/* 除外マスカウンタが4ならコイン20イベントストック+1マスも除外	*/
					if ((name == "1StepON") || (name == "2StepON")
						|| (name == "Coin50EventStock+1ON") || (name == "Coin20EventStock+1ON"))
					{
						MasuInfo[masu].Excluded = true;
					}
					break;
				case 5:
					/* 除外マスカウンタが5ならコイン10イベントストック+1マスも除外	*/
					if ((name == "1StepON") || (name == "2StepON")
						|| (name == "Coin50EventStock+1ON") || (name == "Coin20EventStock+1ON")
						|| (name == "Coin10EventStock+1ON"))
					{
						MasuInfo[masu].Excluded = true;
					}
					break;
				default:	//それ以外の値のときは何もしない
					break;
			}
		}
	}
	/* 除外マスを避けて光らせるマスを更新する	*/
	private void updateRouletteOnMasuAvoidExcludedMasu()
	{
		/* 光らせるマスが除外されている場合は、更新し続ける	*/
		do//do-while文はdo文内の処理を最低1回は行う(whileの条件式はdo文内の処理実行後に評価される)ため、光らせるマスの更新は必ず行うことができる
		{
			updateRouletteOnMasu();//光らせるマスを更新する
		} while (MasuInfo[RouletteOnMasuIdThisCycle].Excluded == true);
	}
	/* 光らせるマス更新処理	*/
	private void updateRouletteOnMasu()
	{
		RouletteOnMasuIdThisCycle++;

		if (RouletteOnMasuIdThisCycle >= NUM_MASU_MAX)//マス最大数までいってたら最初から
		{
			RouletteOnMasuIdThisCycle = NUM_MASU_FIRST;
		}
	}
	/* 待ち要求によるInput確定処理	*/
	private void decideInputByWait()
	{
		countTimerForWait();
	}
	/* 待ち用のタイマーカウント	*/
	private void countTimerForWait()
	{
		if(WaitTimer > 0)
		{
			WaitTimer--;
		}
	}
	//==================================================//
	/* イベント発行										*/
	//==================================================//
	private void fireEvent(int masu)
	{
		fireEventByTurnOff(masu);
		fireEventByRoulette(masu);
		fireEventByWait(masu);
	}
	/* 消灯イベント発行	*/
	private void fireEventByTurnOff(int masu)
	{
		MasuInfo[masu].Event.TurnOffEvent = MASU_TURN_OFF_EVENT.NONE;//何もなければイベントなし(FS)

		if (Request.TurnOff)
		{	//消灯要求あり
			MasuInfo[masu].Event.TurnOffEvent = MASU_TURN_OFF_EVENT.TURN_OFF;//消灯イベント発行
		}
	}
	/* ルーレットイベント発行	*/
	private void fireEventByRoulette(int masu)
	{
		MasuInfo[masu].Event.RouletteEvent = MASU_ROULETTE_EVENT.NONE;//何もなければイベントなし(FS)

		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.DEFAULT) && (Request.Roulette == true))
		{   //ルーレットが始まったら(= 前回まで通常状態 && ルーレット要求あり)
			MasuInfo[masu].Event.RouletteEvent = MASU_ROULETTE_EVENT.ROULETTE_START;//ルーレット開始イベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.ROULETTE) && (masu != RouletteOnMasuIdBeforeCycle) && (masu == RouletteOnMasuIdThisCycle))
		{	//自分が光る番が来たら(= ルーレット状態 && 前回は自分じゃない && 今回は自分)
			MasuInfo[masu].Event.RouletteEvent = MASU_ROULETTE_EVENT.ROULETTE_TURN_COMING;//自分の番来たイベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.ROULETTE) && (masu == RouletteOnMasuIdBeforeCycle) && (masu != RouletteOnMasuIdThisCycle))
		{   //自分が光る番がおわってたら(= ルーレット状態 && 前回は自分 && 今回は自分じゃない)
			MasuInfo[masu].Event.RouletteEvent = MASU_ROULETTE_EVENT.ROULETTE_TURN_END;//自分の番終わりイベント発行
		}
		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.ROULETTE) && (RouletteTimer <= 0))	//自分の番イベントを書き換える(ルーレット終了イベントのほうが優先度高)ためこの判定は一番最後
		{   //ルーレットが終わったら(= 前回までルーレット状態 && ルーレット要求なし)
			MasuInfo[masu].Event.RouletteEvent = MASU_ROULETTE_EVENT.ROULETTE_END;//ルーレット終了イベント発行
		}
	}
	/* 待ちイベント発行	*/
	private void fireEventByWait(int masu)
	{
		MasuInfo[masu].Event.WaitEvent = MASU_WAIT_EVENT.NONE;//何もなければイベントなし(FS)

		if ((MasuInfo[masu].ActionState == ACTION_PATTERN.DEFAULT) && (Request.Wait == true))
		{//待ちが始まったら(= 前回まで通常状態 && 待ち要求あり)
			MasuInfo[masu].Event.WaitEvent = MASU_WAIT_EVENT.WAIT_START;//待ち開始イベント発行
		}
		if((MasuInfo[masu].ActionState == ACTION_PATTERN.WAIT) && ( WaitTimer<=0 ))
		{//待ちが終わったら(= 前回まで待ち状態 && 待ち時間終了)
			MasuInfo[masu].Event.WaitEvent = MASU_WAIT_EVENT.WAIT_END;//待ち終了イベント発行
		}
	}
	//==================================================//
	/* 状態遷移											*/
	//==================================================//
	/* マス表示状態の状態遷移	*/
	private void transitionDisplayState(int masu)
	{
		switch(MasuInfo[masu].ActionState)
		{
			case ACTION_PATTERN.DEFAULT:
				transitionDisplayStateByDefault(masu);//通常状態での状態遷移処理を実行
				break;
			case ACTION_PATTERN.ROULETTE:
				transitionDisplayStateByRoulette(masu);//ルーレット状態での状態遷移処理を実行
				break;
			case ACTION_PATTERN.WAIT:
				transitionDisplayStateByWait(masu);//待ち状態での状態線処理を実行
				break;
			default://FS。なにもしない
				break;
		}
	}
	/* 通常状態での状態遷移処理	*/
	private void transitionDisplayStateByDefault(int masu)
	{
		/* ルーレット開始イベント	*/
		if (MasuInfo[masu].Event.RouletteEvent == MASU_ROULETTE_EVENT.ROULETTE_START)
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.ROULETTE;	//大状態=ルーレット
			MasuInfo[masu].DisplayState = OFF;						//小状態=消灯(強制的にOFFにする。ルーレット開始時に点灯しているマスを消すため)
		}

		/* 消灯イベント	*/
		if (MasuInfo[masu].Event.TurnOffEvent == MASU_TURN_OFF_EVENT.TURN_OFF)
		{
												//大状態=通常(変化なし)
			MasuInfo[masu].DisplayState = OFF;	//小状態=消灯
		}

		/* 待ち開始イベント	*/
		if (MasuInfo[masu].Event.WaitEvent == MASU_WAIT_EVENT.WAIT_START)
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.WAIT;   //大状態=待ち
																//小状態そのまま
		}
	}
	/* ルーレット状態での状態遷移処理	*/
	private void transitionDisplayStateByRoulette(int masu)
	{
		/* ルーレット終了イベント	*/
		if(MasuInfo[masu].Event.RouletteEvent == MASU_ROULETTE_EVENT.ROULETTE_END)
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;    //大状態=通常
																	//小状態は今の状態に依存
		}

		/* ルーレット自分の番来たイベント	*/
		if(MasuInfo[masu].Event.RouletteEvent == MASU_ROULETTE_EVENT.ROULETTE_TURN_COMING)
		{
												//大状態=ルーレット(変化なし)
			MasuInfo[masu].DisplayState = ON;	//小状態=点灯
		}

		/* ルーレット自分の番終わりイベント	*/
		if(MasuInfo[masu].Event.RouletteEvent == MASU_ROULETTE_EVENT.ROULETTE_TURN_END)
		{
												//大状態=ルーレット(変化なし)
			MasuInfo[masu].DisplayState = OFF;	//小状態=消灯
		}

		/* 消灯イベント	*/
		if (MasuInfo[masu].Event.TurnOffEvent == MASU_TURN_OFF_EVENT.TURN_OFF)
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;	//大状態=通常
			MasuInfo[masu].DisplayState = OFF;                      //小状態=消灯
		}
	}
	/* 待ち状態での状態遷移処理	*/
	private void transitionDisplayStateByWait(int masu)
	{
		/* 待ち終了イベント	*/
		if(MasuInfo[masu].Event.WaitEvent == MASU_WAIT_EVENT.WAIT_END)
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;	//大状態=通常
																	//小状態そのまま
		}

		/* 消灯イベント	*/
		if (MasuInfo[masu].Event.TurnOffEvent == MASU_TURN_OFF_EVENT.TURN_OFF)
		{
			MasuInfo[masu].ActionState = ACTION_PATTERN.DEFAULT;    //大状態=通常
			MasuInfo[masu].DisplayState = OFF;                      //小状態=消灯
		}
	}
	//==================================================//
	/* 出力処理											*/
	//==================================================//
	private void setActiveByMasuDisplayState(int masu)
	{
		if (MasuInfo[masu].Excluded != true)//除外マスでなければ出力
		{
			/* DisplayStateはNameONを基準に点灯/消灯としているためNameOFFへの出力は反転させればよい	*/
			MasuInfo[masu].NameOnObject.SetActive(MasuInfo[masu].DisplayState);//NameOnObjectへの出力
			MasuInfo[masu].NameOffObject.SetActive(!MasuInfo[masu].DisplayState);//NameOffObjectへの出力
		}
		else//除外マスであれば
		{
			//SetActiveしない(してしまうと、点灯or消灯用planeと除外用planeの2つが同時に表示される状況になり、確実に除外マスを前面に表示できないため)
		}
	}
	//==================================================//
	/* 各状態終了後の処理									*/
	//==================================================//
	private void settingRequestByInside()
	{
		Request.TurnOff = false;//消灯要求解除(消灯要求は1周期の処理で完了するため必ずクリアする)

		if ((Request.Roulette == true) && (RouletteTimer <= 0))//ルーレット状態が終わったら(ルーレット状態?の条件を入れとかないとルーレットタイマがタイムアップしてたら常に待ち要求セットしてしまう
		{
			ClearRouletteRequest();//ルーレット要求をクリアする
			setWaitRequest();//待ち要求セット
			SugorokuControllerInstance.JudgeSugorokuStart(getRouletteResultMasuName());//すごろくにルーレット結果を渡す
			CoinEventStockManagerInstance.JudgeRouletteResultIsCoinEventStock(getRouletteResultMasuName());//コイン放出イベントストックにルーレット結果を渡す
			Debug.Log("ルーレット結果"+getRouletteResultMasuName()+"を渡した");
		}

		if((Request.Wait==true) && (WaitTimer <= 0))//待ち状態が終わったら
		{
			clearWaitRequest();//待ち要求をクリア
			RouletteIsReadyOk = true;//ルーレット要求を許可
			clearExcludedMasuCounter();//除外マスをクリア
		}
	}

	//==============================================================================//
	//	Setter、Getter																//
	//==============================================================================//
	public void SetTurnOffRequest()
	{
		Request.TurnOff = true;
	}
	public void ClearTurnOffRequest()
	{
		Request.TurnOff = false;
	}
	public void SetRouletteRequest()
	{
		Request.Roulette = true;
		setRouletteTimer();//ルーレット時間の設定
		RouletteIsReadyOk = false;//ルーレット要求禁止(RouletteStockControllerから待ち状態が終わるまでは要求できないようにするため)
	}
	public void ClearRouletteRequest()
	{
		Request.Roulette = false;
	}
	private void setWaitRequest()
	{
		Request.Wait = true;
		WaitTimer = TIME_WAIT;//待ち時間の設定
	}
	private void clearWaitRequest()
	{
		Request.Wait = false;
	}
	public ACTION_PATTERN GetActionState()
	{
		ACTION_PATTERN ret = 0;

		ret = MasuInfo[0].ActionState;//代表して0のを参照する。←そういえばActionStateってMasuInfoに持たせる必要ないよね？？

		return ret;
	}
	public void IncrementExcludedMasuCounter()
	{
		if (ExcludedMasuCounter < NUM_EXCLUDE_MAX)
		{
			ExcludedMasuCounter++;
			settingDisplayStateByExclude();
		}
	}
	private void clearExcludedMasuCounter()
	{
		ExcludedMasuCounter = 0;
		resetDisplayStateByExclude();//除外用の表示状態を解除
	}
	public bool GetRouletteIsReadyOk()
	{
		return RouletteIsReadyOk;
	}
	private string getRouletteResultMasuName()
	{	/* ルーレット結果のマス名(OnMasu名)を返す	*/
		string ret = "none";//FSのためnoneにしている

		for(int masu = 0; masu<NUM_MASU_MAX;masu++)
		{
			if(MasuInfo[masu].DisplayState == ON)
			{
				ret = MasuInfo[masu].NameOnObject.name;
			}
		}

		return ret;
	}
	//==================================================//
	/* ルーレット時間の設定								*/
	//==================================================//
	private void setRouletteTimer()
	{
		uint randomTime = (uint)Random.Range(TIME_RANDOM_MIN, TIME_RANDOM_MAX);//ランダム値の決定
		RouletteTimer = randomTime;//タイマーセット
	}
	private void setTurnOnTimer()
	{
		TurnOnTimer = TIME_TURN_ON;
	}
	//==================================================//
	/* 除外用の表示設定									*/
	//==================================================//
	private void resetDisplayStateByExclude()//除外用の表示状態を解除する
	{
		for(int masu = NUM_MASU_FIRST; masu<NUM_MASU_MAX; masu++)
		{
			MasuInfo[masu].NameExcludeObject.SetActive(false);//除外用planeを非表示(この処理も下のif文に入れてしまうと初期化時に非表示にできない)
			MasuInfo[masu].Excluded = false;//除外を解除
			if (MasuInfo[masu].Excluded == true)//除外されてるマスなら(限定しないと全マスを消灯にしてしまいルーレットで止まったマスも消灯になってしまう)
			{
				MasuInfo[masu].DisplayState = false;//除外されてたマスを消灯に戻す
			}
			else//除外されてるマスじゃないなら
			{
				//なにもしない
			}
			setActiveByMasuDisplayState(masu);//出力
		}
	}
	private void settingDisplayStateByExclude()//除外による表示状態の設定
	{
		for (int masu = NUM_MASU_FIRST; masu < NUM_MASU_MAX; masu++)
		{
			string name = MasuInfo[masu].NameOnObject.name;

			/* 除外する(表示状態を更新するマス)を検索し、表示状態の設定をする	*/
			switch (ExcludedMasuCounter)
			{
				case 0:
					/* 除外マスカウンタが0 = マスの除外なし	*/
					break;
				case 1:
					/* 除外マスカウンタが1なら1すすむマスを除外	*/
					if (name == "1StepON")//ON、OFFどちらでもいい(1すすむマスだと判別できれば)
					{
						outputByExcludeDsiplayState(masu);//表示状態の反映
					}
					break;
				case 2:
					/* 除外マスカウンタが2なら2すすむマスも除外	*/
					if ((name == "1StepON") || (name == "2StepON"))
					{
						outputByExcludeDsiplayState(masu);//表示状態の反映
					}
					break;
				case 3:
					/* 除外マスカウンタが3ならコイン50イベントストック+1マスも除外	*/
					if ((name == "1StepON") || (name == "2StepON")
						|| (name == "Coin50EventStock+1ON"))
					{
						outputByExcludeDsiplayState(masu);//表示状態の反映
					}
					break;
				case 4:
					/* 除外マスカウンタが4ならコイン20イベントストック+1マスも除外	*/
					if ((name == "1StepON") || (name == "2StepON")
						|| (name == "Coin50EventStock+1ON") || (name == "Coin20EventStock+1ON"))
					{
						outputByExcludeDsiplayState(masu);//表示状態の反映
					}
					break;
				case 5:
					/* 除外マスカウンタが5ならコイン10イベントストック+1マスも除外	*/
					if ((name == "1StepON") || (name == "2StepON")
						|| (name == "Coin50EventStock+1ON") || (name == "Coin20EventStock+1ON")
						|| (name == "Coin10EventStock+1ON"))
					{
						outputByExcludeDsiplayState(masu);//表示状態の反映
					}
					break;
				default:    //それ以外の値のときは何もしない
					break;
			}
		}
	}
	private void outputByExcludeDsiplayState(int masu)//除外による表示状態の反映
	{
		MasuInfo[masu].DisplayState = OFF;
		MasuInfo[masu].NameExcludeObject.SetActive(true);//除外用planeを表示
		MasuInfo[masu].NameOnObject.SetActive(false);//点灯用planeを非表示
		MasuInfo[masu].NameOffObject.SetActive(false);//消灯用planeを非表示
	}
}
