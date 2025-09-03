using UnityEngine;

public class Contact_Coin : Contact_abstract
{
    [SerializeField]
    int coins = 1; // 取得するコインの数
    override protected void Contact(Collider2D collider)
    {
        // コインを取得したときの処理
        Debug.Log("Coin collected!");
        Score.Instance().AddCoin(coins); // スコアを増やす
        Destroy(gameObject); // コインオブジェクトを削除
    }
}
