using UnityEngine;

public class PresenterBase : MonoBehaviour
{
    IMove Move { get; }

    public IShape Shape { get; }
}
