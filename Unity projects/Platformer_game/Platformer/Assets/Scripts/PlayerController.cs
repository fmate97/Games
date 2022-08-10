using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Animator animator;
    [SerializeField] GameObject mainCamera;
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed;
    [SerializeField] float runSpeedMultiplier;
    [SerializeField] float rotationSpeed;
    [Header("Jump Settings")]
    [SerializeField] float jumpButtonTime;
    [SerializeField] float jumpHeight;
    [SerializeField] float fallingSpeed;
    [Header("Offroad Settings")]
    [SerializeField] GameObject roadStartPoint;
    [SerializeField] GameObject roadEndPoint;
    [SerializeField] int offRoadDamage;
    [SerializeField] float fenceDamageTime;
    [SerializeField] int fenceDamage;
    [Header("Attack Settings")]
    [SerializeField] float attackCooldown;
    [SerializeField] public int attackDamage;
    [Header("HP Settings")]
    [SerializeField] int enemyMaxHealth;
    [SerializeField] GameObject healthBar;

    [Header("Public Variables")]
    public bool PlayerIsAttacking = false;

    private bool _jump = false, _doubleJumping = false, _stopMovement = false, _isDying = false, _onFence = false, _isDefending = false;
    private float _gravityScale = 1f, _runSpeed = 0f, _jumpTime = 0f, _attackTime = 0f, _getAttackTime = 0f, _getFenceDamageTime = 0f;
    private string _roadTag = "Roads", _waterTag = "Water", _fenceTag = "Fence", _enemyWeaponTag = "EnemyWeapon", _levelEndTag = "LevelEnd", _enemyTag = "Enemy", _itemBoxTag = "ItemBox", _movingRoadsTag = "MovingRoads", _trapTag = "Trap";
    private string _isWalkingBool = "isWalking", _isSprintingBool = "isSprinting", _isJumpingBool = "isJumping", _isAttackingBool = "isAttacking", _isDyingBool = "isDying", _isDyingStayBool = "isDyingStay", _isDefendBool = "isDefend", _getHitBool = "getHit";
    private string _isNormalJumpingTrigger = "isNormalJumping", _isDoubleJumpingTrigger = "isDoubleJumping", _isAttackingTrigger = "isAttackingTrigger";
    private Vector3 _lastOnRoadPosition;
    private PlayerHPBarController _playerHPBarController;

    void Start()
    {
        animator.SetBool(_isWalkingBool, false);
        animator.SetBool(_isSprintingBool, false);
        animator.SetBool(_isJumpingBool, false);
        animator.SetBool(_isDyingBool, false);

        _playerHPBarController = healthBar.GetComponent<PlayerHPBarController>();
        _playerHPBarController.HPBarInit(enemyMaxHealth);

        _jumpTime = jumpButtonTime + 1f;
        _attackTime = attackCooldown + 1f;
        _runSpeed = movementSpeed * runSpeedMultiplier;
    }

    void Update()
    {
        if (!_isDying)
        {
            _getAttackTime += Time.deltaTime;
            _attackTime += Time.deltaTime;
            _jumpTime += Time.deltaTime;
            _getFenceDamageTime += Time.deltaTime;

            if (_onFence && _getFenceDamageTime >= fenceDamageTime)
            {
                _getFenceDamageTime = 0f;
                if (!_playerHPBarController.GetHit(fenceDamage))
                {
                    DieAnimationStart();
                }
            }

            if (Input.GetMouseButton(1) && !_jump)
            {
                _isDefending = true;
                animator.SetBool(_isDefendBool, true);
                DefendPlayerRotate();
            }
            else if (Input.GetMouseButtonDown(0) && _attackTime >= attackCooldown)
            {
                _attackTime = 0f;

                _stopMovement = true;
                _isDefending = false;
                animator.SetBool(_isDefendBool, false);
                animator.SetBool(_isWalkingBool, false);
                animator.SetBool(_isSprintingBool, false);
                animator.SetBool(_isAttackingBool, true);
                animator.SetTrigger(_isAttackingTrigger);

                PlayerIsAttacking = true;
            }
            else if (!_stopMovement || (_stopMovement && _jump))
            {
                _isDefending = false;
                animator.SetBool(_isDefendBool, false);
                BasicMovement();
            }
            
            if (_jump && !_doubleJumping && rigidBody.velocity.y < 0.1f)
            {
                rigidBody.AddForce(new Vector3(0, -fallingSpeed, 0), ForceMode.Impulse);
            }
            else if (_jump && _doubleJumping && rigidBody.velocity.y < 0.1f)
            {
                rigidBody.AddForce(new Vector3(0, -fallingSpeed/2, 0), ForceMode.Impulse);
            }
        }
    }

    void AttackAnimationEnd()
    {
        _stopMovement = false;
        animator.SetBool(_isAttackingBool, false);
        PlayerIsAttacking = false;
    }

    void BasicMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            move += GetCameraForward();
        }
        if (Input.GetKey(KeyCode.S))
        {
            move += GetCameraBack();
        }
        if (Input.GetKey(KeyCode.A))
        {
            move += GetCameraLeft();
        }
        if (Input.GetKey(KeyCode.D))
        {
            move += GetCameraRight();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !_jump && _jumpTime >= jumpButtonTime)
        {
            _jump = true;
            _doubleJumping = false;

            animator.SetTrigger(_isNormalJumpingTrigger);
            animator.SetBool(_isJumpingBool, true);

            float jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics.gravity.y * _gravityScale));
            rigidBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && _jump && !_doubleJumping)
        {
            _doubleJumping = true;

            animator.SetTrigger(_isDoubleJumpingTrigger);
            animator.SetBool(_isJumpingBool, true);

            float jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics.gravity.y * _gravityScale));
            rigidBody.velocity = Vector3.zero;
            rigidBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }

        if (move.Equals(Vector3.zero))
        {
            if (!_jump)
            {
                animator.SetBool(_isWalkingBool, false);
                animator.SetBool(_isSprintingBool, false);
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (!_jump)
                {
                    animator.SetBool(_isWalkingBool, false);
                    animator.SetBool(_isSprintingBool, true);
                }

                transform.Translate(move * _runSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                if (!_jump)
                {
                    animator.SetBool(_isWalkingBool, true);
                    animator.SetBool(_isSprintingBool, false);   
                }

                transform.Translate(move * movementSpeed * Time.deltaTime, Space.World);
            }
        }

        if (!move.Equals(Vector3.zero))
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void DefendPlayerRotate()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            move += GetCameraForward();
        }
        if (Input.GetKey(KeyCode.S))
        {
            move += GetCameraBack();
        }
        if (Input.GetKey(KeyCode.A))
        {
            move += GetCameraLeft();
        }
        if (Input.GetKey(KeyCode.D))
        {
            move += GetCameraRight();
        }

        if (!move.Equals(Vector3.zero))
        {
            Quaternion toRotation = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    Vector3 GetCameraForward()
    {
        float rotationY = mainCamera.transform.rotation.y;

        if (rotationY >= -0.1f && rotationY <= 0.1f)
            return new Vector3(0, 0, 1);
        else if (rotationY >= 0.65 && rotationY <= 0.75f)
            return new Vector3(1, 0, 0);
        else if (rotationY <= -0.65 && rotationY >= -0.75f)
            return new Vector3(-1, 0, 0);
        else if (rotationY >= 0.9 && rotationY <= 1.1f)
            return new Vector3(0, 0, -1);
        else
            return new Vector3(0, 0, 0);
    }

    Vector3 GetCameraBack()
    {
        float rotationY = mainCamera.transform.rotation.y;

        if (rotationY >= -0.1f && rotationY <= 0.1f)
            return new Vector3(0, 0, -1);
        else if (rotationY >= 0.65 && rotationY <= 0.75f)
            return new Vector3(-1, 0, 0);
        else if (rotationY <= -0.65 && rotationY >= -0.75f)
            return new Vector3(1, 0, 0);
        else if (rotationY >= 0.9 && rotationY <= 1.1f)
            return new Vector3(0, 0, 1);
        else
            return new Vector3(0, 0, 0);
    }

    Vector3 GetCameraLeft()
    {
        float rotationY = mainCamera.transform.rotation.y;

        if (rotationY >= -0.1f && rotationY <= 0.1f)
            return new Vector3(-1, 0, 0);
        else if (rotationY >= 0.65 && rotationY <= 0.75f)
            return new Vector3(0, 0, 1);
        else if (rotationY <= -0.65 && rotationY >= -0.75f)
            return new Vector3(0, 0, -1);
        else if (rotationY >= 0.9 && rotationY <= 1.1f)
            return new Vector3(1, 0, 0);
        else
            return new Vector3(0, 0, 0);
    }

    Vector3 GetCameraRight()
    {
        float rotationY = mainCamera.transform.rotation.y;

        if (rotationY >= -0.1f && rotationY <= 0.1f)
            return new Vector3(1, 0, 0);
        else if (rotationY >= 0.65 && rotationY <= 0.75f)
            return new Vector3(0, 0, -1);
        else if (rotationY <= -0.65 && rotationY >= -0.75f)
            return new Vector3(0, 0, 1);
        else if (rotationY >= 0.9 && rotationY <= 1.1f)
            return new Vector3(-1, 0, 0);
        else
            return new Vector3(0, 0, 0);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_roadTag))
        {
            _onFence = false;

            if (_jump)
            {
                _jump = false;
                _jumpTime = 0f;
                animator.SetBool(_isJumpingBool, false);
            }

            _lastOnRoadPosition = transform.position;
            rigidBody.velocity = Vector3.zero;
        }
        else if (collision.gameObject.CompareTag(_movingRoadsTag))
        {
            _onFence = false;
            if (_jump)

            {
                _jump = false;
                _jumpTime = 0f;
                animator.SetBool(_isJumpingBool, false);
            }

            rigidBody.velocity = Vector3.zero;
        }
        else if (collision.gameObject.CompareTag(_itemBoxTag))
        {
            _onFence = false;

            if (_jump)
            {
                _jump = false;
                _jumpTime = 0f;
                animator.SetBool(_isJumpingBool, false);
            }

            rigidBody.velocity = Vector3.zero;
        }
        else if (collision.gameObject.CompareTag(_fenceTag))
        {
            _onFence = false;

            if (_jump)
            {
                _jump = false;
                _jumpTime = 0f;
                animator.SetBool(_isJumpingBool, false);
            }

            if (collision.contacts[0].normal.y >= 0.5f)
            {
                _onFence = true;
            }

            rigidBody.velocity = Vector3.zero;
        }
        else if (collision.gameObject.CompareTag(_waterTag))
        {
            _onFence = false;

            if (_jump)
            {
                _jump = false;
                _jumpTime = 0f;
                animator.SetBool(_isJumpingBool, false);
            }

            PlayerRespawn();
        }
        else if (collision.gameObject.CompareTag(_trapTag))
        {
            _onFence = false;

            if (_jump)
            {
                _jump = false;
                _jumpTime = 0f;
                animator.SetBool(_isJumpingBool, false);
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(_roadTag))
        {
            _lastOnRoadPosition = transform.position;
        }
    }

    void PlayerRespawn()
    {
        if (_lastOnRoadPosition.z < roadStartPoint.transform.position.z) transform.position = roadStartPoint.transform.position;
        else if (_lastOnRoadPosition.z > roadEndPoint.transform.position.z) transform.position = roadEndPoint.transform.position;
        else transform.position = _lastOnRoadPosition;
        transform.eulerAngles = new Vector3(0, 90, 0);

        if (!_playerHPBarController.GetHit(offRoadDamage))
        {
            DieAnimationStart();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_levelEndTag))
        {
            LoadNextLevel();
        }
        else if (other.gameObject.CompareTag(_enemyWeaponTag) && _getAttackTime >= .25f)
        {
            EnemyController enemyController = other.gameObject.GetComponentInParent<EnemyController>();
            if (enemyController.EnemyIsAttacking)
            {
                _getAttackTime = 0f;
                int getDamage = enemyController.attackDamage;

                if (!_isDefending)
                {                    
                    if (!_playerHPBarController.GetHit(getDamage))
                    {
                        DieAnimationStart();
                    }
                    else
                    {
                        _stopMovement = true;
                        animator.SetBool(_getHitBool, true);
                    }
                }
                else
                {
                    Vector3 toTarget = (other.gameObject.transform.position - transform.position).normalized;
                    float dot = Vector3.Dot(toTarget, gameObject.transform.forward);

                    Debug.Log(dot);

                    if (dot <= 0.5f)
                    {
                        if (!_playerHPBarController.GetHit(getDamage))
                        {
                            DieAnimationStart();
                        }
                        else
                        {
                            _stopMovement = true;
                            animator.SetBool(_getHitBool, true);
                        }
                    }

                    _stopMovement = true;
                    animator.SetBool(_getHitBool, true);
                }
            }
        }
    }

    void DieAnimationStart()
    {
        _isDying = true;
        animator.SetBool(_isDyingBool, true);
    }

    void DieAnimationAlmostEnd()
    {
        animator.SetBool(_isDyingStayBool, true);
    }

    void DieAnimationEnd()
    {
        animator.SetBool(_isDyingStayBool, true);
        Invoke("DestroyPlayer", 2f);
    }

    void GetHitAnimationEnd()
    {
        _stopMovement = false;
        animator.SetBool(_getHitBool, false);
    }

    void DestroyPlayer()
    {
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void LoadNextLevel()
    {
        if(GameObject.FindGameObjectsWithTag(_enemyTag).Length == 0)
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            SceneManager.LoadScene("MainMenu");
        }        
    }
}
