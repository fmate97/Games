using UnityEngine;

public class ItemBoxController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] bool decoyBox;
    [SerializeField] GameObject box;
    [SerializeField] GameObject destoryBox;
    [SerializeField] GameObject insideItem;

    private bool _afterCrash = false;
    private string _crashBool = "Crash";
    private string _playerWeaponTag = "PlayerWeapon", _playerTag = "Player";
    private BoxCollider _boxCollider;

    void Start()
    {
        box.SetActive(true);
        destoryBox.SetActive(false);

        if (!decoyBox)
        {
            insideItem.SetActive(false);
        }

        _boxCollider = transform.GetComponent<BoxCollider>();
    }

    void LateUpdate()
    {
        if (decoyBox)
        {
            if (_afterCrash)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            if (_afterCrash && !insideItem.activeSelf)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_playerTag) && collision.contacts[0].normal.y != 0f)
        {
            CrashAnimationStart();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(_playerWeaponTag))
        {
            PlayerController playerController = other.gameObject.GetComponentInParent<PlayerController>();

            if (playerController.PlayerIsAttacking)
            {
                CrashAnimationStart();
            }
        }
    }

    void CrashAnimationStart()
    {
        box.SetActive(false);
        destoryBox.SetActive(true);

        if (!decoyBox)
        {
            insideItem.SetActive(true);
        }

        animator.SetBool(_crashBool, true);
        _boxCollider.enabled = false;
    }

    void CrashAnimationEnd()
    {
        _afterCrash = true;
        destoryBox.SetActive(false);
    }
}
