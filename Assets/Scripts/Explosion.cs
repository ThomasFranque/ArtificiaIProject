using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private GameObject _pointBlank;
    [SerializeField] private GameObject _middle;
    [SerializeField] private GameObject _outskirts;

    public void Initialize(float pointBlank, float middle, float outskirts)
    {
        _pointBlank.transform.localScale = Vector3.one * (pointBlank * 2);
        _middle.transform.localScale = new Vector3(1, 0.25f, 1) * (middle * 2);
        _outskirts.transform.localScale = new Vector3(1, 0.05f, 1) * (outskirts * 2);

        StartCoroutine(CQueueDeath());
    }

    private IEnumerator CQueueDeath()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}
