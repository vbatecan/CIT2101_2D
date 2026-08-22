using UnityEngine;

public class PlayerController : MonoBehaviour
{

    private bool isInWater = false;
    private bool isGoForward = false;
    public float forwardSpeed = 2.0f;
    public float upwardSpeed = 10f;
    public Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        bool isWater = collision.gameObject.layer == LayerMask.NameToLayer("Water");

        if (isWater)
        {
            Debug.Log("Player has entered the water");
            isInWater = true;
            isGoForward = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        bool isWater = collision.gameObject.layer == LayerMask.NameToLayer("Water");

        if (isWater)
        {
            Debug.Log("Player has entered the water");
            isInWater = true;
            isGoForward = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        bool isWater = collision.gameObject.layer == LayerMask.NameToLayer("Water");

        if (isWater)
        {
            Debug.Log("Player has exited the water");
            isGoForward = false;
            isInWater = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isGoForward)
        {
            rb.linearVelocity = new Vector2(forwardSpeed, rb.linearVelocityY);
        }

        // If space key is pressed, move the player up
        if (Input.GetKey(KeyCode.Space) && isInWater)
        {
            rb.AddForce(Vector2.up * upwardSpeed, ForceMode2D.Force);
            Debug.Log("Space key is being held down");
        }
    }
}
