using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] Animator animator;
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed;
    [SerializeField] float runSpeedMultiplier;
    [SerializeField] float rotationSpeed;
    [Header("Jump Settings")]
    [SerializeField] float buttonTime;
    [SerializeField] float jumpHeight;
    [SerializeField] float fallingSpeed;
    [Header("Offroad Settings")]
    [SerializeField] GameObject roadStartPoint;
    [SerializeField] GameObject roadEndPoint;

    private bool _jump = false, _doubleJumping = false;
    private float _gravityScale = 1f, _runSpeed = 0f, _jumpTime = 0f;
    private string _roadTag = "Roads", _terrainTag = "Terrain", _damagedRoadTag = "DamagedRoads";
    private string _isWalkingBool = "isWalking", _isSprintingBool = "isSprinting", _isJumpingBool = "isJumping";
    private string _isNormalJumpingTrigger = "isNormalJumping", _isDoubleJumpingTrigger = "isDoubleJumping";
    private Vector3 _lastOnRoadPosition;

    void Start()
    {
        animator.SetBool(_isWalkingBool, false);
        animator.SetBool(_isSprintingBool, false);
        animator.SetBool(_isJumpingBool, false);

        _runSpeed = movementSpeed * runSpeedMultiplier;
    }

    void Update()
    {
        Vector3 move = Vector3.zero;

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            move += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            move += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            move += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            move += Vector3.right;
        }

        _jumpTime += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space) && !_jump && _jumpTime >= buttonTime)
        {
            _jump = true;
            _doubleJumping = false;

            animator.SetTrigger(_isNormalJumpingTrigger);
            animator.SetBool(_isJumpingBool, true);

            float jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics.gravity.y * _gravityScale));
            rigidBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
        else if(Input.GetKeyDown(KeyCode.Space) && _jump && !_doubleJumping)
        {
            _doubleJumping = true;

            animator.SetTrigger(_isDoubleJumpingTrigger);
            animator.SetBool(_isJumpingBool, true);

            float jumpForce = Mathf.Sqrt(jumpHeight * -2 * (Physics.gravity.y * _gravityScale));
            rigidBody.velocity = Vector3.zero;
            rigidBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
        if(_jump && rigidBody.velocity.y < 0.1f)
        {
            rigidBody.AddForce(new Vector3(0, -fallingSpeed, 0), ForceMode.Impulse);
        }

        if (move.Equals(Vector3.zero))
        {
            if (!_jump)
            {
                animator.SetBool(_isWalkingBool, false);
                animator.SetBool(_isSprintingBool, false);
                animator.SetBool(_isJumpingBool, false);
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
                    animator.SetBool(_isJumpingBool, false);
                }

                transform.Translate(move * _runSpeed * Time.deltaTime, Space.World);
            }
            else
            {
                if (!_jump)
                {
                    animator.SetBool(_isWalkingBool, true);
                    animator.SetBool(_isSprintingBool, false);
                    animator.SetBool(_isJumpingBool, false);
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_roadTag))
        {
            _jump = false;
            _jumpTime = 0f;

            _lastOnRoadPosition = transform.position;
        }
        else if (collision.gameObject.CompareTag(_damagedRoadTag))
        {
            _jump = false;
            _jumpTime = 0f;
        }
        else if (collision.gameObject.CompareTag(_terrainTag))
        {
            _jump = false;
            _jumpTime = 0f;

            Invoke("PlayerRespawn", 1f);
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
    }
}
