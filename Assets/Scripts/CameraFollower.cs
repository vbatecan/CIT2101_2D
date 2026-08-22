using UnityEngine;

public class CameraFollower : MonoBehaviour
{

    public GameObject player;
    public float smoothTime = 0.3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            Vector3 newPosition = new Vector3(player.transform.position.x, player.transform.position.y + 2, -10);
            transform.position = Vector3.Lerp(transform.position, newPosition, smoothTime);
        }
    }
}
