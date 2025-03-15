using UnityEngine;

public class BallController : MonoBehaviour
{
    public float ballSpeed = 5f; 
    public float xDirection;
    public float yDirection;
    
    public Animator ballAnimator;
    public Rigidbody2D rb;

    public int leftScore;
    public int rightScore;
    
    public bool goalHit = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ballAnimator = GetComponent<Animator>();

        rightScore = ScoreManager.Instance.rightScore;
        leftScore = ScoreManager.Instance.leftScore; 
    }

    public void LaunchBall()
    {
        xDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        yDirection = Random.Range(-0.75f, 0.75f);

        Vector2 direction = new Vector2(xDirection, yDirection).normalized;

        rb.velocity = direction * ballSpeed;
        rb.WakeUp(); 

        Physics2D.SyncTransforms();

    }

    public void ResetBall()
    {
        rb.velocity = Vector2.zero;
        ballAnimator.Play("goalHit");
        rb.position = Vector2.zero;
        DebugUtils.LogColor($"[BallController.cs]Ball reset", "grey");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            Vector2 vel;
            vel.x = -rb.velocity.x; 
            vel.y = (rb.velocity.y / 2) + (collision.collider.attachedRigidbody.velocity.y / 3); 
            rb.velocity = vel; 
        }
      
        else if (collision.gameObject.CompareTag("LeftGoal"))
        {
            goalHit = true;

            ScoreManager.Instance.UpdateScore("right"); // Right player scores

            DebugUtils.LogColor($"[BallController.cs]Left goal hit, right paddle scores", "red");
            ResetBall();

        }
        else if (collision.gameObject.CompareTag("RightGoal"))
        {   
            goalHit = true;
            ScoreManager.Instance.UpdateScore("left"); // Left player scores

            DebugUtils.LogColor($"[BallController.cs]reight goal hit, left paddle scores", "blue");
            ResetBall();
            
            
        }
    }
}
