using UnityEngine;

public class ItemHealthflaskController : MonoBehaviour
{
    [SerializeField] int BonusHPValue;

    private string _playerTag = "Player";
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_playerTag))
        {
            other.gameObject.GetComponentInChildren<PlayerHPBarController>().GetHP(BonusHPValue);
            gameObject.SetActive(false);
        }
    }
}
