using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Animator animator;

    private bool _camIsMoving = false;
    private string _playerTag = "Player", _moveLeftTrigger = "MoveLeft", _moveRightTrigger = "MoveRight";
    private GameObject _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
        transform.position = _player.transform.position;
    }

    void LateUpdate()
    {
        transform.position = _player.transform.position;

        if (!_camIsMoving)
        {
            if (Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
            {
                animator.SetTrigger(_moveRightTrigger);
                _camIsMoving = true;
            }
            else if (!Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
            {
                animator.SetTrigger(_moveLeftTrigger);
                _camIsMoving = true;
            }
        }
    }

    void MoveEnd()
    {
        _camIsMoving = false;
    }
}
