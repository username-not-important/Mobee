import React, { useEffect, useRef, useState } from 'react';
import './App.css';
import { SignalRService } from './services/signalRService';
import * as signalR from '@microsoft/signalr';
import VideoPlayer, { VideoPlayerHandle } from './components/VideoPlayer';

const signalRService = new SignalRService();

function App() {
    const playerRef = useRef<VideoPlayerHandle>(null);
    const playbackSyncLock = useRef(false);
    const [group] = useState('default');
    const [username] = useState('ali');
    const [chatMessages, setChatMessages] = useState<string[]>([]);
    const [messageInput, setMessageInput] = useState('');
    const [connectionState, setConnectionState] = useState<signalR.HubConnectionState>(
        signalR.HubConnectionState.Disconnected
    );

    function ticksToHMS(ticks) {
        const totalSeconds = Math.floor(ticks / 10000 / 1000);
        const hours = String(Math.floor(totalSeconds / 3600)).padStart(2, '0');
        const minutes = String(Math.floor((totalSeconds % 3600) / 60)).padStart(2, '0');
        const seconds = String(totalSeconds % 60).padStart(2, '0');
        return `${hours}:${minutes}:${seconds}`;
    }

    useEffect(() => {
        const connect = async () => {
            try {
                await signalRService.start();

                signalRService.connection?.onreconnecting(() => {
                    setConnectionState(signalR.HubConnectionState.Reconnecting);
                });
                signalRService.connection?.onreconnected(() => {
                    setConnectionState(signalR.HubConnectionState.Connected);
                });
                signalRService.connection?.onclose(() => {
                    setConnectionState(signalR.HubConnectionState.Disconnected);
                });

                await new Promise<void>((resolve) => {
                    const check = () => {
                        const state = signalRService.connection?.state;
                        setConnectionState(state ?? signalR.HubConnectionState.Disconnected);
                        if (state === signalR.HubConnectionState.Connected) {
                            resolve();
                        } else {
                            setTimeout(check, 100);
                        }
                    };
                    check();
                });

                await signalRService.joinGroup(group, username);
            } catch (err) {
                setConnectionState(signalR.HubConnectionState.Disconnected);
                console.error('Failed to connect:', err);
            }

            signalRService.onPlaybackToggled((user, isPlaying, position) => {
                if (user === username) return;

                const player = playerRef.current;
                if (!player) return;

                playbackSyncLock.current = true;

                player.setPosition(position);
                isPlaying ? player.play() : player.pause();

                setTimeout(() => {
                    playbackSyncLock.current = false;
                }, 200);

                //setChatMessages((prev) => [...prev, `${isPlaying ? '▶️' : '⏸️'} ${ticksToHMS(position)}`]);
                //console.log(`Playback toggled by ${user}: ${isPlaying ? 'play' : 'pause'} at ${position}`);
            });

            signalRService.onReceiveMessage((from, message) => {
                setChatMessages((prev) => [...prev, `${from}: ${message}`]);
            });

            signalRService.onMemberJoined((user) => {
                setChatMessages((prev) => [...prev, `🟢 ${user} joined`]);
            });

            signalRService.onMemberLeft((user) => {
                setChatMessages((prev) => [...prev, `🔴 ${user} left`]);
            });
        };

        connect();
    }, [group, username]);

    const handleSeek = async (isPlaying: boolean, position: number) => {
        await signalRService.togglePlayback(group, username, isPlaying, position);
    };

    const handlePlayPause = async (isPlaying: boolean, position: number) => {
        await signalRService.togglePlayback(group, username, isPlaying, position);
    };

    const sendMessage = async () => {
        if (messageInput.trim() === '') return;
        await signalRService.sendMessage(group, username, messageInput);
        chatMessages.push(messageInput);
        setMessageInput('');
    };

    return (
        <div className="App">
            {connectionState !== signalR.HubConnectionState.Connected && (
                <div
                    style={{
                        position: 'fixed',
                        top: 0,
                        left: 0,
                        height: '4px',
                        width: '100vw',
                        background: 'linear-gradient(90deg, #2196f3 0%, #21cbf3 100%)',
                        zIndex: 1000,
                        transition: 'opacity 0.3s',
                    }}
                >
                    <div
                        className="progress-indeterminate"
                        style={{
                            height: '100%',
                            width: '100%',
                            background:
                                'repeating-linear-gradient(90deg, #fff6, #fff6 10px, transparent 10px, transparent 20px)',
                            animation: 'move 1.2s linear infinite',
                        }}
                    ></div>
                </div>
            )}

            <h2>Mobee Web Client</h2>
            <VideoPlayer
                ref={playerRef}
                source="/sample.mp4"
                onSeek={handleSeek}
                onPlayPause={handlePlayPause}
                playbackSyncLock={playbackSyncLock}
            />

            <div className="chat">
                <h4>Chat</h4>
                <div className="chat-box">
                    {chatMessages.map((msg, idx) => (
                        <div key={idx}>{msg}</div>
                    ))}
                </div>
                <input
                    type="text"
                    value={messageInput}
                    onChange={(e) => setMessageInput(e.target.value)}
                    placeholder="Type message..."
                />
                <button onClick={sendMessage}>Send</button>
            </div>
        </div>
    );
}

export default App;
