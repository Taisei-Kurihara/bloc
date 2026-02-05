using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenTest : MonoBehaviour
{
    public GameObject obj;

    void Start()
    {
        StartCoroutine(GenerateAfterDelay());
    }

    IEnumerator GenerateAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        // objを1000個生成.
        // 生成も100個ごとに0.5秒待機.
        List<ShotTest> shotTestList = new List<ShotTest>();
        for (int i = 0; i < 1000; i++)
        {
            GameObject o = Instantiate(obj);
            shotTestList.Add(o.GetComponent<ShotTest>());
            if ((i + 1) % 100 == 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return new WaitForSeconds(5f);

        for (int i = 0; i < shotTestList.Count; i++)
        {
            shotTestList[i].Init(Vector3.one);
            if ((i + 1) % 100 == 0)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
