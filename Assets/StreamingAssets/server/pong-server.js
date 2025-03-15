import { Server } from "socket.io";
import chalk from "chalk";
const io = new Server(3000);
import db from "./database-api.js";




let players = [];
let paddles = [];
let ballPosVel = { x: 0, y: 0, vx: 0, vy: 0 };
let ballOwner = null;
let scores = { left: 0, right: 0 }; 


io.on("connection", (socket) => {

    if (!ballOwner) {
        ballOwner = socket.id;
        console.log(chalk.bgCyan(`Player ${socket.id} is the ball owner`));
    }
    io.emit("setBallOwner", { owner: ballOwner });
    

    socket.on("requestRole", () => {
        let assignedRole;
        const leftExists = players.some(player => player.role === "left");

        if (!leftExists) {
            assignedRole = "left";
        } else {
            assignedRole = "right";
        }
        players.push({ id: socket.id, role: assignedRole });

        console.log(`Player ${socket.id} assigned as ${assignedRole}`);
        socket.emit("assignRole", assignedRole);
    //    io.emit("updatePlayers", players);
    });

    socket.on("playerNameAssigned", (data) => {
        if (!data.role || !data.name) {
            console.log(chalk.red("Invalid player data received for name assignment!"));
            return;
        }
    
        players[socket.id] = { role: data.role, name: data.name };
    
        console.log(chalk.blue(`Player assigned: ${data.role} - ${data.name}`));
    
        // Send back to all clients without unnecessary array wrapping
        io.emit("playerNameAssigned", { role: data.role, name: data.name });
    });
    
    socket.on("gameStart", () => {
        io.emit("gameStart"); // Notify all players
    });
    
    //BALL LOGIC
    socket.on("spawnBall", () => {
        console.log(chalk.bold("Ball spawned"));
        if (socket.id === ballOwner) {
            console.log(`Player ${socket.id} is the ball owner`);
            io.emit("spawnBall"); // Only send spawnBall once to all clients
        }
    });

    socket.on("updateBall", (data) => {
        if (socket.id === ballOwner) {
            ballPosVel = data;
           // console.log(chalk.bgYellow(`Ball owner ${socket.id} sent position: `, JSON.stringify(data, null, 2)));
            //console.log(chalk.bgGreen(`Ball owner ${socket.id} sent position: `, JSON.stringify(ballPosVel, null, 2)));
            io.emit("syncPositionVelocity", ballPosVel);
        }
    });

 // PADDLE LOGIC
     socket.on("spawnPaddle", (role) => {
        paddles.push({ paddle: role });
        console.log(chalk.blue(`Paddle spawned for ${role} player`));

        io.emit("spawnPaddle", { role: role });
    });

    socket.on("movePaddle", (data) => {
        const player = players.find(p => p.id === socket.id);
        if (player) {
            player.y = data.y;

          //  console.log(chalk.bgMagenta(`Player ${socket.id} moved paddle to ${data.y}`));
            io.emit("updatePaddle", { role: player.role, y: data.y });
        }
    });

    socket.on("hideStartText", () => {
        io.emit("hideStartText"); // Sends event to all players
    });
    
    socket.on("updateScore", (data) => {
        if (socket.id === ballOwner) { 
            scores.left = data.left;
            scores.right = data.right;
    
            console.log(chalk.yellow(`Score Update - Left: ${scores.left}, Right: ${scores.right}`));
    
            io.emit("updateScore", scores);
    
            // win condition
            if (scores.left >= 3) {
                io.emit("gameOver", { winner: "left" });
                console.log(chalk.bgRedBright("Game Over - Left player wins"));
            } else if (scores.right >= 3) {
                io.emit("gameOver", { winner: "right" });
                console.log(chalk.bgRedBright("Game Over - Right player wins"));
            }
        } else {
            console.log(chalk.grey("Ignored duplicate updateScore request from non-owner"));
        }
    });
    
    // Handle player disconnect
    socket.on("disconnect", () => {
        console.log(`Player ${socket.id} disconnected`);
        if (socket.id === ballOwner) {
            ballOwner = null; // Reset ball ownership
        }
        // Remove player and their paddle from the arrays
        players = players.filter(player => player.id !== socket.id);
     //   paddles = paddles.filter(p => p.paddle !== player.role);

        io.emit("updatePlayers", players);
      //  io.emit("spawnedPaddle", paddles);
    });
});

console.log(chalk.red("Server running on port 3000"));
