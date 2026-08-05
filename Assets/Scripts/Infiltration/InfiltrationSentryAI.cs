using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfiltrationSentryAI : MonoBehaviour
{
    public float rotateInterval = 2f;

    IEnumerator Start()
    {
        while (true)
        {
            // 90도씩 회전하여 시야 돌리기
            transform.Rotate(0, 0, 90f);
            yield return new WaitForSeconds(rotateInterval);
        }
    }
}