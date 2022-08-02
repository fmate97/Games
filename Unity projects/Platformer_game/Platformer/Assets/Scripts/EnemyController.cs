using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] Animator animator;
    [SerializeField] GameObject[] waypoints;
    [SerializeField] float walkingMovementSpeed;
    [SerializeField] float chasingMovemenetSpeed;
    [Header("Player Following Settings")]
    [SerializeField] float playerDetectionDistance;
    [SerializeField] float stopPlayerFollowingDistance;
    [SerializeField] GameObject stopFollwingIfReachThisPointMin;
    [SerializeField] GameObject stopFollwingIfReachThisPointMax;
    [SerializeField] bool stopFollowingXCoordinate;
    [SerializeField] bool stopFollowingYCoordinate;
    [SerializeField] bool stopFollowingZCoordinate;
    [Header("Attack Settings")]
    [SerializeField] float attackCooldown;
    [SerializeField] public int attackDamage;
    [Header("HP Settings")]
    [SerializeField] int enemyMaxHealth;
    [SerializeField] GameObject healthBar;

    [Header("Public Variables")]
    public bool EnemyIsAttacking = false;

    private bool _playerInRange, _stopMovement = false, _isDying = false, _isAttacking = false;
    private int _waypointIndex = 0;
    private float stopFollowingIfReachThisMinCoordinate = 0f, stopFollowingIfReachThisMaxCoordinate = 0f;
    private float getAttackTime = 0f, _attackTime = 0f;
    private string _playerTag = "Player", _playerWeaponTag = "PlayerWeapon", _playerShieldTag = "PlayerShield";
    private string _isWalkingBool = "isWalking", _isDyingBool = "isDying", _isAttackingBool = "isAttacking";
    private GameObject _player;
    private PlayerController _playerController;
    private EnemyHPBarController _enemyHPBarController;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
        _playerController = _player.GetComponent<PlayerController>();
        transform.position = waypoints[_waypointIndex].transform.position;
        _playerInRange = PlayerIsInDetectionDistance();

        _attackTime = attackCooldown + 1f;

        _enemyHPBarController = healthBar.GetComponent<EnemyHPBarController>();
        _enemyHPBarController.HPBarInit(enemyMaxHealth);
        healthBar.SetActive(_playerInRange);

        if (stopFollowingXCoordinate && !stopFollowingYCoordinate && !stopFollowingZCoordinate)
        {
            stopFollowingIfReachThisMinCoordinate = stopFollwingIfReachThisPointMin.transform.position.x;
            stopFollowingIfReachThisMaxCoordinate = stopFollwingIfReachThisPointMax.transform.position.x;
        }
        else if (!stopFollowingXCoordinate && stopFollowingYCoordinate && !stopFollowingZCoordinate)
        {
            stopFollowingIfReachThisMinCoordinate = stopFollwingIfReachThisPointMin.transform.position.y;
            stopFollowingIfReachThisMaxCoordinate = stopFollwingIfReachThisPointMax.transform.position.y;
        }
        else if (!stopFollowingXCoordinate && !stopFollowingYCoordinate && stopFollowingZCoordinate)
        {
            stopFollowingIfReachThisMinCoordinate = stopFollwingIfReachThisPointMin.transform.position.z;
            stopFollowingIfReachThisMaxCoordinate = stopFollwingIfReachThisPointMax.transform.position.z;
        }
        else
        {
            Debug.LogError($"{transform.name} gameobject -> EnemyController script -> Stop Following bool incorrect!");
        }
    }

    void Update()
    {
        if (!_isDying && !_isAttacking)
        {
            getAttackTime += Time.deltaTime;
            _attackTime += Time.deltaTime;

            _playerInRange = PlayerIsInDetectionDistance();
            healthBar.SetActive(_playerInRange);

            BasicMove();
        }
    }

    void BasicMove()
    {
        Vector3 targetPosition;

        if (_playerInRange)
        {
            targetPosition = new Vector3(
                _player.transform.position.x,
                transform.position.y,
                _player.transform.position.z);
        }
        else
        {
            if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) <= .1f)
            {
                _waypointIndex++;
            }
            if (_waypointIndex >= waypoints.Length)
            {
                _waypointIndex = 0;
            }
            targetPosition = waypoints[_waypointIndex].transform.position;
        }

        if (!_stopMovement)
        {
            transform.LookAt(targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * (_playerInRange ? chasingMovemenetSpeed : walkingMovementSpeed));
            animator.SetBool(_isWalkingBool, true);
        }
        else
        {
            animator.SetBool(_isWalkingBool, false);
        }
    }

    bool PlayerIsInDetectionDistance()
    {
        float distance = Vector3.Distance(transform.position, new Vector3(
                _player.transform.position.x,
                transform.position.y,
                _player.transform.position.z));

        if (stopFollowingXCoordinate &&
            (_player.transform.position.x < stopFollowingIfReachThisMinCoordinate ||
            _player.transform.position.x > stopFollowingIfReachThisMaxCoordinate))
        {
            if (_playerInRange)
            {
                FindTheNearestWaypoint();
            }

            return false;
        }
        else if (stopFollowingYCoordinate &&
            (_player.transform.position.y < stopFollowingIfReachThisMinCoordinate ||
            _player.transform.position.y > stopFollowingIfReachThisMaxCoordinate))
        {
            if (_playerInRange)
            {
                FindTheNearestWaypoint();
            }

            return false;
        }
        else if (stopFollowingZCoordinate &&
            (_player.transform.position.z < stopFollowingIfReachThisMinCoordinate ||
            _player.transform.position.z > stopFollowingIfReachThisMaxCoordinate))
        {
            if (_playerInRange)
            {
                FindTheNearestWaypoint();
            }

            return false;
        }

        if (distance <= playerDetectionDistance)
        {
            if (distance <= stopPlayerFollowingDistance)
            {
                _stopMovement = true;

                if (_attackTime >= attackCooldown)
                {
                    AttackAnimationStart();
                }
            }
            else _stopMovement = false;

            return true;
        }
        else
        {
            if (_playerInRange)
            {
                FindTheNearestWaypoint();
            }

            return false;
        }
    }

    void FindTheNearestWaypoint()
    {
        float nearestWaypointDistance = int.MaxValue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, waypoints[i].transform.position);
            if (distance < nearestWaypointDistance)
            {
                nearestWaypointDistance = distance;
                _waypointIndex = i;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_playerWeaponTag)
            && _playerController.PlayerIsAttacking
            && getAttackTime >= 0.85f)
        {

            Debug.Log(getAttackTime);

            getAttackTime = 0f;
            if (!_enemyHPBarController.GetHit(_playerController.attackDamage))
            {
                DieAnimationStart();
            }
        }
        else if (other.gameObject.CompareTag(_playerShieldTag))
        {
            EnemyIsAttacking = false;
        }
    }

    void AttackAnimationStart()
    {
        _attackTime = 0f;

        EnemyIsAttacking = true;
        _isAttacking = true;

        animator.SetBool(_isAttackingBool, true);
    }

    void AttackAnimationEnd()
    {
        EnemyIsAttacking = false;
        _isAttacking = false;

        animator.SetBool(_isAttackingBool, false);
    }

    void DieAnimationStart()
    {
        _isDying = true;

        gameObject.GetComponent<Rigidbody>().useGravity = false;
        gameObject.GetComponent<BoxCollider>().enabled = false;

        animator.SetBool(_isDyingBool, true);
    }

    void DieAnimationEnd()
    {
        healthBar.SetActive(false);

        Destroy(gameObject, 2f);
    }
}
