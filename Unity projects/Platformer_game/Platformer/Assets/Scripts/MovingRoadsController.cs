using UnityEngine;

public class MovingRoadsController : MonoBehaviour
{
    [SerializeField] GameObject[] waypoints;
    [SerializeField] float movementSpeed;

    private int _waypointIndex = 0;
    private string _playerTag = "Player";
    private Transform _originalPlayerParent;

    void Start()
    {
        transform.position = waypoints[_waypointIndex].transform.position;
        _originalPlayerParent = GameObject.FindGameObjectWithTag(_playerTag).transform.parent;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) <= .1f)
        {
            _waypointIndex++;
        }
        if (_waypointIndex >= waypoints.Length)
        {
            _waypointIndex = 0;
        }

        transform.position = Vector3.MoveTowards(transform.position, waypoints[_waypointIndex].transform.position, Time.deltaTime * movementSpeed);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_playerTag))
        {
            collision.gameObject.transform.SetParent(gameObject.transform, true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(_playerTag))
        {
            collision.gameObject.transform.SetParent(_originalPlayerParent, true);
        }
    }
}
