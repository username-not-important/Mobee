# Mobee - Open-Source Synchronized Video Player

<div align="center">
 
![MobeeLogoDark](https://user-images.githubusercontent.com/30866718/210573912-b4bcf837-c029-4f71-a311-7fcab9b8d33e.png)

**Watch Together, Anywhere**

A cross-platform synchronized video player for watching videos together in real-time with friends and family. Since 2023.

[Windows Client](#windows-client-wpf) • [Web Client](#web-client-react) • [Server](#server-aspnet)

</div>

---

## 📖 Overview

Mobee is an open-source synchronized platform that enables multiple users to watch videos together in perfect sync and play interactive multiplayer activities (such as Wordle), regardless of their location. Built on a client-server architecture using SignalR for real-time communication, Mobee supports both Windows desktop (WPF) and web browsers (React).

### Key Features (Some of these features are only available on the deployed website only)

- **🎬 Synchronized Playback** - Play, pause, and seek in perfect sync across all participants
- **🎮 Interactive Activities** - Play real-time multiplayer activities (such as Wordle) directly within room sessions
- **💬 Real-time Chat** - Text and voice messaging during playback
- **👥 Room Management** - Create private rooms with passcodes or join existing ones
- **🔒 Room Permissions** - Control who can change playback or send messages in a room
- **🌐 Cross-Platform** - Desktop (Windows WPF) and Web (React) clients
- **🔒 Secure Authentication** - JWT-based authorization with client registration
- **📱 Responsive UI** - Material Design for desktop, modern React UI for web
- **🌍 Internationalization** - Multi-language support (Persian/English)
- **🎨 Customizable** - Font selection, themes, and UI preferences
- **🔄 Auto-reconnect** - Resilient connection handling with automatic reconnection
- **📊 Media Explorer** - Search and browse media content

---

## 🏗️ Architecture

Mobee consists of three main components:

```
┌─────────────────────┐
│  Windows Client     │
│  (WPF .NET 7)       │◄────┐
└─────────────────────┘     │
                            │
┌─────────────────────┐     │    SignalR + REST API
│  Web Client         │     ├───────────────────────►  ┌─────────────────────┐
│  (React + Vite)     │◄────┘                          │  ASP.NET Server     │
└─────────────────────┘                                │  (SignalR Hub)      │
                                                       └─────────────────────┘
```

### Technology Stack

#### Windows Client (WPF)
- **.NET 7.0** - Core framework
- **FlyleafLib** - Media playback engine
- **SignalR Client** - Real-time communication
- **Material Design** - UI framework
- **Flurl.Http** - HTTP client
- **Polly** - Resilience and retry policies
- **NAudio** - Audio processing for voice messages
- **CommunityToolkit.Mvvm** - MVVM architecture support

#### Web Client
- **React 18** - UI framework
- **Vite** - Build tool
- **Axios** - HTTP client
- **SignalR** - Real-time communication
- **React Router** - Navigation

#### Server (ASP.NET)
- **ASP.NET Core 7.0** - Web framework
- **SignalR** - WebSocket hub for real-time communication
- **Entity Framework Core** - Data access
- **JWT Authentication** - Secure authorization
- **NLog** - Logging

---

## 🚀 Getting Started

### Prerequisites

- **Windows Client**: Windows 10/11, .NET 7.0 SDK
- **Web Client**: Node.js 16+, npm/yarn
- **Server**: .NET 7.0 SDK, SQL Server (optional for data persistence)

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/username-not-important/Mobee.git
cd Mobee
```

#### 2. Start the Server

```bash
cd Mobee.Server.Aspnet
dotnet restore
dotnet run
```

The server will start on `https://localhost:5001` (or configured port).

#### 3. Run Windows Client

```bash
cd Mobee.Client.WPF
dotnet restore
dotnet run
```

#### 4. Run Web Client

```bash
cd Mobee.Client.Web
npm install
npm run dev
```

The web client will be available at `http://localhost:5173`.

---

## 🎮 Usage

### Creating a Room

1. Launch the client (Windows or Web)
2. Register or log in with your alias
3. Click **"Create Room"**
4. Select activity (Video Player or Wordle)
5. Set an optional passcode and media source (for video rooms)
6. Share the room code with friends

### Joining a Room

1. Launch the client
2. Enter the **room code** provided by the host
3. Enter the **passcode** (if required)
4. Start watching together or playing interactive activities! *(Non-video rooms bypass media selection automatically)*

### Features During Playback & Activities

- **Play/Pause Sync** - All participants see the same playback state
- **Seek Sync** - Jumping to a timestamp syncs across all viewers
- **Interactive Games** - Real-time multiplayer activities like Wordle with shared game boards and guesses
- **Chat** - Send text messages visible to all room members
- **Voice Messages** - Record and send audio messages
- **Reactions** - Express emotions with emoji reactions
- **Member List** - See who's currently in the room

---

## 📂 Project Structure

```
Mobee/
├── Mobee.Client.WPF/          # Windows Desktop Client
│   ├── Network/                         # SignalR & HTTP communication
│   │   ├── ClientHelper.cs              # API endpoints
│   │   └── Gate.cs                      # HTTP resilience & error handling
│   ├── ViewModels/                      # MVVM view models
│   ├── Controls/                        # Custom WPF controls
│   ├── Views/                           # XAML views
│   ├── Stores/                          # Application state management
│   ├── Connection/                      # SignalR connection & reconnection
│   ├── Localization/                    # Multi-language resources
│   └── Assets/                          # Images, fonts, sounds
│
├── Mobee.Client.Web/          # Web Client (React)
│   ├── src/
│   │   ├── components/                  # React components
│   │   ├── pages/                       # Page components
│   │   ├── services/                    # API & SignalR services
│   │   ├── models/                      # TypeScript models
│   │   └── utils/                       # Utility functions
│   └── package.json
│
├── Mobee.Server.Aspnet/       # ASP.NET Server
│   ├── Hubs/                            # SignalR hubs
│   │   └── PlayersHub.cs                # Main player synchronization hub
│   ├── Controllers/                     # REST API controllers
│   ├── Services/                        # Business logic
│   │   └── RoomsManager.cs              # Room management service
│   ├── Models/                          # Domain models
│   ├── Data/                            # Database context
│   └── Middlewares/                     # Authentication & error handling
│
└── Mobee.Common/              # Shared Models & Interfaces
    ├── IPlayerHub.cs                    # Hub interface (server → client)
    ├── IPlayerClient.cs                 # Client interface (client → server)
    ├── Models/                          # Shared data models
    └── Hubs/                            # Hub metadata
```

---

## 🔌 API Reference

### SignalR Hub Methods

#### Server → Client (IPlayerClient)

```csharp
Task PlaybackToggled(string userCode, string userName, bool isPlaying, long position)
Task ActivityStateUpdated(string activityStateJson)
Task ReceiveMessage(string fromCode, string fromName, string message)
Task ReceiveTyping(string fromCode, string fromName)
Task MemberJoined(string userCode, string userName)
Task MemberLeft(string userCode, string userName)
Task ReceiveVoice(string userCode, string userName, byte[] audioData)
```

#### Client → Server (IPlayerHub)

```csharp
Task<ResultModel<JoinedRoomViewModel>> JoinRoom(string roomCode, string passCode, string fileName)
Task SendActivityAction(string roomCode, string actionJson)
Task<List<string>> QueryRoomParticipants(string group)
Task<PlaybackStatusViewModel> QueryPlaybackStatus(string group)
Task TogglePlayback(string roomCode, bool isPlaying, long position)
Task SendMessage(string roomCode, string message)
Task SendTyping(string roomCode)
Task SendVoice(string roomCode, byte[] audioData)
```

### REST API Endpoints

- `POST /api/Clients/RegisterClient` - Register a new client
- `POST /api/Clients/ChangeAlias` - Change user alias
- `POST /api/Rooms/CreateRoom` - Create a new room
- `POST /api/Rooms/AskToJoinRoom` - Request to join a room
- `POST /api/Rooms/CloseRoom` - Close an existing room
- `GET /api/Rooms/MyRooms` - Get user's rooms

---

## 📝 License

This project is open-source. Please check the repository for license information.

---

## 🙏 Acknowledgments

- **FlyleafLib** - Powerful media playback library
- **Material Design** - Beautiful UI components
- **SignalR** - Real-time communication framework

---

## 📞 Support

For issues, questions, or suggestions:

- Open an issue on GitHub
- Visit [Mobees.ir](http://mobees.ir)

---

<div align="center">

**Made with ❤️ by Alireza Joonbakhsh**
⭐ Star this repo if you find it useful!

</div>
 
## Thanks To:
 - [Flyleaf](https://github.com/SuRGeoNix/Flyleaf)
 - [Material Design in Xaml Toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit/)
 - [Mafe.Localization](https://github.com/fmazzant/Localization)
 
