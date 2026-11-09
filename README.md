# Skye Music Companion

A lightweight, cross‑platform remote control app for the Skye Music host player.  
Built with .NET 10 MAUI, the Companion connects to the Skye Music server over TCP, displays real‑time Now Playing information, and provides full transport controls from anywhere on your local network.

## ✨ Features

### 🎧 Real‑Time Now Playing
- Track title, artist, and playback state  
- Current position and total duration  
- Automatic updates pushed from the server  
- Artwork display with safe fallback handling  

### 📡 Reliable TCP Connection
- Connects to the Skye Music host over LAN  
- Automatic HELLO handshake with device name  
- Reconnect support  
- Clean connection/disconnection events  
- UI connection indicator (green/red)  

### 🎮 Playback Controls
- Play / Pause  
- Stop  
- Next  
- Previous
- Low‑latency command sending via TCP

### 🔊 Audio Streaming Toggle
- Enable or disable audio streaming from the Skye Music host

### 📄 Playlist Viewer
- Search your playlist
- See the current song highlighted
- Tap to play

### 📜 Built‑In Log Viewer
- View raw network messages  
- Debug connection events  
- Helps diagnose server/client behavior  

### 🖥️ Cross‑Platform
- Android  
- Windows  
- Unified codebase using .NET MAUI

## 🔌 How It Works

### 1. Companion connects to the server
The app opens a TCP connection to the Skye Music host using the IP and port configured in Settings.

### 2. HELLO handshake
Immediately after connecting, the Companion sends: hello<device name>
The device name is retrieved using `DeviceInfo.Name`, giving a friendly, user‑set name on Android and Windows.

### 3. Server identifies the client
The Skye Music server assigns:
- DeviceName  
- ConnectedAt  
- LastMessageAt  
- Remote endpoint  

### 4. Real‑time updates
The server pushes messages such as: NOWPLAYING... PLAYLIST... STREAMINFO... SERVERCLOSING...
The Companion parses these and updates the UI accordingly.

### 5. User controls playback
Transport commands are sent via: toggle stop next previous NOWPLAYING
All commands are sent through a persistent StreamWriter with AutoFlush enabled.

## 🧱 Architecture Overview

### CompanionConnection Service
Handles:
- TCP client lifecycle  
- StreamWriter creation  
- Listening loop  
- Message routing  
- HELLO handshake  
- NOWPLAYING requests  
- Command sending  

### MainPage
Displays:
- Now Playing metadata  
- Artwork  
- Playback state  
- Transport controls  
- Connection status icon
- Audio Streaming Toggle

### Playlist
- Full browsing of your Skye Music playlist
- Searching Supports
- Highlighting of current song

### LogPage
Displays:
- Raw network messages  
- Connection events  
- Debug output  

### Helpers
- `DeviceInfoHelper` — retrieves cross‑platform device name

## 🛠️ Requirements

- .NET 10
- .NET MAUI workload  
- Skye Music host server running on the same network  
- TCP port open between devices

## 🚀 Roadmap

- Visual polish and animations

## 📄 License

This project is licensed under the GPLv3 License.  
You are free to use, modify, and distribute the software as long as the original license terms are included.

## 🤝 Contributing

Contributions are welcome.  
If you want to add a feature or fix a bug, please open an issue first so we can discuss the change.  
Pull requests should be focused, clear, and limited to a single purpose.

## 💬 About

Skye Music Companion is part of the Skye Music ecosystem — a personal, local‑network music controller designed for clarity, reliability, and emotionally safe everyday use.  
It provides a simple, responsive interface for controlling playback and viewing Now Playing information from the Skye Music host application.
