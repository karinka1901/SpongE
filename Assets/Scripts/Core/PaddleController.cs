using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public float speed = 7f; // Paddle movement speed
    public bool isLocalPlayer = false;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MovePaddle()
    {
        if(!isLocalPlayer) return;

        float move = 0f;

        if (Input.GetKey(KeyCode.W) && this.transform.position.y < 2.6f) move = 1f;
        else if (Input.GetKey(KeyCode.S) && this.transform.position.y > -2.6f) move = -1f;

        transform.Translate(Vector3.up * move * speed * Time.deltaTime);
    }

    public void SetLocalPlayer(bool isLocal)
    {
        isLocalPlayer = isLocal;
    }
}
