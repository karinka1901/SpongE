using UnityEngine;
using SimpleJSON; // For JSON parsing
using System.Collections.Generic;
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using System.Data;
using UnityEngine.UIElements;

public class PongGameplay : MonoBehaviour
{
    [SerializeField] private SocketIOUnity socket;
    
    [Header("Ball")]
    [SerializeField] private GameObject spawnedBall;
    [SerializeField] private BallController ballController;
    [SerializeField] private Rigidbody2D ballRb;
    public GameObject ballPrefab;
    [SerializeField] private bool ballSpawned = false;
    [SerializeField] private bool isBallOwner = false;

    [Header("Paddles")]
    public GameObject leftPaddlePrefab;
    public GameObject rightPaddlePrefab;
    private GameObject leftPaddle;
    private GameObject rightPaddle;
    private PaddleController leftPaddleController;
    private PaddleController rightPaddleController;
    [SerializeField] private bool leftPaddleSpawned = false;
    [SerializeField] private bool rightPaddleSpawned = false;
    [SerializeField] private bool hasSpawnedPaddle = false;

    [Header("Player Role")]
     private bool isPlayerAssigned = false;
    [SerializeField] private string playerRole = ""; 

    private void Start()
    {
        socket = new SocketIOUnity("http://localhost:3000");

        socket.OnConnected += (sender, e) =>
        {
            DebugUtils.LogColor("Connected to server", "cyan");
            socket.Emit("requestRole"); // Ask the server for a role
        };

        socket.On("assignRole", response =>
        {
            JSONNode data = JSON.Parse(response.ToString());
            playerRole = data[0].Value;

            // playerRole = response.ToString();
            DebugUtils.LogColor($"Assigned role: {playerRole}", "red");
            isPlayerAssigned = true;

        });

        //Paddle events
        socket.On("spawnPaddle", response =>
        {
            JSONNode data = JSON.Parse(response.ToString());
            string role = data[0]["role"];

            DebugUtils.LogColor($"Server requested paddle spawn for {role} player", "yellow");

            UnityMainThreadDispatcher.Instance().Enqueue(() => SpawnPaddle(role));

        });
        socket.On("updatePaddle", response =>
        {
            JSONNode data = JSON.Parse(response.ToString());
            string role = data[0]["role"];
            float yPos = data[0]["y"];
           // DebugUtils.LogColor($"New position for {role} paddle: {yPos}", "grey");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (role == "left")
                {
                    // leftPaddleController.SetPosition(yPos); // Call function in PaddleController
                    leftPaddle.transform.position = new Vector3(leftPaddle.transform.position.x, yPos, leftPaddle.transform.position.z);
                }
                else if (role == "right")
                {
                    // rightPaddleController.SetPosition(yPos); // Call function in PaddleController
                    rightPaddle.transform.position = new Vector3(rightPaddle.transform.position.x, yPos, rightPaddle.transform.position.z);
                }
            });
        });


        //Ball events
        socket.On("setBallOwner", response =>
        {
            JSONNode data = JSON.Parse(response.ToString());
            string ownerId = data[0]["owner"];
            DebugUtils.LogColor($"Ball owner is: {ownerId}", "grey");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (socket.Id == ownerId)
                {
                    isBallOwner = true;
                    DebugUtils.LogColor("I am the ball owner!", "grey");
                }
                else
                {
                    isBallOwner = false;
                }
            });
        });
        socket.On("spawnBall", response =>
        {
            DebugUtils.LogColor("Server requested ball spawn", "yellow");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (spawnedBall == null)
                {
                    SpawnBall();
                }
            });

        });
        socket.On("syncPositionVelocity", response =>
        {
            JSONNode ballPosVel = JSON.Parse(response.ToString());
            float x = ballPosVel[0]["x"];
            float y = ballPosVel[0]["y"];
            float vx = ballPosVel[0]["vx"];
            float vy = ballPosVel[0]["vy"];

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (!isBallOwner && spawnedBall != null)
                {
                    spawnedBall.transform.position = new Vector2(x, y);
                    ballRb.velocity = new Vector2(vx, vy);

                    //  DebugUtils.LogColor($"Synced ball velocity nad position: {ballRb.velocity}{spawnedBall.transform.position}", "blue");
                }
            });
        });

        //Score events
        socket.On("updateScore", response =>
        {
            JSONNode data = JSON.Parse(response.ToString());
            int leftScore = data[0]["left"];
            int rightScore = data[0]["right"];
            DebugUtils.LogColor($"Left score: {leftScore}, Right score: {rightScore}", "grey");
        });

        socket.Connect();
    }


    public void SpawnBall()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (spawnedBall != null) return; // Prevent duplicate ball spawns

            spawnedBall = Instantiate(ballPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            DebugUtils.LogColor("Ball spawned", "blue");
            ballSpawned = true;

            ballRb = spawnedBall.GetComponent<Rigidbody2D>();
            ballController = spawnedBall.GetComponent<BallController>();

            if (isBallOwner) // Only the owner launches the ball
            {
                DebugUtils.LogColor("I am the ball owner, launching ball...", "green");
                StartCoroutine(DelayedLaunch());
            }


        });
    }

    private IEnumerator DelayedLaunch()
    {
        yield return new WaitForFixedUpdate(); // Wait for physics update
        DebugUtils.LogColor("Launching ball after delay...", "magenta");
        ballController.LaunchBall();
        DebugUtils.LogColor($"Sending ball velocity nad position: {ballRb.velocity}{spawnedBall.transform.position}", "magenta");

        socket.Emit("updateBall", new
        {
            x = spawnedBall.transform.position.x,
            y = spawnedBall.transform.position.y,
            vx = ballRb.velocity.x,
            vy = ballRb.velocity.y
        });



    }

    private void RespawnBall()
    {
        //Destroy(spawnedBall);
        Invoke("SpawnBall", 1f);
        DebugUtils.LogColor("Relaunching...", "yellow");
        ballController.goalHit = false; // Reset goal state
    }

    //PADDLES
    public void SpawnPaddle(string role)
    {
        if (role == "left" && !leftPaddleSpawned)
        {
            leftPaddle = Instantiate(leftPaddlePrefab, new Vector3(-3.82f, 0, 0), Quaternion.identity);
            // DebugUtils.LogColor("Left paddle spawned", "yellow");
            leftPaddleController = leftPaddle.GetComponent<PaddleController>();
            leftPaddleSpawned = true;

            if (playerRole == "left")
            {
                leftPaddleController.isLocalPlayer = true;
                DebugUtils.LogColor("I am controlling the LEFT paddle!", "green");
            }

        }
        if (role == "right" && !rightPaddleSpawned)
        {
            rightPaddle = Instantiate(rightPaddlePrefab, new Vector3(3.82f, 0, 0), Quaternion.identity);
            rightPaddleController = rightPaddle.GetComponent<PaddleController>();
            // DebugUtils.LogColor("Right paddle spawned", "yellow");
            rightPaddleSpawned = true;

            if (playerRole == "right")
            {
                rightPaddleController.isLocalPlayer = true;
                DebugUtils.LogColor("I am controlling the RIGHT paddle!", "blue");
            }

        }

        if (leftPaddleSpawned && rightPaddleSpawned) hasSpawnedPaddle = true;

        //  else DebugUtils.LogColor("Paddles are alreaddy spawned!", "red");

    }

    public void SendPaddleMovement(float yPosition)
    {
        socket.Emit("movePaddle", new
        {
            role = playerRole,
            y = yPosition
        });
    }





    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isPlayerAssigned && !hasSpawnedPaddle)
            {
                socket.Emit("spawnPaddle", playerRole);
            }
            if (hasSpawnedPaddle && !ballSpawned)
            {
                DebugUtils.LogColor("Press space to start", "red");
                socket.Emit("spawnBall");
            }
        }


        if (ballSpawned && ballController!=null) //ballOwner send position and velocity
        {
            if (isBallOwner)
            {
                socket.Emit("updateBall", new
                {
                    x = spawnedBall.transform.position.x,
                    y = spawnedBall.transform.position.y,
                    vx = ballRb.velocity.x,
                    vy = ballRb.velocity.y
                });
            } //ball movement
            if (ballController.goalHit)
            {
                Destroy(spawnedBall);
                RespawnBall();
                socket.Emit("updateScore", new
                {
                    left = ballController.leftScore,
                    right = ballController.rightScore
                });
            } //ball respawn
        }

        if (hasSpawnedPaddle)
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                if (playerRole == "left")
                {
                    leftPaddleController.MovePaddle();
                    SendPaddleMovement(leftPaddle.transform.position.y);
                }
                else if (playerRole == "right")
                {
                    rightPaddleController.MovePaddle();
                    SendPaddleMovement(rightPaddle.transform.position.y);

                }

            }
        } //MOVEMENT

     
    }
}