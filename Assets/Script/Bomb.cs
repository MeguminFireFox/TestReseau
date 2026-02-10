using Unity.Netcode;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
    [SerializeField] private float _radius = 5f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _fallSpeed = 5f;
    [SerializeField] private GameObject _sphereVisualPrefab;

    private bool activated = false;

    private void Update()
    {
        if (!IsServer || activated) return;

        transform.position += Vector3.down * _fallSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || activated) return;

        if (((1 << other.gameObject.layer) & _groundLayer) != 0)
        {
            activated = true;
            Area();
        }
    }

    private void Area()
    {
        SpawnVisualClientRpc(transform.position, _radius);

        Collider[] hits = Physics.OverlapSphere(transform.position, _radius, _playerLayer);

        foreach (Collider hit in hits)
        {
            var playerCollector = hit.GetComponent<PlayerCoinCollector>();

            if (playerCollector != null)
            {
                playerCollector.TakeDamage(_damage);
            }
        }

        PoolManager.Instance.ReturnToPool("BombPool", NetworkObject);
        activated = false;
    }

    [ClientRpc]
    private void SpawnVisualClientRpc(Vector3 position, float radius)
    {
        if (_sphereVisualPrefab == null) return;

        GameObject visual = Instantiate(
            _sphereVisualPrefab,
            position,
            Quaternion.identity
        );

        visual.transform.localScale = Vector3.one * radius * 2f;
        Destroy(visual, 2f);
    }
}
