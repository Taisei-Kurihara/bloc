using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;


// シーンの種類を表すenum.
public enum SceneControllerType
{
    OutGame,         // ゲーム外（メニュー等）.
    InGame,          // ゲーム内.
    CurrentSceneOnly // 現在のシーンのみ.
}

// (既)修: 自身のsceneの種類(inGame,putGame,今sceneだけ) を指定するenum　を実装してください
// 上:DynamicObjectController_interface に public get のみの指定するenumを継承したクラスで強制するようにしてください
// 上:DynamicObjectController_interface を継承するクラスの内 _Default は outGame
// 上:Game_Default はinGame それ以外は今sceneだけのenum を指定してください

//(既)修:inGame で読み込まれたものはOutGame時にリリース
//(既)修:OutGame　で読み込まれたものはinGame でrelease
//(既)修:CurrentSceneOnly で読み込まれたものはscene切り替え時に破棄

// 動的オブジェクトコントローラーのインターフェース定義.
// シーンの読み込み・解放時の処理を実装するためのインターフェース.
public interface DynamicObjectController_interface
{
    /// <summary> このコントローラーのシーン種類. </summary>
    public SceneControllerType ControllerType { get; }

    /// <summary> 子のDynamicObjectControllerリスト. </summary>
    public List<DynamicObjectController_interface> ChildControllers { get; }

    /// <summary> このコントローラーがロードするアセットのアドレスリスト. </summary>
    public List<AddressableAssetAddress> LoadedAssetAddresses { get; }

    /// <summary> fade開始前に待機させずに読み込みを始めるメソッド（非同期）. </summary>
    public UniTask OnBeforeFadeLoadAsync();

    /// <summary> シーンのロードが完了した後、fade前に呼び出されるメソッド. </summary>
    public UniTask OnSceneLoadedAsync();

    /// <summary> fade完了後に呼び出されるメソッド. </summary>
    public UniTask OnFadeCompletedAsync();

    /// <summary> シーンのアンロードが完了した後に呼び出されるメソッド </summary>
    public UniTask OnSceneUnloadedAsync();
}