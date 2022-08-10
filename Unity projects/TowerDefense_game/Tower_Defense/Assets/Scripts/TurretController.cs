using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] float range = 5f;
    [SerializeField] GameObject partToRotate;
    [SerializeField] float rotateSpeed = 20f;
    [Header("Shooting Settings")]
    [SerializeField] float shootingTime = 5f;
    [Header("Bullet Settings")]
    [SerializeField] GameObject[] launchPoints;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject bulletImpactEffect;
    [SerializeField] float bulletDMG = 2f;
    [SerializeField] float bulletSpeed = 1f;
    [Header("Shoot sound")]
    [SerializeField] AudioSource shootSound;

    private float _deltaTime = 0f;
    private string _enemyTag = "Enemy", _hitPointName = "HitPoint";
    private GameObject _target = null;

    private void Start()
    {
        BulletController bulletController = bullet.GetComponent<BulletController>();
        bulletController.impactEffect = bulletImpactEffect;
        bulletController.speed = bulletSpeed;
        bulletController.dmg = bulletDMG;
        bulletController.range = range;
    }


    void Update()
    {  
        GetTarget();
        TowerRotating();

        _deltaTime -= Time.deltaTime;
        if (_deltaTime <= 0f)
        {
            Shoot();
        }
    }

    void GetTarget()
    {
        float nearestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag(_enemyTag))
        {
            float distance = Vector3.Distance(enemy.transform.position, gameObject.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if(nearestEnemy != null && nearestDistance <= range)
        {
            _target = nearestEnemy;
        }
        else
        {
            _target = null;
        }
    }

    void TowerRotating()
    {
        if (_target != null)
        {
            Vector3 direction = _target.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 rotation = Quaternion.Lerp(partToRotate.transform.rotation, lookRotation, Time.deltaTime * rotateSpeed).eulerAngles;
            partToRotate.transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }
    }

    void Shoot()
    {

        if (_target != null)
        {
            float dot = Vector3.Dot(
                (_target.transform.position - partToRotate.transform.position).normalized, 
                partToRotate.transform.forward);

            if (dot >= 0.975f)
            {
                bullet.GetComponent<BulletController>().target = _target.transform.Find(_hitPointName).gameObject;
                foreach (GameObject launchPoint in launchPoints)
                {
                    bullet.GetComponent<BulletController>().startPoint = launchPoint.transform.position;
                    Instantiate(bullet, launchPoint.transform.position, Quaternion.identity);
                }

                _deltaTime = shootingTime;
                shootSound.Play();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
