using UnityEngine;

public class LaserController : MonoBehaviour
{
    [SerializeField] float laserSpeed = 1;
    Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        Vector3 addValues = new(laserSpeed, 0);
        transform.position += addValues * Time.deltaTime;
    }

    public void ResetPosition()
    {
        transform.position = originalPosition;
    }
}
