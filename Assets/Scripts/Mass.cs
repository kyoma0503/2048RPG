using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mass : MonoBehaviour
{
    //自分自身が持つ要素を定義
    public int myNum = 0;
    public int myMergeNum = -1;
    public Text myText;

    //表示テキストを更新
    public void TextUpdate()
    {
        if(myNum == 0)
        {
            myText.text = "";
        }
        else
        {
            myText.text = myNum.ToString();
        }
    }

    //指定場所に移動する関数
    public void MoveTo(Vector2 pos, float MoveTime, bool isMerged)
    {
        //アニメーションのコルーチンを実行
        StartCoroutine(MoveAnimation(pos, MoveTime, isMerged));
    }

    //指定場所に移動するアニメーションを実行するコルーチン
    IEnumerator MoveAnimation(Vector2 TargetPos, float MoveTime, bool isMerged)
    {
        //指定秒数からコマフレームを計算
        float cutTime = 0.020f;
        int TimeCutNum = (int)(MoveTime / cutTime);

        //目標座標と現在地から移動距離を計算
        float moveX = TargetPos.x - transform.position.x;
        float moveY = TargetPos.y - transform.position.y;

        //移動するアニメーションを実行
        for(int i = 0; i < TimeCutNum; i++)
        {
            var pos = transform.position;
            pos.x += moveX / TimeCutNum;
            pos.y += moveY / TimeCutNum;

            transform.position = pos;
            yield return new WaitForSeconds(cutTime);
        }

        //マージされている場合は移動後にオブジェクトを削除
        if (isMerged) Destroy(this.gameObject);
    }
}
