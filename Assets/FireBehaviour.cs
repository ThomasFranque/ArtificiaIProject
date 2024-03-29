using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FireBehaviour : MonoBehaviour
{
    [SerializeField] private Renderer _fireRenderer;
    [SerializeField] private GameObject _fireOutskirts;
    [SerializeField] private NavMeshObstacle _navMeshObstacle;
    [SerializeField] private Animator _fireStatemachine;
    [SerializeField] private int _fireAftermathLayer;
    [SerializeField] private LayerMask _fireAndObstacles;
    [SerializeField] private Vector2 _minMaxLifetime;
    [SerializeField] private float aftermathDuration;
    private bool spawnRight;
    private bool spawnLeft;
    private bool spawnFront;
    private bool spawnBack;

    // Start is called before the first frame update
    private void Awake()
    {
        spawnRight = Random.value >= 0.5f;
        spawnLeft = Random.value >= 0.5f;
        spawnFront = Random.value >= 0.5f;
        spawnBack = Random.value >= 0.5f;
    }

    private void Start()
    {
        StartCoroutine(CLifetime());
    }

    private IEnumerator CLifetime()
    {
        yield return new WaitForSeconds(Random.Range(_minMaxLifetime.x, _minMaxLifetime.y));
        Propagate();
        PutOutFire();
        yield return new WaitForSeconds(aftermathDuration);
        Destroy(gameObject);
    }

    private void Propagate()
    {
        Vector3 posRight = transform.position + Vector3.right * transform.parent.localScale.x;
        Vector3 posLeft = transform.position + Vector3.left * transform.parent.localScale.x;
        Vector3 posFront = transform.position + Vector3.forward * transform.parent.localScale.x;
        Vector3 posBack = transform.position + Vector3.back * transform.parent.localScale.x;

        if (spawnRight && IsFreeSpace(posRight))
            Instantiate(gameObject, posRight, Quaternion.identity, transform.parent);
        if (spawnLeft && IsFreeSpace(posLeft))
            Instantiate(gameObject, posLeft, Quaternion.identity, transform.parent);
        if (spawnFront && IsFreeSpace(posFront))
            Instantiate(gameObject, posFront, Quaternion.identity, transform.parent);
        if (spawnBack && IsFreeSpace(posBack))
            Instantiate(gameObject, posBack, Quaternion.identity, transform.parent);
    }

    private void PutOutFire()
    {
        _fireRenderer.enabled = false;
        _navMeshObstacle.enabled = false;
        _fireStatemachine.enabled = false;
        _fireOutskirts.SetActive(false);
        gameObject.layer = _fireAftermathLayer;
    }

    private bool IsFreeSpace(Vector3 pos)
    {
        return !Physics.CheckSphere(pos, transform.localScale.x * 0.45f, _fireAndObstacles);
    }
}
