using UnityEngine;
using UnityEngine.UI;

public class Manual : MonoBehaviour
{
    [SerializeField]
    Button manual;
    [SerializeField]
    Button r;
    [SerializeField]
    GameObject im;

    private void Start()
    {
        im.SetActive(false);

        manual.onClick.AddListener(Manualstart);
    }

    private void Manualstart()
    {
        im.SetActive(true);

        manual.onClick.RemoveAllListeners();

        r.onClick.AddListener(Manualreturn);
    }

    private void Manualreturn()
    {
        im.SetActive(false);

        r.onClick.RemoveAllListeners();

        manual.onClick.AddListener(Manualstart);
    }
}
