using UnityEngine;

public class TrapChildrenController : MonoBehaviour
{
    private TrapController _trapController;

    void Start()
    {
        _trapController = transform.parent.GetComponent<TrapController>();
    }

    void OnTriggerEnter(Collider other)
    {
        _trapController.OnChieldTriggerEnter(other);
    }
}
