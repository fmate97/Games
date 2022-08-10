using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy settings")]
    [SerializeField] public float enemyHP = 5f;
    [SerializeField] float attackSpeed = 5f;
    [SerializeField] int damage = 10;
    [SerializeField] int goldValue = 10;
    [Header("Health bar")]
    [SerializeField] GameObject HPBar;
    [SerializeField] Gradient gradient;
    [SerializeField] Image fill;
    [Header("Sound")]
    [SerializeField] AudioSource hitSound;

    public bool attack = false, walk = false, die = false;
    public LevelController levelController;

    private float _deltaTime = 0f;
    private string _attackParam = "attack", _dieParam = "die", _walkParam = "walk", cameraName = "Main Camera";
    private Animator _animator;
    private Slider _slider;
    private Transform _camera;

    void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
        _slider = HPBar.GetComponent<Slider>();
        _slider.maxValue = enemyHP;
        _slider.value = enemyHP;
        fill.color = gradient.Evaluate(_slider.normalizedValue);
        _camera = GameObject.Find(cameraName).transform;
    }

    void Update()
    {
        _deltaTime -= Time.deltaTime;

        if (enemyHP <= 0f && !_animator.GetBool(_dieParam))
        {
            walk = false;
            die = true;
            _animator.SetBool(_dieParam, true);
        }
        else if (attack)
        {
            walk = false;
            if (_deltaTime <= 0f)
            {
                _animator.SetTrigger(_attackParam);
                _deltaTime = attackSpeed;
            }
        }

        _animator.SetBool(_walkParam, walk);
    }

    void LateUpdate()
    {
        HPBar.transform.LookAt(HPBar.transform.position + _camera.forward);
        _slider.value = enemyHP;
        fill.color = gradient.Evaluate(_slider.normalizedValue);
    }

    void DieAnimationEnd()
    {
        levelController.AddGold(goldValue);
        Destroy(gameObject);
    }

    void Attack()
    {
        hitSound.Play();
        levelController.GetHit(damage);
    }
}
