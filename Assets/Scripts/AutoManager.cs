using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class AutoManager : MonoBehaviour
{
    //オートモードを制御するスクリプト

    BattleManager myBattleManager;
    public GameObject AutoButton;

    void Start()
    {
        myBattleManager = GameObject.FindWithTag("BattleManager").GetComponent<BattleManager>();

        //オート状態の初期化
        InitAutoMode();
    }

    void Update()
    {
        //オートモードの実行
        AutoRun();
    }

    //オート状態を初期化する関数
    void InitAutoMode()
    {
        myBattleManager.isAuto = false;
        AutoUpdate();
    }

    //オートモードを実行する関数
    void AutoRun()
    {
        //isAutoフラグがOFFなら何もしない
        if (!myBattleManager.isAuto) return;

        //アクション実行中なら何もしない
        if (myBattleManager.isActioned) return;

        //アクションスタックに何かあるなら何もしない
        if (myBattleManager.ActionStuck.Count > 0) return;

        //アクションスタックに何もなければ、AIから手法をGET
        string method = GetHand();

        //AIが何も返さなければゲームオーバーなので何もしない
        if (method == "") return;

        //AIから答えが返ってきた場合は、実行
        myBattleManager.ActionStuck.Add(method);
    }

    //オートモードのON/OFFを切り替える関数
    public void ChangeAuto()
    {
        myBattleManager.isAuto = !myBattleManager.isAuto;
        AutoUpdate();
    }

    //オート状態の表示を更新する関数
    void AutoUpdate()
    {
        Text autoText = AutoButton.transform.Find("Text").GetComponent<Text>();

        if (myBattleManager.isAuto) autoText.text = "Auto:ON";
        else autoText.text = "Auto:OFF";
    }


    //上下方向のどの手が最も良いかを判断する関数(1手先読み)
    public string GetResult()
    {
        //配列の用意
        int[,] UpMass = new int[4, 4];
        int[,] LeftMass = new int[4, 4];
        int[,] RightMass = new int[4, 4];
        int[,] DownMass = new int[4, 4];

        //現在の盤面をコピー
        Array.Copy(myBattleManager.MassNum, 0, UpMass, 0, 16);
        Array.Copy(UpMass, 0, LeftMass, 0, 16);
        Array.Copy(UpMass, 0, RightMass, 0, 16);
        Array.Copy(UpMass, 0, DownMass, 0, 16);

        //変数を初期化
        string result = "";

        bool isAddNum = false;
        int maxScore = -1;
        int score = 0;

        ////上方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (UpMass[i - 1, j] == 0 && UpMass[i, j] != 0)
                    {
                        isAddNum = true;

                        UpMass[i - 1, j] = UpMass[i, j];
                        UpMass[i, j] = 0;
                    }
                }
            }
        }

        for (int i = 1; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (UpMass[i, j] == UpMass[i - 1, j] && UpMass[i, j] != 0)
                {
                    isAddNum = true;

                    UpMass[i, j] += UpMass[i - 1, j];
                    score += UpMass[i, j];//スコア加算

                    for (int loop = i; loop < 4; loop++)
                    {
                        UpMass[loop - 1, j] = UpMass[loop, j];
                        UpMass[loop, j] = 0;
                    }
                }
            }
        }
        ////上方向ここまで////

        //移動があり、かつスコアを更新していれば上書き
        if (isAddNum && score > maxScore)
        {
            result = "up";
            maxScore = score;
        }

        //変数の初期化
        score = 0;
        isAddNum = false;

        ////左方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (LeftMass[j, i - 1] == 0 && LeftMass[j, i] != 0)
                    {
                        isAddNum = true;

                        LeftMass[j, i - 1] = LeftMass[j, i];
                        LeftMass[j, i] = 0;
                    }
                }
            }
        }

        for (int i = 1; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (LeftMass[j, i] == LeftMass[j, i - 1] && LeftMass[j, i] != 0)
                {
                    isAddNum = true;

                    LeftMass[j, i] += LeftMass[j, i - 1];
                    score += LeftMass[j, i];//スコア加算

                    for (int loop = i; loop < 4; loop++)
                    {
                        LeftMass[j, loop - 1] = LeftMass[j, loop];
                        LeftMass[j, loop] = 0;
                    }
                }
            }
        }
        ////左方向ここまで////

        //移動があり、かつスコアを更新していれば上書き
        if (isAddNum && score > maxScore)
        {
            result = "left";
            maxScore = score;
        }

        //変数の初期化
        score = 0;
        isAddNum = false;

        ////右方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 4 - 2; i >= 0; i--)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (RightMass[j, i + 1] == 0 && RightMass[j, i] != 0)
                    {
                        isAddNum = true;

                        RightMass[j, i + 1] = RightMass[j, i];
                        RightMass[j, i] = 0;
                    }
                }
            }
        }

        for (int i = 4 - 2; i >= 0; i--)
        {
            for (int j = 0; j < 4; j++)
            {
                if (RightMass[j, i] == RightMass[j, i + 1] && RightMass[j, i] != 0)
                {
                    isAddNum = true;

                    RightMass[j, i] += RightMass[j, i + 1];
                    score += RightMass[j, i];//スコア加算

                    for (int loop = i; loop >= 0; loop--)
                    {
                        RightMass[j, loop + 1] = RightMass[j, loop];
                        RightMass[j, loop] = 0;
                    }
                }
            }
        }
        ////右方向ここまで////
        
        //移動があり、かつスコアを更新していれば上書き
        if (isAddNum && score > maxScore)
        {
            result = "right";
            maxScore = score;
        }

        //変数の初期化
        score = 0;
        isAddNum = false;

        ////下方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 4 - 2; i >= 0; i--)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (DownMass[i + 1, j] == 0 && DownMass[i, j] != 0)
                    {
                        isAddNum = true;

                        DownMass[i + 1, j] = DownMass[i, j];
                        DownMass[i, j] = 0;
                    }
                }
            }
        }

        for (int i = 4 - 2; i >= 0; i--)
        {
            for (int j = 0; j < 4; j++)
            {
                if (DownMass[i, j] == DownMass[i + 1, j] && DownMass[i, j] != 0)
                {
                    isAddNum = true;

                    DownMass[i, j] += DownMass[i + 1, j];
                    score += DownMass[i, j];//スコア加算

                    for (int loop = i; loop >= 0; loop--)
                    {
                        DownMass[loop + 1, j] = DownMass[loop, j];
                        DownMass[loop, j] = 0;
                    }
                }
            }
        }
        ////下方向ここまで////

        //移動があり、かつスコアを更新していれば上書き
        if (isAddNum && score > maxScore)
        {
            result = "down";
            maxScore = score;
        }

        return result;
    }



    //上下方向のどの手が最も良いかを判断する関数(2手先読み)
    string GetHand()
    {
        //今の盤面をコピー
        int[,] Mass = new int[4, 4];
        Array.Copy(myBattleManager.MassNum, 0, Mass, 0, 16);

        //新規クラスを作成して現状を格納
        MassState ms = new MassState();
        ms.NowMass = Mass;
        ms.isAddNum = false;
        ms.score = 0;

        //2手先まで読むAIに値をパス
        MassState ms2 = ChoiseHand2(ms);

        //返ってきた内容をreturn
        if (ms2 == null) return "";
        else return ms2.Choise[0];
    }

    //2手先の中でもっともScoreが高い手を返す関数
    MassState ChoiseHand2(MassState ms)
    {
        List<MassState> mss = new List<MassState>();

        //上下左右のそれぞれの手を変数に格納
        MassState ms5 = GoUp(ms);
        MassState ms6 = GoLeft(ms);
        MassState ms7 = GoRight(ms);
        MassState ms8 = GoDown(ms);

        //変化があった場合だけ更にその1手先の変化をリストに格納
        if (ms5.isAddNum) mss.Add(ChoiseHand(ms5));
        if (ms6.isAddNum) mss.Add(ChoiseHand(ms6));
        if (ms7.isAddNum) mss.Add(ChoiseHand(ms7));
        if (ms8.isAddNum) mss.Add(ChoiseHand(ms8));

        //変化があったものだけを抽出して、スコアの高い順にソート
        var a = mss.FindAll(n => n.isAddNum == true);
        a.Sort((n, m) => m.score - n.score);

        //一番上の手を返す
        if (a.Count == 0) return null;
        else return a[0];
    }

    //1手先の中でもっともScoreが高い手を返す関数
    MassState ChoiseHand(MassState ms)
    {
        List<MassState> mss = new List<MassState>();

        //上下左右のそれぞれの手をリストに格納
        mss.Add(GoUp(ms));
        mss.Add(GoLeft(ms));
        mss.Add(GoRight(ms));
        mss.Add(GoDown(ms));

        //変化があったものだけを抽出して、スコアの高い順にソート
        var a = mss.FindAll(n => n.isAddNum == true);
        a.Sort((n, m) => m.score - n.score);

        //一番上の手を返す
        if (a.Count == 0) return null;
        else return a[0];
    }

    //上方向の操作
    MassState GoUp(MassState ms_origin)
    {
        MassState ms = ms_origin.GetCopy();

        ms.isAddNum = false;
        ms.Choise.Add("up");

        ////上方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (ms.NowMass[i - 1, j] == 0 && ms.NowMass[i, j] != 0)
                    {
                        ms.isAddNum = true;

                        ms.NowMass[i - 1, j] = ms.NowMass[i, j];
                        ms.NowMass[i, j] = 0;
                    }
                }
            }
        }

        for (int i = 1; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (ms.NowMass[i, j] == ms.NowMass[i - 1, j] && ms.NowMass[i, j] != 0)
                {
                    ms.isAddNum = true;

                    ms.NowMass[i, j] += ms.NowMass[i - 1, j];
                    ms.score += ms.NowMass[i, j];//スコア加算

                    for (int loop = i; loop < 4; loop++)
                    {
                        ms.NowMass[loop - 1, j] = ms.NowMass[loop, j];
                        ms.NowMass[loop, j] = 0;
                    }
                }
            }
        }
        return ms;
    }

    //左方向の操作
    MassState GoLeft(MassState ms_origin)
    {
        MassState ms = ms_origin.GetCopy();

        ms.isAddNum = false;
        ms.Choise.Add("left");

        ////左方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (ms.NowMass[j, i - 1] == 0 && ms.NowMass[j, i] != 0)
                    {
                        ms.isAddNum = true;

                        ms.NowMass[j, i - 1] = ms.NowMass[j, i];
                        ms.NowMass[j, i] = 0;
                    }
                }
            }
        }

        for (int i = 1; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (ms.NowMass[j, i] == ms.NowMass[j, i - 1] && ms.NowMass[j, i] != 0)
                {
                    ms.isAddNum = true;

                    ms.NowMass[j, i] += ms.NowMass[j, i - 1];
                    ms.score += ms.NowMass[j, i];//スコア加算

                    for (int loop = i; loop < 4; loop++)
                    {
                        ms.NowMass[j, loop - 1] = ms.NowMass[j, loop];
                        ms.NowMass[j, loop] = 0;
                    }
                }
            }
        }
        return ms;
    }

    //右方向の操作
    MassState GoRight(MassState ms_origin)
    {
        MassState ms = ms_origin.GetCopy();

        ms.isAddNum = false;
        ms.Choise.Add("right");

        ////右方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 4 - 2; i >= 0; i--)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (ms.NowMass[j, i + 1] == 0 && ms.NowMass[j, i] != 0)
                    {
                        ms.isAddNum = true;

                        ms.NowMass[j, i + 1] = ms.NowMass[j, i];
                        ms.NowMass[j, i] = 0;
                    }
                }
            }
        }

        for (int i = 4 - 2; i >= 0; i--)
        {
            for (int j = 0; j < 4; j++)
            {
                if (ms.NowMass[j, i] == ms.NowMass[j, i + 1] && ms.NowMass[j, i] != 0)
                {
                    ms.isAddNum = true;

                    ms.NowMass[j, i] += ms.NowMass[j, i + 1];
                    ms.score += ms.NowMass[j, i];//スコア加算

                    for (int loop = i; loop >= 0; loop--)
                    {
                        ms.NowMass[j, loop + 1] = ms.NowMass[j, loop];
                        ms.NowMass[j, loop] = 0;
                    }
                }
            }
        }
        return ms;
    }

    //下方向の操作
    MassState GoDown(MassState ms_origin)
    {
        MassState ms = ms_origin.GetCopy();

        ms.isAddNum = false;
        ms.Choise.Add("down");

        ////下方向の操作でのスコアをGET////
        for (int loop = 0; loop < 4 - 1; loop++)
        {
            for (int i = 4 - 2; i >= 0; i--)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (ms.NowMass[i + 1, j] == 0 && ms.NowMass[i, j] != 0)
                    {
                        ms.isAddNum = true;

                        ms.NowMass[i + 1, j] = ms.NowMass[i, j];
                        ms.NowMass[i, j] = 0;
                    }
                }
            }
        }

        for (int i = 4 - 2; i >= 0; i--)
        {
            for (int j = 0; j < 4; j++)
            {
                if (ms.NowMass[i, j] == ms.NowMass[i + 1, j] && ms.NowMass[i, j] != 0)
                {
                    ms.isAddNum = true;

                    ms.NowMass[i, j] += ms.NowMass[i + 1, j];
                    ms.score += ms.NowMass[i, j];//スコア加算

                    for (int loop = i; loop >= 0; loop--)
                    {
                        ms.NowMass[loop + 1, j] = ms.NowMass[loop, j];
                        ms.NowMass[loop, j] = 0;
                    }
                }
            }
        }
        return ms;
    }

}

//マスの状態（盤面、変化の有無、スコア、上下左右の手の内訳）を格納できるクラス
public class MassState
{
    public int[,] NowMass = new int[4, 4];
    public bool isAddNum;
    public int score;
    public List<string> Choise = new List<string>();

    //新規インスタンスに今の数値をコピーできる関数
    public MassState GetCopy()
    {
        MassState ms = new MassState();

        int[,] Mass = new int[4, 4];
        Array.Copy(NowMass, 0, Mass, 0, 16);

        ms.NowMass = Mass;
        ms.isAddNum = isAddNum;
        ms.score = score;
        ms.Choise = new List<string>(Choise);

        return ms;
    }
}