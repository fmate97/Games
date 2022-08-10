using UnityEngine;

public class ItemCoinController : MonoBehaviour
{
    [SerializeField] UIPanelController uiPanelController;

    private string _playerTag = "Player";
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag(_playerTag))
        {
            uiPanelController.CoinsNumberIncrease();
            gameObject.SetActive(false);
        }
    }
}
