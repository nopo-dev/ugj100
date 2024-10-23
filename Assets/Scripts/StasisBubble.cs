using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StasisBubble : MonoBehaviour
{

    [SerializeField] private float _bubbleScale = 2f;
    [SerializeField] private float _expandShrinkTime = 0.1f;

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    public void Stasis(float time)
    {
        StartCoroutine(ExpandBubble(time));
    }

    private IEnumerator ExpandBubble(float time)
    {
        for (float expandTimer = 0; expandTimer <= _expandShrinkTime; expandTimer += Time.deltaTime)
        {
            transform.localScale = Vector3.one * _bubbleScale * (expandTimer / _expandShrinkTime);
            yield return null;
        }
        yield return new WaitForSeconds(time - 2f * _expandShrinkTime);
        StartCoroutine(ContractBubble());
    }

    // private IEnumerator Instability(float time)
    // {

    // }

    private IEnumerator ContractBubble()
    {
        for (float expandTimer = 0; expandTimer <= _expandShrinkTime; expandTimer += Time.deltaTime)
        {
            transform.localScale = Vector3.one * _bubbleScale * ((_expandShrinkTime - expandTimer) / _expandShrinkTime);
            yield return null;
        }
        transform.localScale = Vector3.zero;
    }

    public void ResetBubble()
    {
        StopAllCoroutines();
        StartCoroutine(ContractBubble());
    }
}
