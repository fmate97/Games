using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    private string _playerTag = "Player";
    private Vector3 _offsetPosition;
    private GameObject _player;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag(_playerTag);
        _offsetPosition = transform.position - _player.transform.position;
    }

    void Update()
    {
        transform.position = _player.transform.position + _offsetPosition;
    }
}
