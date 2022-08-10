using UnityEngine;

public class UIPanelController : MonoBehaviour
{
    [SerializeField] GameObject coin1;
    [SerializeField] GameObject coin2;
    [SerializeField] GameObject coin3;

    private int _coinsNumber;

    void Start()
    {
        _coinsNumber = 0;
        CoinsNumberChanged();
    }

    public void CoinsNumberIncrease()
    {
        _coinsNumber++;
        CoinsNumberChanged();
    }

    void CoinsNumberChanged()
    {
        switch (_coinsNumber)
        {
            case 0:
                coin1.SetActive(false);
                coin2.SetActive(false);
                coin3.SetActive(false);
                break;
            case 1:
                coin1.SetActive(true);
                coin2.SetActive(false);
                coin3.SetActive(false);
                break;
            case 2:
                coin1.SetActive(true);
                coin2.SetActive(true);
                coin3.SetActive(false);
                break;
            case 3:
                coin1.SetActive(true);
                coin2.SetActive(true);
                coin3.SetActive(true);
                break;
            default:
                _coinsNumber = 0;
                CoinsNumberChanged();
                break;
        }
    }
}
