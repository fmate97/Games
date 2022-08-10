using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurretGround : MonoBehaviour
{
    [Header("Level settings")]
    [SerializeField] GameObject levelController;
    [Header("Turret settings")]
    [SerializeField] GameObject turretLv1;
    [SerializeField] int turretLv1Price;
    [SerializeField] GameObject turretLv2;
    [SerializeField] int turretLv2Price;
    [SerializeField] GameObject turretLv3;
    [SerializeField] int turretLv3Price;
    [SerializeField] GameObject turretLv4;
    [SerializeField] int turretLv4Price;
    [Header("Buttons")]
    [SerializeField] GameObject ButtonsPanel;
    [SerializeField] Button BuyButton;
    [SerializeField] Button SellButton;
    [SerializeField] TextMeshProUGUI BuyButtonText;
    [SerializeField] TextMeshProUGUI SellButtonText;
    [SerializeField] AudioSource clickSound;

    private int _sellValue = 0;
    private GameObject _turret = null;
    private LevelController _levelController;
    private Vector3 _cameraPos;
    private enum TurretType { empty, lv1, lv2, lv3, lv4 };
    private TurretType _turretType = TurretType.empty;

    void Start()
    {
        _levelController = levelController.GetComponent<LevelController>();
    }

    void Update()
    {
        if (ButtonsPanel.activeSelf 
            &&  Vector3.Distance(_levelController.mainCamera.transform.position, _cameraPos) > 3f)
        {
            ButtonsPanel.SetActive(false);
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickSound.Play();

            ButtonsPanel.SetActive(true);
            _cameraPos = _levelController.mainCamera.transform.position;

            _sellValue = SellPriceCalc(_turretType);
            switch (_turretType)
            {
                case TurretType.lv4:
                    SetButtonsText(0, _sellValue);
                    break;
                case TurretType.lv3:
                    SetButtonsText(turretLv4Price, _sellValue);
                    break;
                case TurretType.lv2:
                    SetButtonsText(turretLv3Price, _sellValue);
                    break;
                case TurretType.lv1:
                    SetButtonsText(turretLv2Price, _sellValue);
                    break;
                default:
                    SetButtonsText(turretLv1Price, _sellValue);
                    break;
            }
        }
    }

    private int SellPriceCalc(TurretType turretType)
    {
        int sellPrice = 0;

        switch (turretType)
        {
            case TurretType.lv4:
                sellPrice += turretLv4Price;
                goto case TurretType.lv3;
            case TurretType.lv3:
                sellPrice += turretLv3Price;
                goto case TurretType.lv2;
            case TurretType.lv2:
                sellPrice += turretLv2Price;
                goto case TurretType.lv1;
            case TurretType.lv1:
                sellPrice += turretLv1Price;
                break;
            default:
                sellPrice += 0;
                break;
        }

        return Mathf.RoundToInt(sellPrice / 2);
    }

    private void SetButtonsText(int buyPrice, int sellPrice)
    {
        BuyButton.interactable = SellButton.interactable = true;

        BuyButtonText.text = $"BUY\n({buyPrice})";
        SellButtonText.text = $"SELL\n({sellPrice})";

        if(buyPrice <= 0)
        {
            BuyButton.interactable = false;
        }
        if(sellPrice <= 0)
        {
            SellButton.interactable = false;
        }
    }

    public void BuyButtonPressed()
    {
        clickSound.Play();
        switch (_turretType)
        {
            case TurretType.lv3:
                if (_levelController.BuyWithGold(turretLv4Price))
                {
                    Destroy(_turret);
                    _turret = Instantiate(turretLv4, gameObject.transform.position, Quaternion.identity);
                    _turretType++;
                }
                break;
            case TurretType.lv2:
                if (_levelController.BuyWithGold(turretLv3Price))
                {
                    Destroy(_turret);
                    _turret = Instantiate(turretLv3, gameObject.transform.position, Quaternion.identity);
                    _turretType++;
                }
                break;
            case TurretType.lv1:
                if (_levelController.BuyWithGold(turretLv2Price))
                {
                    Destroy(_turret);
                    _turret = Instantiate(turretLv2, gameObject.transform.position, Quaternion.identity);
                    _turretType++;
                }
                break;
            case TurretType.empty:
                if (_levelController.BuyWithGold(turretLv1Price))
                {
                    Destroy(_turret);
                    _turret = Instantiate(turretLv1, gameObject.transform.position, Quaternion.identity);
                    _turretType++;
                }
                break;
            default:
                break;
        }
        ButtonsPanel.SetActive(false);
    }

    public void SellButtonPressed()
    {
        if (_turret != null && _sellValue > 0)
        {
            clickSound.Play();

            _levelController.AddGold(_sellValue);
            _sellValue = 0;

            Destroy(_turret);
            _turret = null;

            _turretType = TurretType.empty;

            ButtonsPanel.SetActive(false);
        }
    }
}
