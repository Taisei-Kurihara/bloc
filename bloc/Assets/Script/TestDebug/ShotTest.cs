using UnityEngine;

public class ShotTest : MonoBehaviour
{
    public void Init(Vector3 v3)
    {
        GetComponent<Rigidbody2D>().linearVelocity = v3*5;
    }
}
