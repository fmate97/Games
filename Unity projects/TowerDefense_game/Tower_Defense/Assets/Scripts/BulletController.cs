using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed;
    public float dmg;
    public float range;
    public Vector3 startPoint;
    public GameObject target;
    public GameObject impactEffect;

    private string _enemyTag = "Enemy";

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }
        else if(Vector3.Distance(startPoint, transform.position) >= range)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_enemyTag))
        {
            other.gameObject.GetComponent<EnemyController>().enemyHP -= dmg;

            GameObject effect = (GameObject)Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);

            Destroy(gameObject);
        }
    }
}
