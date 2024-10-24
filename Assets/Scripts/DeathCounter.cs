using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCounter : MonoBehaviour
{
    [SerializeField] private GameObject _hundredsPlace;
    [SerializeField] private GameObject _tensPlace;
    [SerializeField] private GameObject _onesPlace;

    public void UpdateNumber(int number)
    {
        _hundredsPlace.GetComponent<Animator>().SetInteger("Number", number / 100 % 10);
        _tensPlace.GetComponent<Animator>().SetInteger("Number", number / 10 % 10);
        _onesPlace.GetComponent<Animator>().SetInteger("Number", number % 10);
    }

    public void DisplayNumber()
    {
        _hundredsPlace.GetComponent<SpriteRenderer>().enabled = true;
        _tensPlace.GetComponent<SpriteRenderer>().enabled = true;
        _onesPlace.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void HideNumber()
    {
        _hundredsPlace.GetComponent<SpriteRenderer>().enabled = false;
        _tensPlace.GetComponent<SpriteRenderer>().enabled = false;
        _onesPlace.GetComponent<SpriteRenderer>().enabled = false;
    }

    private float _delayTime = 3f;
    public void DelayHideNumber()
    {
        StopAllCoroutines();
        StartCoroutine(HideNumberAfterTime());
    }

    private IEnumerator HideNumberAfterTime()
    {
        if (_hundredsPlace.GetComponent<SpriteRenderer>().enabled == false)
            yield break;
        yield return new WaitForSeconds(_delayTime);
        HideNumber();
    }
}
