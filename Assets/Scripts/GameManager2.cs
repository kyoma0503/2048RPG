using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager2 : MonoBehaviour
{
    //マスのサイズ
    int EDGE = 4;

    //マスの処理に必要な要素を定義
    public int[,] MassNum = new int[4, 4];
    public int[,] constMergeNum = new int[4, 4];
    public List<int>[,] MergeNum = new List<int>[4, 4];
    public bool[,] isMergedHere = new bool[4, 4];
    public Vector2[,] MassPos = new Vector2[4, 4];

    //ブロックの移動時間
    public float MoveTime = 0.15f;

    //生成するブロックとその親となるオブジェクトを定義
    public GameObject Blocks;
    public GameObject Block;

    //使用するフラグを定義
    public bool isAddNum = false;
    public bool isActioned = false;

    //リソース開放の
    public int resouceCounter = 0;
    [SerializeField] int ReleaseSpan = 15;

    void Start()
    {
        //ランダム関数の初期化
        Random.InitState(System.DateTime.Now.Millisecond);

        //盤面などの初期設定
        InitIsMergedFlag();
        InitMassPos();
        InitMassNum();
        InitMergeNum();

        //ブロックを生成してゲームスタート
        CreateNum();
    }

    void Update()
    {

        if (isActioned) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            UpArrow();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DownArrow();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            LeftArrow();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RightArrow();
        }
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
        for(int i = 0; i < EDGE; i++)
        {
            for(int j = 0; j < EDGE; j++)
            {
                switch (i)
                {
                    case 0:
                        MassPos[i,j].y = 3;
                        break;
                    case 1:
                        MassPos[i, j].y = 1.5f;
                        break;
                    case 2:
                        MassPos[i, j].y = 0;
                        break;
                    case 3:
                        MassPos[i, j].y = -1.5f;
                        break;
                }

                switch (j)
                {
                    case 0:
                        MassPos[i, j].x = -2;
                        break;
                    case 1:
                        MassPos[i, j].x = -0.66f;
                        break;
                    case 2:
                        MassPos[i, j].x = 0.66f;
                        break;
                    case 3:
                        MassPos[i, j].x = 2;
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

    //ブロックを生成してゲームをスタートする関数
    void CreateNum()
    {
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if(MassNum[i, j] != 0)
                {
                    StartCoroutine(CreateBlock(i, j));
                }
            }
        }
    }


    void UpArrow()
    {
        //行動フラグをON
        isActioned = true;

        //マージに関する要素を初期化
        InitIsMergedFlag();
        InitMergeNum();

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

        //実際のブロックの移動
        GoPos();

        //マス追加フラグがONならマスを追加
        if (isAddNum) CreateNewNum();

        //リソース開放
        RecouceRelease();

        //処理完了後にフラグをOFF
        StartCoroutine(ActionedFlagOff());
    }

    void DownArrow()
    {
        //行動フラグをON
        isActioned = true;

        //マージに関する要素を初期化
        InitIsMergedFlag();
        InitMergeNum();

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

        //実際のブロックの移動
        GoPos();

        //マス追加フラグがONならマスを追加
        if (isAddNum) CreateNewNum();

        //リソース開放
        RecouceRelease();

        //処理完了後にフラグをOFF
        StartCoroutine(ActionedFlagOff());
    }

    void LeftArrow()
    {
        //行動フラグをON
        isActioned = true;

        //マージに関する要素を初期化
        InitIsMergedFlag();
        InitMergeNum();

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

        //実際のブロックの移動
        GoPos();

        //マス追加フラグがONならマスを追加
        if (isAddNum) CreateNewNum();

        //リソース開放
        RecouceRelease();

        //処理完了後にフラグをOFF
        StartCoroutine(ActionedFlagOff());
    }

    void RightArrow()
    {
        //行動フラグをON
        isActioned = true;

        //マージに関する要素を初期化
        InitIsMergedFlag();
        InitMergeNum();

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

        //実際のブロックの移動
        GoPos();

        //マス追加フラグがONならマスを追加
        if (isAddNum) CreateNewNum();

        //リソース開放
        RecouceRelease();

        //処理完了後にフラグをOFF
        StartCoroutine(ActionedFlagOff());
    }

    //実際にブロックを移動させる関数
    void GoPos()
    {
        //いま存在しているブロックに対して処理を実行
        foreach (Transform n in Blocks.transform)
        {
            var thisMass = n.GetComponent<Mass>();
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
                thisMass.MoveTo(pos, MoveTime, isMerged);

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
                thisMass.MoveTo(pos, MoveTime, isMerged);
            }
        }
    }

    //ブロックの移動完了後にブロックを生成するコルーチン
    IEnumerator MakeNowSence(int x, int y)
    {
        yield return new WaitForSeconds(MoveTime);

        StartCoroutine(CreateBlock(x, y));
    }

    //行動終了後に行動フラグを解除するコルーチン
    IEnumerator ActionedFlagOff()
    {
        //ブロックの移動時間
        yield return new WaitForSeconds(MoveTime);

        //マージが実行される時間
        yield return new WaitForSeconds(0.1f);

        //行動フラグを解除
        isActioned = false;
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
        StartCoroutine(CreateBlock(x, y));
    }

    //指定場所にブロックを生成するコルーチン
    IEnumerator CreateBlock(int x, int y)
    {
        //ブロックを指定場所に生成してスクリプトを確保
        var thisBlock = Instantiate(Block, MassPos[x, y], Quaternion.identity, Blocks.transform);
        var myMass = thisBlock.GetComponent<Mass>();

        //それぞれの要素を更新して表示
        myMass.myNum = MassNum[x, y];
        myMass.myMergeNum = constMergeNum[x, y];
        myMass.TextUpdate();

        //徐々に大きくなるモーションを実行
        thisBlock.transform.localScale = Vector2.zero;
        for(int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.02f);
            thisBlock.transform.localScale = new Vector2(0.2f * i, 0.2f * i);
        }
    }

    //一定行動を実行したらリソースの開放を行う関数
    void RecouceRelease()
    {
        if(resouceCounter == ReleaseSpan)
        {
            Resources.UnloadUnusedAssets();
            resouceCounter = 0;
        }
        else
        {
            resouceCounter += 1;
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("MoveGame");
    }
}
