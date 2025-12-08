# Telemetry Rush XR – Real‑Time Racing Telemetry, Endurance Analysis & Immersive XR Client

![Telemetry Rush Screenshot](https://github.com/user-attachments/assets/8f882783-11f2-46c8-90fd-20e8da8e49f7)

Telemetry Rush XR is the immersive extension of **Telemetry Rush**, originally built during the Toyota Gazoo Racing *HackTheTrack* hackathon.  
The core Windows application replays, visualizes, and analyzes motorsport telemetry data using external Python servers.  
Telemetry Rush XR streams this experience into Meta Quest headsets, turning racing data into an interactive entertainment platform.

---

## ✨ Key Features

- **Telemetry Playback**
  - Streams vehicle telemetry from recorded CSV logs at configurable speeds.
  - Supports play, pause, reverse (WIP), restart (WIP), and seek (WIP).
  - Displays speed, RPM, gear, throttle, braking, steering, lap distance, and more in real time.

- **Vehicle Visualization**
  - Cars rendered along spline tracks with accurate steering and wheel rotation.
  - GPS coordinates converted into Unity positions using `GPSUtils`.
  - Track paths drawn dynamically with `LineRenderer`.
  - Ghost cars show ideal racing line positions (toggleable).

- **Weather Simulation**
  - Real‑time weather data applied to the scene (temperature, humidity, wind, rain).
  - Fog density, skybox tint, rain particles, and sun position adapt to telemetry input.
  - UI shows current weather condition and icon (toggleable).

- **Endurance Racing UI**
  - Displays lap times, sector splits, top speeds, rankings, and flags.
  - Charts visualize lap performance across vehicles, laps, and sectors.
  - Supports top‑10 fastest lap analysis and race result summaries.

- **Leaderboard Tracking**
  - Leaderboard streamed via WebSocket.
  - Shows position, laps completed, gaps to leader/previous, and best lap stats.
  - Updates Unity UI dynamically with color‑coded rows.

- **Camera & Vehicle Selection**
  - Switch between multiple vehicles and camera perspectives.
  - Player‑controlled car mode with cockpit and follow cameras.
  - Records lap and sector times for player car.

- **Modular Servers**
  - **Telemetry Server** (`port 8765`) – streams vehicle + weather frames.
  - **Section Endurance Server** (`port 8766`) – streams lap events.
  - **Leaderboard Server** (`port 8767`) – streams race standings.
  - Lightweight Python scripts using `asyncio` + `websockets`, compiled to `.exe` with PyInstaller.

---

## 🖥️ Windows Application Usage

After building the Unity project, you get a standalone **Windows executable** (`HackTheTrack.exe`).  
Place the Python server executables (`leaderboard_server.exe`, `section_endurance_server.exe`, `telemetry_vehicle_server.exe`) in the same folder as the Unity build.

### Steps to Run
1. **Start the signaling server**  
   - Check [Railway deployment](https://webrtc-signaling-production-8584.up.railway.app/) if the WebRTC signaling server is running.

2. **Run the Unity application and servers**  
   - Double‑click `HackTheTrack.exe`.  
   - The app connects to servers automatically and begins playback.  
   - If servers don’t start, launch them manually and wait until all are running.

3. **Interact with the UI**  
   - Play/Pause/Reverse telemetry playback.  
   - Adjust playback speed.  
   - Toggle weather effects and ghost cars.  
   - View lap times, charts, and leaderboard updates.  
   - Spawn your own car and challenge professional racers.

4. **Quit gracefully**  
   - Press **Esc** to exit.  
   - Servers shut down automatically when playback ends.

---

## 🎮 XR Application Usage

### Installation
1. Download the `.apk` file from [GitHub Releases](https://github.com/FireDragonGameStudio/hack-the-track/releases).
2. Install on your Meta Quest headset using **ADB** or **Meta Quest Hub**.
3. Launch the XR app and connect to the signaling server.

### XR Controls
- **Hand Gestures / Controllers**
  - Touch buttons on UI, to trigger the respective functionalities.
  - Left + Right Thumbtap to show/hide the main UI.
- **UI Interactions**
  - Play/Pause telemetry playback.
  - Toggle "Live Stream".
  - Toggle chart data
  - Switch cameras (cockpit, follow, external).
  - Spawn your own car and challenge racers.
- **Passthrough Mode**
  - Activate passthrough surfaces to stay aware of your environment.

---

## 📖 Project Structure

### Unity Components
- `TelemetryReceiver` – Connects to telemetry server, applies weather data, updates vehicles.
- `TelemetryVehiclePlayer` – Applies telemetry samples to car geometry and UI.
- `TelemetryUI` – Main UI controller for playback and weather toggling.
- `TelemetryVehicleSelector` – Manages vehicle and camera selection.
- `WeatherManager` – Applies weather data to scene effects.
- `SectionEnduranceReceiver` – Receives lap events for analytics.
- `SectionEnduranceUI` – Displays lap times, charts, and race results.
- `LeaderboardReceiver` – Receives race standings and updates UI.
- `GPSUtils` – Converts GPS coordinates into Unity positions.

### Python Servers
- `telemetry_server.py` – Streams telemetry + weather data.
- `section_endurance_server.py` – Streams lap events.
- `leaderboard_server.py` – Streams leaderboard standings.

### Signaling Server
- Node.js/Express + `ws` for WebSocket signaling.
- Deployed on Railway/Fly.io for continuous reachability.
- Provides connection management for XR clients.

---

## 🛠️ Installation (Entertainment Build)
1. Download the `.zip` build for Windows + Quest from [Releases](https://github.com/FireDragonGameStudio/hack-the-track/releases).
2. Extract and run `HackTheTrack.exe`.
3. There is a Node.js project called signaling server, which can be used for local development by using `node server.js`
4. Wait until all servers show “running on ws://localhost:xxxx”.
5. Use UI buttons and keyboard shortcuts on windows computers:
   - **C/V** – switch cameras  
   - **R** – cycle vehicles  
   - **X** – spawn/unspawn player car (WASD + SPACE controls)  
   - **TAB** – toggle statistics menu

---

## 🛠️ Installation (Development Build)
1. Clone the repository:
   ```bash
   git clone https://github.com/FireDragonGameStudio/hack-the-track.git
   ```
2. Install Python dependencies:
  ```bash
  pip install pandas websockets python-dateutil
  ```
3. Install Node.js dependencies for signaling server:
  ```bash
  npm install express ws
  ```
4. Build Unity project for Windows/Android.
5. Copy server executables and log files into the Unity build folder.
6. Run servers and launch the Unity app.

## 🎯 Hackathon Vision
Telemetry Rush started as a project for the Toyota Gazoo Racing HackTheTrack hackathon. It has since evolved into Telemetry Rush XR, extending the desktop app into an XR streaming and entertainment client. The vision is to let fans not only watch races, but interact with telemetry data, challenge professional lap times, and experience motorsport analytics in a whole new way.

## 📦 Used Assets
- XCharts for Unity - https://github.com/XCharts-Team/XCharts
- Toyota Car Model - https://sketchfab.com/3d-models/toyota-gt86-stock-c4732cfe6f65408eb387668b7a36f768
- Prometeo Car Controller - https://assetstore.unity.com/packages/tools/physics/prometeo-car-controller-209444
- Cartoon Race Track - https://assetstore.unity.com/packages/3d/environments/roadways/cartoon-race-track-oval-175061

## 📜 License
MIT License – free to use, modify, and share.
