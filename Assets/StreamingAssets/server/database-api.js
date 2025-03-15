import express from "express";
import mysql from "mysql";
import cors from "cors";
import chalk from "chalk";

export default {
    connectToDatabase: function() {
        console.log("Database API Loaded and Connected!");
    }
};

const app = express();
app.use(cors());

app.use(express.urlencoded({ extended: true })); 
app.use(express.json()); 

const port = process.env.PORT || 5000;

// MySQL Database Connection
const db = mysql.createConnection({
    host: 'localhost',
    user: 'root',
    password: '',
    database: 'ponggame'
});

db.connect(err => {
    if (err) {
        console.error(chalk.red('Database connection failed:', err.stack));
        return;
    }
    console.log(chalk.bgBlue('Connected to MySQL Database'));
});

// Adding/Updating Players and Scores
app.post('/api/update-score', (req, res) => {
    console.log("Received POST request:", req.body); 

    const username = req.body.playerName; // playerName from Unity
    const score = parseInt(req.body.score, 10) / 2; // score from Unity

    if (!username || isNaN(score)) {
        console.log(chalk.red("Error: Missing username or invalid score"));
        return res.status(400).json({ error: "Missing username or invalid score" });
    }

    // Check if player exists in the database
    db.query("SELECT * FROM players WHERE username = ?", [username], (err, result) => {
        if (err) {
            console.error("Database SELECT error:", err);
            return res.status(500).json({ error: err.message });
        }

        if (result.length > 0) {
            // Player exists, update their score
            db.query(
                "UPDATE players SET score = COALESCE(score, 0) + ? WHERE username = ?",
                [score, username],
                (err, updateResult) => {
                    if (err) {
                        console.error("Database UPDATE error:", err);
                        return res.status(500).json({ error: err.message });
                    }
                    res.json({ message: "Score updated!", playerId: result[0].id, updatedScore: score });
                }
            );
        } else {
            // Player does not exist, insert with initial score
            db.query(
                "INSERT INTO players (username, score) VALUES (?, ?)",
                [username, score],
                (err, insertResult) => {
                    if (err) {
                        console.error("Database INSERT error:", err);
                        return res.status(500).json({ error: err.message });
                    }
                    res.json({ message: "Player added!", playerId: insertResult.insertId, initialScore: score });
                }
            );
        }
    });
});

// fetch top 10 players with highest scores
app.get('/api/top-scores', (req, res) => {
    db.query("SELECT username, score FROM players ORDER BY score DESC LIMIT 10", (err, result) => {
        if (err) {
            console.error("Database SELECT error:", err);
            return res.status(500).json({ error: err.message });
        }
        res.json(result); 
    });
});


// Start Server
app.listen(port, () => {
    console.log(chalk.blue(`Server running on port ${port}`));
});
