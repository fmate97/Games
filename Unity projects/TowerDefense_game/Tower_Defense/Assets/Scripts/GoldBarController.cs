using System;
using TMPro;
using UnityEngine;

public class GoldBarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldValueText;
    [SerializeField] float rateOfValueChange = .1f;

    private int _GoldValue = 0;
    private float _displayGoldValue = 0f;

    public void GoldBarInit(int startGoldValue)
    {
        _displayGoldValue = _GoldValue = startGoldValue;
    }

    void Update()
    {
        if(_displayGoldValue > _GoldValue)
        {
            _displayGoldValue -= rateOfValueChange;
        }
        else if(_displayGoldValue < _GoldValue)
        {
            _displayGoldValue += rateOfValueChange;
        }

        SetGoldValue(Convert.ToInt32(_displayGoldValue));
    }

    public void AddGold(int goldValue)
    {
        _GoldValue += goldValue;

        if(_GoldValue > 1000000)
        {
            _GoldValue = 1000000;
        }
    }

    public bool BuyWithGold(int goldValue)
    {
        if (goldValue > _GoldValue)
        {
            return false;
        }
        else
        {
            _GoldValue -= goldValue;
            return true;
        }
    }

    private void SetGoldValue(int setValue)
    {
        goldValueText.text = $"{setValue}";
    }
}
