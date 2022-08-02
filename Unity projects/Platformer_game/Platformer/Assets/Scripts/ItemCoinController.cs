using UnityEngine;

public class ItemCoinController : MonoBehaviour
{
    private string _playerTag = "Player";
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(_playerTag))
        {
            gameObject.SetActive(false);
        }
    }
}
