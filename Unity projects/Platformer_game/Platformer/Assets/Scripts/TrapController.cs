using UnityEngine;

public class TrapController : MonoBehaviour
{
    [Range(0, 20)]
    [SerializeField] int TrapDamage;

    private string _playerTag = "Player";

    public void OnChieldTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_playerTag))
        {
            other.gameObject.GetComponentInChildren<PlayerHPBarController>().GetHit(TrapDamage);
        }
    }
}
