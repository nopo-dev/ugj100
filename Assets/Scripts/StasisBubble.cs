using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StasisBubble : MonoBehaviour
{

    [SerializeField] private float _bubbleScale = 2f;
    [SerializeField] private float _expandShrinkTime = 0.1f;
    [SerializeField] private Vector3 _slowShrinkScale = Vector3.one * 0.75f;

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
        StartCoroutine(SlowContractBubble());
    }

    private IEnumerator SlowContractBubble()
    {
        float slowShrinkTime = 5f - 2 * _expandShrinkTime;

        for (float t = 0; t <= slowShrinkTime; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(Vector3.one * _bubbleScale, _slowShrinkScale * _bubbleScale, t / slowShrinkTime);
            yield return null;
        }
        
        StartCoroutine(ContractBubble());
    }

    private IEnumerator ContractBubble()
    {
        Vector3 initialScale = transform.localScale;
        for (float expandTimer = 0; expandTimer <= _expandShrinkTime; expandTimer += Time.deltaTime)
        {
            transform.localScale = initialScale * ((_expandShrinkTime - expandTimer) / _expandShrinkTime);
            yield return null;
        }
        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

    public void ResetBubble()
    {
        StopAllCoroutines();
        StartCoroutine(ContractBubble());
    }
}
