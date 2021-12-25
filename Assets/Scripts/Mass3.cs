using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mass3 : MonoBehaviour
{

    //攻撃イメージSO
    public AttackImages attackImages;

    //自分自身が持つ要素を定義
    public int myNum = 0;
    public int myMergeNum = -1;
    public int myLevel = 0;

    //各種フラグ
    public bool isDisplayed = false;
    public bool isMoving = false;
    public bool isBirthing = false;


    //画像を更新する関数
    public void TextUpdate()
    {
        //SpriteRendererとImagesの保有枚数を取得
        var sr = GetComponent<Image>();
        int max = attackImages.Images.Length;

        //現在のmyNumの値からmyLevelを設定
        myLevel = (int)Mathf.Log(myNum, 2);

        //Levelが0なら、Spriteをナシに設定
        if (myLevel <= 0) sr.sprite = null;

        //Levelが1~maxの間なら、Levelに応じたSpriteを設定
        else if (myLevel < max) sr.sprite = attackImages.Images[myLevel - 1];

        //Levelがmax以上なら(多分無いけどｗ)、1週してもとに戻る
        else sr.sprite = attackImages.Images[myLevel % max];
    }

    //生み出される関数
    public void Birth()
    {
        StartCoroutine(BirthAnimation());
    }

    //生み出されるアニメーションを実行するコルーチン
    IEnumerator BirthAnimation()
    {
        //誕生フラグをON
        isBirthing = true;

        //アニメーションの時間を設定
        int Times = BattleManager.MotionTime;

        //徐々に大きくなるモーションを実行
        transform.localScale = Vector2.zero;
        for (int i = 0; i < Times; i++)
        {
            transform.localScale = new Vector2((float)1 / Times * i, (float)1 / Times * i);
            yield return null;
        }

        //１倍スケールを設定
        transform.localScale = Vector2.one;

        //誕生フラグをOFF
        isBirthing = false;
    }


    //指定場所に移動する関数
    public void MoveTo(Vector2 pos, bool isMerged)
    {
        //アニメーションのコルーチンを実行
        StartCoroutine(MoveAnimation(pos, isMerged));
    }

    //指定場所に移動するアニメーションを実行するコルーチン
    IEnumerator MoveAnimation(Vector2 TargetPos, bool isMerged)
    {
        //移動フラグをON
        isMoving = true;

        //アニメーションの時間を設定
        int Times = BattleManager.MotionTime;

        //目標座標と現在地から移動距離を計算
        float moveX = TargetPos.x - transform.localPosition.x;
        float moveY = TargetPos.y - transform.localPosition.y;

        //移動するアニメーションを実行
        for (int i = 0; i < Times; i++)
        {
            var pos = transform.localPosition;
            pos.x += moveX / Times;
            pos.y += moveY / Times;

            transform.localPosition = pos;
            yield return null;
        }

        //目的地に設定
        transform.localPosition = TargetPos;

        //移動フラグをOFF
        isMoving = false;

        //マージされている場合は移動後にオブジェクトを削除
        if (isMerged)
        {
            isDisplayed = false;
            this.gameObject.SetActive(false);
        }
    }
}
