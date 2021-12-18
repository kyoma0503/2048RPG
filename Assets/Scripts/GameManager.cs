using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    int EDGE = 4;

    public int[,] MassNum = new int[4,4];
    public GameObject Blocks;

    public bool isAddNum = false;

    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

        InitMassNum();
        DisplayMassNum();
    }

    void Update()
    { 
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

    void InitMassNum()
    {
        for(int i = 0; i < EDGE; i++)
        {
            for(int j = 0; j < EDGE; j++)
            {
                MassNum[i, j] = 0;
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

    int NewNumGenerator()
    {
        int rnd = Random.Range(0, 4);

        if (rnd == 3) return 4;
        else return 2;
    }

    public void DisplayMassNum()
    {
        for (int i = 0; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                var Mymass = Blocks.transform.GetChild(i).GetChild(j).GetComponent<Mass>();
                Mymass.myNum = MassNum[i, j];
                Mymass.TextUpdate();
            }
        }
    }

    void UpArrow()
    {
        isAddNum = false;

        for (int loop = 0; loop < EDGE-1; loop++)
        {
            for (int i = 1; i < EDGE; i++)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[i - 1, j] == 0)
                    {
                        if (MassNum[i, j] != 0) isAddNum = true;

                        MassNum[i - 1, j] = MassNum[i, j];
                        MassNum[i, j] = 0;
                    }
                }
            }
        }
        // 加算
        for (int i = 1; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[i, j] == MassNum[i - 1, j])
                {
                    if (MassNum[i, j] != 0) isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[i, j] += MassNum[i - 1, j];
                    for (int loop = i; loop < EDGE; loop++)
                    {
                        MassNum[loop - 1, j] = MassNum[loop, j];
                        MassNum[loop, j] = 0;
                    }
                }
            }
        }

        if (isAddNum) CreateNewNum();
        DisplayMassNum();
    }

    void DownArrow()
    {
        isAddNum = false;

        for (int loop = 0; loop < EDGE - 1; loop++)
        {
            for (int i = EDGE-2; i >= 0; i--)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[i + 1, j] == 0)
                    {
                        if (MassNum[i, j] != 0) isAddNum = true;

                        MassNum[i + 1, j] = MassNum[i, j];
                        MassNum[i, j] = 0;
                    }
                }
            }
        }
        // 加算
        for (int i = EDGE - 2; i >= 0; i--)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[i, j] == MassNum[i + 1, j])
                {
                    if (MassNum[i, j] != 0) isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[i, j] += MassNum[i + 1, j];
                    for (int loop = i; loop >= 0; loop--)
                    {
                        MassNum[loop + 1, j] = MassNum[loop, j];
                        MassNum[loop, j] = 0;
                    }
                }
            }
        }

        if (isAddNum) CreateNewNum();
        DisplayMassNum();
    }

    void LeftArrow()
    {
        isAddNum = false;

        for (int loop = 0; loop < EDGE - 1; loop++)
        {
            for (int i = 1; i < EDGE; i++)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[j, i - 1] == 0)
                    {
                        if (MassNum[j, i] != 0) isAddNum = true;

                        MassNum[j, i - 1] = MassNum[j, i];
                        MassNum[j, i] = 0;
                    }
                }
            }
        }
        // 加算
        for (int i = 1; i < EDGE; i++)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[j, i] == MassNum[j, i - 1])
                {
                    if (MassNum[j, i] != 0) isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[j, i] += MassNum[j, i - 1];
                    for (int loop = i; loop < EDGE; loop++)
                    {
                        MassNum[j, loop - 1] = MassNum[j, loop];
                        MassNum[j, loop] = 0;
                    }
                }
            }
        }

        if (isAddNum) CreateNewNum();
        DisplayMassNum();
    }

    void RightArrow()
    {
        isAddNum = false;

        for (int loop = 0; loop < EDGE - 1; loop++)
        {
            for (int i = EDGE - 2; i >= 0; i--)
            {
                for (int j = 0; j < EDGE; j++)
                {
                    if (MassNum[j, i + 1] == 0)
                    {
                        if (MassNum[j,i] != 0) isAddNum = true;

                        MassNum[j, i + 1] = MassNum[j,i];
                        MassNum[j,i] = 0;
                    }
                }
            }
        }
        // 加算
        for (int i = EDGE - 2; i >= 0; i--)
        {
            for (int j = 0; j < EDGE; j++)
            {
                if (MassNum[j, i] == MassNum[j, i + 1])
                {
                    if (MassNum[j, i] != 0) isAddNum = true;

                    // 移動元に加算してから、移動先に上書きする形で移動する
                    MassNum[j, i] += MassNum[j, i + 1];
                    for (int loop = i; loop >= 0; loop--)
                    {
                        MassNum[j, loop + 1] = MassNum[j, loop];
                        MassNum[j, loop] = 0;
                    }
                }
            }
        }

        if (isAddNum) CreateNewNum();
        DisplayMassNum();
    }

    void CreateNewNum()
    {
        List<int[]> zeroList = new List<int[]>();

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

        int rnd = Random.Range(0, zeroList.Count);
        int x = zeroList[rnd][0];
        int y = zeroList[rnd][1];

        MassNum[x, y] = NewNumGenerator();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene("Game");
    }
}
