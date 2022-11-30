using UnityEngine;

public class CheckForCollision : MonoBehaviour
{
    bool touchingGround;
    Agent agent;

    private void Awake()
    {
        touchingGround = false;
        agent = gameObject.GetComponentInParent<Agent>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            touchingGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            touchingGround = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Laser")
        {
            agent.touchedLaser = true;
        }
    }

    public bool IsTouchingGround()
    {
        if (touchingGround)
        {
            return true;
        }

        return false;
    }
}
