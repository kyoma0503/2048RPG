using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    //マスのサイズ
    int EDGE = 4;

    //PlayerとEnemyのクラス
    public Test Player;
    public Test Enemy;

    //PlayerとEnemyのHP
    public int PlayerHP = 6000;
    public Slider PlayerHPSlider;
    public int EnemyHP = 6000;
    public Slider EnemyHPSlider;

    //マスの処理に必要な要素を定義
    public int[,] MassNum = new int[4, 4];
    public int[,] constMergeNum = new int[4, 4];
    public List<int>[,] MergeNum = new List<int>[4, 4];
    public bool[,] isMergedHere = new bool[4, 4];
    public Vector2[,] MassPos = new Vector2[4, 4];

    //ブロックの移動時間
    public static int MotionTime = 3;

    //生成するブロックとその親となるオブジェクトを定義
    public GameObject Blocks;

    //使用するフラグを定義
    bool isAddNum = false;
    public bool isActioned = false;
    public bool isAuto = false;

    //リソース開放のための要素
    int resouceCounter = 0;
    int ReleaseSpan = 1000;

    //ゲームクリア状態の判定
    public int GameState = 0;//0:ゲーム中、1:ゲームクリア、2:ゲームオーバー
    [SerializeField] int TargetScore = 2048;

    //フリック処理
    float FlickDetectionNum = 30;
    Vector2 touchStartPos;
    Vector2 touchNowPos;
    string touchDirection = "";
    bool isTouch = false;

    //行動のスタック処理
    public List<string> ActionStuck = new List<string>();
    [SerializeField] int MaxStuckNum = 5;

    //敵の行動ターンのカウンター
    int enemyTurnCounter = 0;
    [SerializeField] int EnemyTurnSpan = 100;

    //ディスプレイ用
    public Text GameClearText;

    void Start()
    {
        //フレームレートの固定化
        Application.targetFrameRate = 30;

        //ランダム関数の初期化
        Random.InitState(System.DateTime.Now.Millisecond);

        Player = GameObject.FindWithTag("Player").GetComponent<Test>();
        Enemy = GameObject.FindWithTag("Enemy").GetComponent<Test>();

        //盤面などの初期設定
        InitIsMergedFlag();
        InitMassPos();
        InitMassNum();
        InitMergeNum();
        AllBlocksDisappear();
        InitHPSlider();

        //ブロックを生成してゲームスタート
        CreateNum();
    }

    void Update()
    {
        //フリックまたはキーボードでの入力を格納
        FlickCheck();
        InputArrowCheck();

        //行動のアニメーションが止まっているかどうかを判定
        AnimationFlagCkeck();

        //入力に従って移動アクションを実行
        MoveAction();

        //敵のターンを処理する関数
        EnemyTurn();
    }

    //フリックされたかの判定
    void FlickCheck()
    {
        //画面がタップされたとき、タップ位置を記録してタップフラグをON
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isTouch = true;
        }

        //画面から指が離れたとき、タップフラグを解除
        if (Input.GetMouseButtonUp(0))
        {
            isTouch = false;
        }

        //タップ中は指の位置を検出し続けて、差が大きければフリック判定
        if (isTouch)
        {
            touchNowPos = Input.mousePosition;
            GetDirection();
        }
    }

    //タップの方向を検出する関数
    void GetDirection()
    {
        //タップの開始位置と現在位置の差を取得
        float directionX = touchNowPos.x - touchStartPos.x;
        float directionY = touchNowPos.y - touchStartPos.y;

        //横方向の移動が縦方向より大きい場合
        if (Mathf.Abs(directionY) < Mathf.Abs(directionX))
        {
            if (directionX > FlickDetectionNum) touchDirection = "right";
            if (directionX < -FlickDetectionNum) touchDirection = "left";
        }
        //縦方向の移動が横方向より大きい場合
        else if (Mathf.Abs(directionY) > Mathf.Abs(directionX))
        {
            if (directionY > FlickDetectionNum) touchDirection = "up";
            if (directionY < -FlickDetectionNum) touchDirection = "down";
        }
        //移動がなかった場合
        else
        {
            touchDirection = "touch";
        }

        //フリックでない場合は何もしない
        if (touchDirection == "" || touchDirection == "touch") return;

        //オートモードの場合は何もしない
        if (isAuto) return;

        //フリックされていた場合は、スタック上限に達していなければスタックに格納
        if (ActionStuck.Count<MaxStuckNum) ActionStuck.Add(touchDirection);

        //タッチフラグを解除
        isTouch = false;

        //フリック入力を解除
        touchDirection = "";
    }

    //入力に従って移動アクションを実行する関数
    void MoveAction()
    {
        //行動中なら何もしない
        if (isActioned) return;

        //行動スタックに何もなければ何もしない
        if (ActionStuck.Count == 0) return;

        //ゲームステータスが0以外なら何もしない
        if (GameState != 0) return;

        //行動フラグをONに切り替え
        isActioned = true;

        //マージに関する要素を初期化
        InitIsMergedFlag();
        InitMergeNum();

        //行動スタックの一番古い入力に従い行動を実行
        switch (ActionStuck[0])
        {
            case "up":
                UpArrow();
                break;
            case "down":
                DownArrow();
                break;
            case "left":
                LeftArrow();
                break;
            case "right":
                RightArrow();
                break;
        }

        //実行済みの入力を削除
        ActionStuck.RemoveAt(0);
    }

    //キーボード入力でスタック受付する関数(デバッグ用)
    void InputArrowCheck()
    {
        string inputDirection = "";

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            inputDirection = "up";
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            inputDirection = "down";
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            inputDirection = "left";
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            inputDirection = "right";
        }

        //オートモードの場合は何もしない
        if (isAuto) return;

        //上下左右の矢印キーの入力があり、スタック上限に達していなければ行動スタックに格納
        if (inputDirection != "" && ActionStuck.Count < MaxStuckNum) ActionStuck.Add(inputDirection);
    }

    //行動が終了したかどうかを判定する関数
    void AnimationFlagCkeck()
    {
        //行動中でなければ何もしない
        if (!isActioned) return;

        //移動中もしくは誕生中のブロックがいる場合は何もしない
        foreach(Transform n in Blocks.transform)
        {
            var myMass = n.GetComponent<Mass3>();
            if (myMass.isMoving || myMass.isBirthing) return;
        }

        //すべて移動・誕生していない場合は行動フラグを解除
        isActioned = false;
    }

    //マージされたかどうかのフラグの初期化
    void InitIsMergedFlag()
    {
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                isMergedHere[i, j] = false;
            }
        }
    }

    //ブロックを生成する位置の初期化
    private void InitMassPos()
    {
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                switch (i)
                {
                    case 0:
                        MassPos[i, j].y = 40;
                        break;
                    case 1:
                        MassPos[i, j].y = -107;
                        break;
                    case 2:
                        MassPos[i, j].y = -253;
                        break;
                    case 3:
                        MassPos[i, j].y = -400;
                        break;
                }

                switch (j)
                {
                    case 0:
                        MassPos[i, j].x = -220;
                        break;
                    case 1:
                        MassPos[i, j].x = -73;
                        break;
                    case 2:
                        MassPos[i, j].x = 73;
                        break;
                    case 3:
                        MassPos[i, j].x = 220;
                        break;
                }
            }
        }
    }

    //マスの数字を初期化
    void InitMassNum()
    {
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                MassNum[i, j] = 0;
                constMergeNum[i, j] = i * 4 + j;
                MergeNum[i, j] = new List<int>();
            }
        }

        int x1 = Random.Range(0, EDGE);
        int y1 = Random.Range(0, EDGE);

        MassNum[x1, y1] = NewNumGenerator();

        int x2;
        int y2;

        while (true)
        {
            x2 = Random.Range(0, EDGE);
            y2 = Random.Range(0, EDGE);

            if (x1 != x2 || y1 != y2) break;
        }

        MassNum[x2, y2] = NewNumGenerator();
    }

    //2か4をランダムで生成する関数
    int NewNumGenerator()
    {
        int rnd = Random.Range(0, 4);

        if (rnd == 3) return 4;
        else return 2;
    }

    //マージされた数字を格納する配列を初期化する関数
    void InitMergeNum()
    {
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                MergeNum[i, j].Clear();
                MergeNum[i, j].Add(constMergeNum[i, j]);
            }
        }
    }

    //すべてのブロックを非表示にする関数
    void AllBlocksDisappear()
    {
        foreach(Transform n in Blocks.transform)
        {
            n.gameObject.SetActive(false);
            n.GetComponent<Mass3>().isDisplayed = false;
        }
    }

    //HPスライダーを初期化する関数
    void InitHPSlider()
    {
        PlayerHPSlider.maxValue = PlayerHP;
        PlayerHPSlider.value = PlayerHP;
        EnemyHPSlider.maxValue = EnemyHP;
        EnemyHPSlider.value = EnemyHP;
    }

    //ブロックを生成してゲームをスタートする関数
    void CreateNum()
    {
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[i, j] != 0)
                {
                    CreateBlock(i, j);
                }
            }
        }
    }


    void UpArrow()
    {
        //マスを追加すうかどうかのフラグを初期化
        isAddNum = false;

        //2行目から下に向かって順に0なら移動を行う
        for (int loop = 0; loop < EDGE - 1; loop++)
        {
            for (int i = 1; i < EDGE; i++)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[i - 1, j] == 0 && MassNum[i, j] != 0)
                    {
                        isAddNum = true;

                        MassNum[i - 1, j] = MassNum[i, j];
                        MassNum[i, j] = 0;

                        MergeNum[i - 1, j].AddRange(MergeNum[i, j]);
                        MergeNum[i, j].Clear();
                    }
                }
            }
        }

        //2行目から下に向かって順に同じ数字なら加算を行う
        for (int i = 1; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[i, j] == MassNum[i - 1, j] && MassNum[i, j] != 0)
                {
                    isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[i, j] += MassNum[i - 1, j];
                    MergeNum[i - 1, j].Add(100);

                    for (int loop = i; loop < EDGE; loop++)
                    {
                        MassNum[loop - 1, j] = MassNum[loop, j];
                        MassNum[loop, j] = 0;

                        MergeNum[loop - 1, j].AddRange(MergeNum[loop, j]);
                        MergeNum[loop, j].Clear();
                    }
                }
            }
        }

        //行動終わりの処理を実行
        ActionEnd();
    }

    void DownArrow()
    {
        //マスを追加すうかどうかのフラグを初期化
        isAddNum = false;

        //3行目から上に向かって順に0なら移動を行う
        for (int loop = 0; loop < EDGE - 1; loop++)
        {
            for (int i = EDGE - 2; i >= 0; i--)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[i + 1, j] == 0 && MassNum[i, j] != 0)
                    {
                        isAddNum = true;

                        MassNum[i + 1, j] = MassNum[i, j];
                        MassNum[i, j] = 0;

                        MergeNum[i + 1, j].AddRange(MergeNum[i, j]);
                        MergeNum[i, j].Clear();
                    }
                }
            }
        }

        //3行目から上に向かって順に同じ数字なら加算を行う
        for (int i = EDGE - 2; i >= 0; i--)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[i, j] == MassNum[i + 1, j] && MassNum[i, j] != 0)
                {
                    isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[i, j] += MassNum[i + 1, j];
                    MergeNum[i + 1, j].Add(100);

                    for (int loop = i; loop >= 0; loop--)
                    {
                        MassNum[loop + 1, j] = MassNum[loop, j];
                        MassNum[loop, j] = 0;

                        MergeNum[loop + 1, j].AddRange(MergeNum[loop, j]);
                        MergeNum[loop, j].Clear();
                    }
                }
            }
        }

        //行動終わりの処理を実行
        ActionEnd();
    }

    void LeftArrow()
    {
        //マスを追加すうかどうかのフラグを初期化
        isAddNum = false;

        //2列目から右に向かって順に0なら移動を行う
        for (int loop = 0; loop < EDGE - 1; loop++)
        {
            for (int i = 1; i < EDGE; i++)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[j, i - 1] == 0 && MassNum[j, i] != 0)
                    {
                        isAddNum = true;

                        MassNum[j, i - 1] = MassNum[j, i];
                        MassNum[j, i] = 0;

                        MergeNum[j, i - 1].AddRange(MergeNum[j, i]);
                        MergeNum[j, i].Clear();
                    }
                }
            }
        }

        //2列目から右に向かって順に同じ数字なら加算を行う
        for (int i = 1; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[j, i] == MassNum[j, i - 1] && MassNum[j, i] != 0)
                {
                    isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[j, i] += MassNum[j, i - 1];
                    MergeNum[j, i - 1].Add(100);

                    for (int loop = i; loop < EDGE; loop++)
                    {
                        MassNum[j, loop - 1] = MassNum[j, loop];
                        MassNum[j, loop] = 0;

                        MergeNum[j, loop - 1].AddRange(MergeNum[j, loop]);
                        MergeNum[j, loop].Clear();
                    }
                }
            }
        }

        //行動終わりの処理を実行
        ActionEnd();
    }

    void RightArrow()
    {
        //マスを追加すうかどうかのフラグを初期化
        isAddNum = false;

        //3列目から左に向かって順に0なら移動を行う
        for (int loop = 0; loop < EDGE - 1; loop++)
        {
            for (int i = EDGE - 2; i >= 0; i--)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[j, i + 1] == 0 && MassNum[j, i] != 0)
                    {
                        isAddNum = true;

                        MassNum[j, i + 1] = MassNum[j, i];
                        MassNum[j, i] = 0;

                        MergeNum[j, i + 1].AddRange(MergeNum[j, i]);
                        MergeNum[j, i].Clear();
                    }
                }
            }
        }

        //3列目から左に向かって順に同じ数字なら加算を行う
        for (int i = EDGE - 2; i >= 0; i--)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[j, i] == MassNum[j, i + 1] && MassNum[j, i] != 0)
                {
                    isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[j, i] += MassNum[j, i + 1];
                    MergeNum[j, i + 1].Add(100);

                    for (int loop = i; loop >= 0; loop--)
                    {
                        MassNum[j, loop + 1] = MassNum[j, loop];
                        MassNum[j, loop] = 0;

                        MergeNum[j, loop + 1].AddRange(MergeNum[j, loop]);
                        MergeNum[j, loop].Clear();
                    }
                }
            }
        }

        //行動終わりの処理を実行
        ActionEnd();
    }

    //行動の終わりに実行される関数
    void ActionEnd()
    {
        //実際のブロックの移動
        GoPos();

        //マス追加フラグがONならマスを追加
        if (isAddNum) CreateNewNum();

        //リソース開放
        RecouceRelease();

        //ゲームクリアまたはゲームオーバーのチェック
        GameCheck();
    }

    //実際にブロックを移動させる関数
    void GoPos()
    {
        //いま存在しているブロックに対して処理を実行
        foreach (Transform n in Blocks.transform)
        {
            var thisMass = n.GetComponent<Mass3>();

            if (thisMass.isDisplayed)
            {
                var myMergeNum = thisMass.myMergeNum;

                int x = 0;
                int y = 0;
                bool isMerged = false;

                for (int i = 0; i < EDGE; i++)
                {
                    for (int j = 0; j < EDGE; j++)
                    {
                        //自分のマージ番号が含まれている場所を確認
                        if (MergeNum[i, j].Contains(myMergeNum))
                        {
                            x = i;
                            y = j;

                            //要素に100が含まれている場合はマージフラグをON
                            if (MergeNum[i, j].Contains(100)) isMerged = true;
                        }
                    }
                }

                //移動先を確定
                Vector2 pos = MassPos[x, y];

                //マージされている場合は移動を実行して合計のブロックを生成
                if (isMerged)
                {
                    //移動を実行
                    thisMass.MoveTo(pos, isMerged);

                    //重複を避けるため1度だけブロック生成を実行
                    if (!isMergedHere[x, y])
                    {
                        isMergedHere[x, y] = true;
                        StartCoroutine(MakeNowSence(x, y));
                    }
                }
                //移動だけ行われている場合は、マージ番号を更新して移動のみ実行
                else if (constMergeNum[x, y] != myMergeNum)
                {
                    //マージ番号の更新
                    thisMass.myMergeNum = constMergeNum[x, y];

                    //移動を実行
                    thisMass.MoveTo(pos, isMerged);
                }
            }
        }
    }

    //ブロックの移動完了後にブロックを生成するコルーチン
    IEnumerator MakeNowSence(int x, int y)
    {
        for (int i = 0; i < MotionTime; i++) yield return null;
        yield return new WaitForEndOfFrame();

        //ブロックを生成
        CreateBlock(x, y);

        //HPにダメージを付与
        DamageAttack(MassNum[x, y]);
    }

    //HPにダメージを付与する関数
    void DamageAttack(int num)
    {
        //64以下の攻撃を小攻撃としてモーション
        if (num <= 64)
        {
            Player.Attack();
            Enemy.Damage_s();
        }
        //64より大きい攻撃を大攻撃としてモーション
        else
        {
            Player.StrongAttack();
            Enemy.Damage_l();
        }

        //num数分だけEnemyにダメージを付与
        EnemyHP -= num;
        EnemyHPSlider.value = EnemyHP;

        //HPが0以下の場合は死亡処理
        if (EnemyHP <= 0)
        {
            //ゲームステータスを変更
            GameState = 3;

            //ゲームクリア処理
            GameClear();
        }
    }

    //新たなマスを追加してブロックを生成する関数
    void CreateNewNum()
    {
        //int配列を格納するリストを作成
        List<int[]> zeroList = new List<int[]>();

        //マスが0である場所を追加
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[i, j] == 0)
                {
                    int[] Array = { i, j };
                    zeroList.Add(Array);
                }
            }
        }

        //ランダムに抽選を実施
        int rnd = Random.Range(0, zeroList.Count);
        int x = zeroList[rnd][0];
        int y = zeroList[rnd][1];

        //マスを生成
        MassNum[x, y] = NewNumGenerator();

        //ブロックを生成
        CreateBlock(x, y);
    }

    //指定場所にブロックを生成するコルーチン
    void CreateBlock(int x, int y)
    {
        //現在、非表示になっているBlockを一つ取得
        GameObject thisBlock = null;
        foreach(Transform n in Blocks.transform)
        {
            if (!n.GetComponent<Mass3>().isDisplayed)
            {
                thisBlock = n.gameObject;
                n.GetComponent<Mass3>().isDisplayed = true;
                break;
            }
        }

        //ブロックを指定位置に表示
        thisBlock.SetActive(true);
        thisBlock.transform.localPosition = MassPos[x, y];

        //それぞれの要素を更新して表示
        var myMass = thisBlock.GetComponent<Mass3>();
        myMass.myNum = MassNum[x, y];
        myMass.myMergeNum = constMergeNum[x, y];
        myMass.TextUpdate();

        //ブロックの表示アニメーションを実行
        myMass.Birth();
    }

    //一定行動を実行したらリソースの開放を行う関数
    void RecouceRelease()
    {
        if (resouceCounter == ReleaseSpan)
        {
            Resources.UnloadUnusedAssets();
            resouceCounter = 0;
        }
        else
        {
            resouceCounter += 1;
        }
    }

    //ゲームクリアかゲームオーバーを判定する関数
    void GameCheck()
    {
        int cantMoveCount = 0;  // 移動できないマス数
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                // クリアチェック
                if (MassNum[i, j] == TargetScore && GameState == 0) GameState = 1;

                // 上右下左の順にチェック
                bool[] checkNext = new bool[4];
                if (i - 1 < 0 || MassNum[i - 1, j] != 0 && MassNum[i - 1, j] != MassNum[i, j]) checkNext[0] = true;
                if (j + 1 >= EDGE || MassNum[i, j + 1] != 0 && MassNum[i, j + 1] != MassNum[i, j]) checkNext[1] = true;
                if (i + 1 >= EDGE || MassNum[i + 1, j] != 0 && MassNum[i + 1, j] != MassNum[i, j]) checkNext[2] = true;
                if (j - 1 < 0 || MassNum[i, j - 1] != 0 && MassNum[i, j - 1] != MassNum[i, j]) checkNext[3] = true;

                // どの方向にも移動できないマスを数える
                if (checkNext[0] && checkNext[1] && checkNext[2] && checkNext[3])
                    cantMoveCount++;
            }
        }

        // 移動できないマス数が辺×辺(16マス)ならゲームオーバー
        if (cantMoveCount == EDGE * EDGE) GameState = 2;

        // ゲームクリア
        if (GameState == 1)
        {
            GameClearText.text = "2048を達成!";
            GameClearText.gameObject.SetActive(true);
        }
        // ゲームオーバー
        else if (GameState == 2)
        {
            //ゲームオーバー処理
            GameOver();
        }
    }

    //敵の攻撃を処理する関数
    void EnemyTurn()
    {
        //ゲームステータスが0以外なら何もしない
        if (GameState != 0) return;

        //カウンターに加算
        enemyTurnCounter += 1;

        //カウンターが目標値に達していなければ何もしない
        if (enemyTurnCounter < EnemyTurnSpan) return;

        //ここからエネミーの行動

        //カウンターを0に戻す
        enemyTurnCounter = 0;

        //ダメージ値を設定
        int Damage = 50;

        //小攻撃・大攻撃をランダムに決定
        float r = Random.value;
        if (r < 0.5f)
        {
            Enemy.Attack();
            Player.Damage_s();
        }
        else
        {
            Damage = 100;
            Enemy.StrongAttack();
            Player.Damage_l();
        }

        //Playerにダメージを付与
        PlayerHP -= Damage;
        PlayerHPSlider.value = PlayerHP;

        //HPが0以下の場合はゲームオーバー処理
        if (PlayerHP <= 0)
        {
            //ゲームステータスを変更
            GameState = 4;

            //ゲームオーバー処理
            GameOver();
        }
    }

    //ゲームクリアを処理する関数
    void GameClear()
    {
        //勝利、死亡モーションをそれぞれ実行
        Player.HangOn();
        Enemy.Dead();

        //ゲームクリア表示
        GameClearText.text = "GameClear!";
        GameClearText.transform.localPosition = new Vector2(0, -175);
        GameClearText.gameObject.SetActive(true);
    }

    //ゲームオーバーを処理する関数
    void GameOver()
    {
        //勝利、死亡モーションをそれぞれ実行
        Enemy.HangOn();
        Player.Dead();

        //ゲームオーバーUIを表示
        GameClearText.text = "GameOver";
        GameClearText.transform.localPosition = new Vector2(0, -175);
        GameClearText.gameObject.SetActive(true);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("BattleScene");
    }

}
