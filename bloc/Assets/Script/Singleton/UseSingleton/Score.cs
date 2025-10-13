using Common;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class Score : Singleton_MonoBehaviourBase<Score>
{
    ReactiveProperty<int> coinProperty = new ReactiveProperty<int>(0);
    public int CoinValue
    {
        get { return coinProperty.Value; }
        private set { coinProperty.Value = value; }
    }

    public void AddCoin(int amount)
    {
        CoinValue += amount;
        Debug.Log("Score added: " + amount + ", Total Score: " + CoinValue);
    }

    public void ResetCoin()
    {
        CoinValue = 0;
        Debug.Log("Score reset to zero.");
    }
}
