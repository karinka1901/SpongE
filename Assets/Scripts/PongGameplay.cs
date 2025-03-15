using UnityEngine;
using SimpleJSON; // For JSON parsing
using PimDeWitte.UnityMainThreadDispatcher;
using System.Collections;
using System.Collections.Generic;


public class PongGameplay : MonoBehaviour
{
    [SerializeField] private SocketIOUnity socket;
    public UIManager uiManager;
    [SerializeField] private bool gameOver = false;


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
    [SerializeField] private bool isPlayerAssigned = false;
    [SerializeField] private bool isNameAssigned = false;
    [SerializeField] private string playerRole = "";
    [SerializeField] private bool playersReady = false;
   [ SerializeField] private bool gameOn = false;

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
        socket.On("playerNameAssigned", response =>
        {
            JSONNode data = JSON.Parse(response.ToString());
            string role = data[0]["role"];
            string name = data[0]["name"];

            DebugUtils.LogColor($"Received player name: {role} - {name}", "green");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (role == "left")
                {
                    UIManager.Instance.leftPlayerName.text = name;
                    UIManager.Instance.AssignName(name);
                }
                else if (role == "right")
                {
                    UIManager.Instance.rightPlayerName.text = name;
                    UIManager.Instance.AssignName(name);
                }
            });

          //  isNameAssigned = true;
        });

        socket.On("gameStart", response =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (!ballSpawned)
                {
                    SpawnBall();
                }
            });
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

        socket.On("hideStartText", response =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                DebugUtils.LogColor("Hiding start text for all players!", "green");
                uiManager.ActivateUIelement(uiManager.start_txt, false);
            });
        });

        //Score events
        socket.On("updateScore", response =>
        {
            JSONNode data = JSON.Parse(response.ToString());
            int leftScore = data[0]["left"];
            int rightScore = data[0]["right"];
            DebugUtils.LogColor($"Left score: {leftScore}, Right score: {rightScore}", "grey");

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ScoreManager.Instance.SyncScore(leftScore, rightScore);
            });
        });

        socket.On("gameOver", response =>
        {
            JSONNode winner = JSON.Parse(response.ToString());
            string player = winner[0]["winner"];
            DebugUtils.LogColor("Game over!", "red");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GameOver(player);
            });
        });

        socket.Connect();
    }

    //BALL LOGIC
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

        if (leftPaddleSpawned && rightPaddleSpawned)
        {
            hasSpawnedPaddle = true;
           

            // Update UI
            uiManager.ActivateUIelement(uiManager.waiting_txt, false);
            uiManager.ActivateUIelement(uiManager.start_txt, true);
        }



    }

    public void SendPaddleMovement(float yPosition)
    {
        socket.Emit("movePaddle", new
        {
            role = playerRole,
            y = yPosition
        });
    }

    //UI
    public void GameOver(string winner)
    {
        gameOver = true;

        Destroy(spawnedBall);

        uiManager.ActivateUIelement(uiManager.gameOvere_txt, true);
        uiManager.ActivateUIelement(uiManager.playerWon_txt, true);
        uiManager.SetPlayerWonText(winner);

        if (winner == "left")
        {
            ScoreManager.Instance.totalScoreLeft++;
            PongDatabase.Instance.UpdatePlayerScore(UIManager.Instance.leftPlayerName.text, ScoreManager.Instance.totalScoreLeft);
        }
        else if (winner == "right")
        {
            ScoreManager.Instance.totalScoreRight++;
            PongDatabase.Instance.UpdatePlayerScore(UIManager.Instance.rightPlayerName.text, ScoreManager.Instance.totalScoreRight);
        }

    }

    public void StartGame()
    {
        if (hasSpawnedPaddle && !ballSpawned)
        {
            DebugUtils.LogColor("Press space to start", "green");
            socket.Emit("spawnBall");
            socket.Emit("hideStartText");
            socket.Emit("gameStart");
            uiManager.ActivateUIelement(uiManager.start_txt, false);
            gameOn = true;

        }
    }
    public void JoinGame()
    {
        if (isPlayerAssigned && !hasSpawnedPaddle)
        {
            socket.Emit("spawnPaddle", playerRole);
            uiManager.ActivateUIelement(uiManager.join_txt, false);
        }

        if (!hasSpawnedPaddle)
        {
            DebugUtils.LogColor("Waiting for players...", "yellow");
            uiManager.ActivateUIelement(uiManager.waiting_txt, true);
            uiManager.ActivateUIelement(uiManager.start_txt, false);
        }
        else
        {
            DebugUtils.LogColor("Both players are ready! Press space to start.", "green");
            uiManager.ActivateUIelement(uiManager.waiting_txt, false);
            uiManager.ActivateUIelement(uiManager.start_txt, true);
            playersReady = true;
        }
    }


    private void Update()
    {
        if(isPlayerAssigned && UIManager.Instance.nameAssigned && !isNameAssigned)
        {
            UIManager.Instance.AssignName(playerRole);    
            DebugUtils.LogColor($"Assigning name {playerRole} {UIManager.Instance.playerName}", "green");

            socket.Emit("playerNameAssigned", new
            {
                role = playerRole,
                name = UIManager.Instance.playerName

            });
            isNameAssigned = true;
        } //Assignig name

        if (!gameOver)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !gameOn)
            {
                JoinGame();
                StartGame();
                
            } //START GAME

            if (ballSpawned && ballController != null) //ballOwner send position and velocity
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
                            left = ScoreManager.Instance.leftScore,
                            right = ScoreManager.Instance.rightScore
                        });
                    

                } //Scoring and ball respawning
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

        if(gameOver && spawnedBall != null)
        {
            Destroy(spawnedBall);
        } //GAME OVER
        if(gameOver && Input.GetKeyDown(KeyCode.Space))
        {
            gameOver = false;
            UIManager.Instance.ActivateUIelement(uiManager.gameOvere_txt, false);
            UIManager.Instance.ActivateUIelement(uiManager.playerWon_txt, false);
            UIManager.Instance.ActivateUIelement(uiManager.start_txt, true);
            UIManager.Instance.ActivateUIelement(uiManager.waiting_txt, false);
            UIManager.Instance.OpenLeaderboard();
        } //RESTART GAME





    }
}