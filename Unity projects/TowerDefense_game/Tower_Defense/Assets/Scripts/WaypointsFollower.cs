using UnityEngine;

public class WaypointsFollower : MonoBehaviour
{
    public float movementSpeed = 0.1f;
    public GameObject[] waypoints;

    private int _waypointIndex = 0;
    private float _speed = 0f;
    private string _enemyTag = "Enemy";
    private EnemyController _enemyController;
    private GameObject _prevEnemy;

    void Start()
    {
        _speed = movementSpeed;
        _enemyController = gameObject.GetComponent<EnemyController>();
        transform.position = waypoints[_waypointIndex].transform.position;
    }

    void Update()
    {
        if (_waypointIndex >= waypoints.Length)
        {
            movementSpeed = 0f;
            _enemyController.walk = false;
            _enemyController.attack = true;
        }
        else if (movementSpeed > 0f && !_enemyController.die)
        {
            Walk();
        }
        else if (PrevEnemyMoveOrDestroy())
        {
            Invoke("MoveAgain", 1f);
        }
    }

    void Walk()
    {
        _enemyController.walk = true;

        transform.position = Vector3.MoveTowards(transform.position, waypoints[_waypointIndex].transform.position, Time.deltaTime * movementSpeed);

        if (Vector3.Distance(transform.position, waypoints[_waypointIndex].transform.position) <= 0.1f)
        {
            _waypointIndex++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_enemyTag))
        {
            Vector3 toTarget = (other.gameObject.transform.position - transform.position).normalized;
            if (Vector3.Dot(toTarget, gameObject.transform.forward) > 0)
            {
                movementSpeed = 0f;
                _enemyController.walk = false;
                _prevEnemy = other.gameObject;
            }
        }
    }

    private bool PrevEnemyMoveOrDestroy()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag(_enemyTag))
        {
            if (enemy.Equals(_prevEnemy) && (enemy.GetComponent<WaypointsFollower>().movementSpeed == 0f))
            {
                return false;
            }
        }

        return true;
    }

    private void MoveAgain()
    {
        _enemyController.walk = true;
        movementSpeed = _speed;
    }
}
