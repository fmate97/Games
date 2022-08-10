using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBarController : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    [SerializeField] Image fill;
    [SerializeField] Gradient gradient;
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text HPValueText;
    [SerializeField] TMP_Text maxHPValueText;
    [SerializeField] float rateOfValueChange;

    private int _HPValue = 0;
    private float _displayHPValue = 0f;

    public void HPBarInit(int maxHPValue)
    {
        HPValueText.text = maxHPValueText.text = $"{maxHPValue}";
        slider.value = slider.maxValue = maxHPValue;
        _displayHPValue = _HPValue = maxHPValue;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    void Update()
    {
        if (_displayHPValue > _HPValue)
        {
            _displayHPValue -= rateOfValueChange;

            SetHPValue(Convert.ToInt32(_displayHPValue));
        }
        else if(_displayHPValue < _HPValue)
        {
            _displayHPValue += rateOfValueChange;

            SetHPValue(Convert.ToInt32(_displayHPValue));
        }
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.transform.forward);
    }

    public bool GetHit(int hitDMG)
    {
        _HPValue -= hitDMG;

        if (_HPValue > 0)
        {
            return true;
        }
        else
        {
            _HPValue = 0;
            return false;
        }
    }

    public void GetHP(int HPvalue)
    {
        _HPValue += HPvalue;

        if(_HPValue > slider.maxValue)
        {
            _HPValue = (int)slider.maxValue;
        }
    }

    void SetHPValue(int setValue)
    {
        HPValueText.text = $"{setValue}";
        slider.value = setValue;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
