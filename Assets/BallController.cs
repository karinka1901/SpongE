using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;

public class BallController : MonoBehaviour
{
    public float ballSpeed = 5f;
    public Rigidbody2D rb;
    public float xDirection;
    public float yDirection;
    public int leftScore = 0;
    public int rightScore = 0;
    public bool goalHit = false;

    public Animator ballAnimator;
    
   // public bool ballReset = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ballAnimator = GetComponent<Animator>();
    }

    public void LaunchBall()
    {
        xDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        yDirection = Random.Range(-0.75f, 0.75f);

        Vector2 direction = new Vector2(xDirection, yDirection).normalized;

        rb.velocity = direction * ballSpeed;
        rb.WakeUp(); // Ensure Rigidbody is awake

        DebugUtils.LogColor($"[BallController.cs]Ball launched with velocity: {rb.velocity}", "grey");
        Physics2D.SyncTransforms();
        //ballReset = false;
    }

    public void ResetBall()
    {
        rb.velocity = Vector2.zero;
        ballAnimator.Play("goalHit");
        rb.position = Vector2.zero;
        DebugUtils.LogColor($"[BallController.cs]Ball reset", "yellow");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y).normalized * ballSpeed;
        }
        //else if (collision.gameObject.CompareTag("Wall"))
        //{
        //    rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y);
        //}
        else if (collision.gameObject.CompareTag("LeftGoal"))
        {
            goalHit = true;
            leftScore++;

            DebugUtils.LogColor($"[BallController.cs]Left goal hit", "red");
            ResetBall();
            
            

        }
        else if (collision.gameObject.CompareTag("RightGoal"))
        {   
            goalHit = true;
            rightScore++;
            DebugUtils.LogColor($"[BallController.cs]reight goal hit", "blue");
            ResetBall();
            
            
        }
    }
}
