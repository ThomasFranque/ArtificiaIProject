using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour
{
    [SerializeField] private float _explosionRadius;
    [SerializeField, Range(0f, 1f)] private float _pointBlankRadius;
    [SerializeField, Range(0f, 1f)] private float _middleRadius;
    [SerializeField] private float explosionWearoffTime = 5;
    [SerializeField] private bool _hideGizmos;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private LayerMask _agentMask;

    private static Coroutine _explosionWearoff;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //create a ray cast and set it to the mouses cursor position in game
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //draw invisible ray cast/vector
                Debug.DrawLine(ray.origin, hit.point);
                SpawnExplosionAt(hit.point);
            }
        }
    }

    private void SpawnExplosionAt(Vector3 position)
    {
        Explosion explosion;
        explosion = GameObject.Instantiate(_explosionPrefab).GetComponent<Explosion>();
        explosion.transform.position = position;
        explosion.Initialize(PointBlankRadius, MiddleRadius, OutskirtRadius);
        HitAll(position);
    }

    private void HitAll(Vector3 position)
    {
        HashSet<AgentEntity> _alreadyDealtWith = new HashSet<AgentEntity>();
        Collider[] toKill;
        Collider[] toCripple;
        Collider[] toScare;

        toKill = Physics.OverlapSphere(position, PointBlankRadius, _agentMask);
        toCripple = Physics.OverlapSphere(position, MiddleRadius, _agentMask);
        toScare = Physics.OverlapSphere(position, OutskirtRadius, _agentMask);

        ApplyEffect(toKill, ExplosionRadiusType.point_blank);
        ApplyEffect(toCripple, ExplosionRadiusType.middle);
        ApplyEffect(toScare, ExplosionRadiusType.outskirts);

        OnExplosion?.Invoke(toKill.Length);

        if (_explosionWearoff != default)
            StopCoroutine(_explosionWearoff);
        _explosionWearoff = StartCoroutine(CExplosionWearoff());

        void ApplyEffect(Collider[] collection, ExplosionRadiusType radiusType)
        {
            foreach (Collider c in collection)
            {
                AgentEntity e;
                e = c.GetComponent<AgentEntity>();
                if (_alreadyDealtWith.Contains(e)) continue;
                e.ExplosionVictim(radiusType);
                _alreadyDealtWith.Add(e);
            }
        }
    }

    private IEnumerator CExplosionWearoff()
    {
        yield return new WaitForSeconds(explosionWearoffTime);
        _explosionWearoff = default;
        OnExplosionWearOff?.Invoke();
    }

    private void OnDrawGizmos()
    {
        if (_hideGizmos) return;
        Gizmos.color = Color.red; // Point blank
        Gizmos.DrawWireSphere(transform.position, PointBlankRadius);
        Gizmos.color = Color.yellow; // Middle
        Gizmos.DrawWireSphere(transform.position, MiddleRadius);
        Gizmos.color = Color.cyan; // Alert
        Gizmos.DrawWireSphere(transform.position, OutskirtRadius);
    }

    private float PointBlankRadius => _explosionRadius * _middleRadius * _pointBlankRadius;
    private float MiddleRadius => _explosionRadius * _middleRadius;
    private float OutskirtRadius => _explosionRadius;

    public static event System.Action<int> OnExplosion;
    public static event System.Action OnExplosionWearOff;
}
