using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI HPValueText;
    [SerializeField] TextMeshProUGUI MaxHPValueText;
    [SerializeField] Gradient gradient;
    [SerializeField] Image fill;
    [SerializeField] float rateOfValueChange = .1f;

    private int _HPValue = 0;
    private float _displayHPValue = 0f;
    private Slider _slider;

    void Start()
    {
        _slider = gameObject.GetComponent<Slider>();
    }

    public void HPBarInit(int maxHPValue)
    {
        _slider.value = _slider.maxValue = maxHPValue;
        HPValueText.text = MaxHPValueText.text = $"{maxHPValue}";
        _displayHPValue = _HPValue = maxHPValue;
        fill.color = gradient.Evaluate(_slider.normalizedValue);
    }

    void Update()
    {
        if (_displayHPValue != _HPValue)
        {
            _displayHPValue -= rateOfValueChange;

            if (_displayHPValue < _HPValue)
            {
                _displayHPValue = _HPValue;
            }

            SetHPValue(Convert.ToInt32(_displayHPValue));
        }
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

    private void SetHPValue(int setValue)
    {
        HPValueText.text = $"{setValue}";
        _slider.value = setValue;
        fill.color = gradient.Evaluate(_slider.normalizedValue);
    }
}
