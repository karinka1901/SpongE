using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;

public class BallController : MonoBehaviour
{
    public float ballSpeed = 5f;
    public Rigidbody2D rb;
    public float xDirection;
    public float yDirection;
    public bool goalHit = false; 
    public Animator ballAnimator;

    public int leftScore;
    public int rightScore;

    public ScoreManager scoreManager;


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

       // DebugUtils.LogColor($"[BallController.cs]Ball launched with velocity: {rb.velocity}", "grey");
        Physics2D.SyncTransforms();
        //ballReset = false;
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
            rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y).normalized * ballSpeed;
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
