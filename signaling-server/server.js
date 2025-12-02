import express from "express";
import { WebSocketServer } from "ws";
import http from "http";
// import https from "https";
// import fs from "fs";

const app = express();

// const options = {
//   key: fs.readFileSync("certificate-key.pem"),
//   cert: fs.readFileSync("certificate.pem"),
// };

// const server = https.createServer(
//   options,
//   (req, res) => {
//     res.end("Hello from HTTPS!");
//   },
//   app
// );

const server = http.createServer(app);

// Attach WebSocket server to HTTPS
const wss = new WebSocketServer({ server });

// Handle WebSocket connections
wss.on("connection", (socket, req) => {
  console.log("Secure client connected:", req.socket.remoteAddress);

  socket.on("message", (msg) => {
    console.log("Received:", msg.toString());

    // Broadcast to all other clients
    wss.clients.forEach((client) => {
      if (client !== socket && client.readyState === 1) {
        client.send(msg.toString());
      }
    });
  });

  socket.on("close", () => {
    console.log("Client disconnected");
  });
});

// Example REST endpoint
app.get("/", (req, res) => {
  res.send("Express + Secure WebSocket signaling server running");
});

// Listen on LAN‑reachable port
server.listen(3000, "0.0.0.0", () => {
  console.log("Server listening on http://0.0.0.0:3000");
});
