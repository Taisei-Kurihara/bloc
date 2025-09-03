using UnityEngine;

public class Contact_Goal : Contact_abstract
{
    override protected void Contact(Collider2D collider)
    {
        // ゴールに到達したときの処理
        Debug.Log("Goal reached!");
        // ここでゲームクリアの処理を追加できます
        // 例: シーンの切り替え、UIの表示など
    }
}
