using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBarController : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    [SerializeField] Image fill;
    [SerializeField] Gradient gradient;
    [SerializeField] Slider slider;
    [SerializeField] float rateOfValueChange;

    private int _HPValue = 0;
    private float _displayHPValue = 0f;

    public void HPBarInit(int maxHPValue)
    {
        slider.value = slider.maxValue = maxHPValue;
        _displayHPValue = _HPValue = maxHPValue;
        fill.color = gradient.Evaluate(slider.normalizedValue);
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

    void SetHPValue(int setValue)
    {
        slider.value = setValue;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
