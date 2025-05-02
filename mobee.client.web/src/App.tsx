import React, { useEffect, useRef, useState } from 'react';
import './App.css';
import { SignalRService } from './services/signalRService';
import * as signalR from '@microsoft/signalr';
import VideoPlayer, { VideoPlayerHandle } from './components/VideoPlayer';

const signalRService = new SignalRService();

function App() {
    const playerRef = useRef<VideoPlayerHandle>(null);
    const [group] = useState('default');
    const [username] = useState('ali');
    const [chatMessages, setChatMessages] = useState<string[]>([]);
    const [messageInput, setMessageInput] = useState('');

    useEffect(() => {
        const connect = async () => {
            try {
                await signalRService.start();

                await new Promise<void>((resolve) => {
                    const check = () => {
                        if (
                            signalRService.connection?.state ===
                            signalR.HubConnectionState.Connected
                        ) {
                            resolve();
                        } else {
                            setTimeout(check, 100);
                        }
                    };
                    check();
                });

                await signalRService.joinGroup(group, username);
            } catch (err) {
                console.error('Failed to connect:', err);
            }

            signalRService.onPlaybackToggled((user, isPlaying, position) => {
                const player = playerRef.current;
                if (!player) return;

                player.setPosition(position);
                isPlaying ? player.play() : player.pause();
                console.log(
                    `Playback toggled by ${user}: ${isPlaying ? 'play' : 'pause'} at ${position}`
                );
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

    const togglePlayback = async () => {
        const player = playerRef.current;
        if (!player) return;

        const isPlaying = playerRef.current?.plyr?.playing;
        const currentTime = playerRef.current?.plyr?.currentTime ?? 0;
        const position = Math.floor(currentTime * 1000 * 10000);

        if (isPlaying) {
            player.pause();
        } else {
            player.play();
        }

        await signalRService.togglePlayback(group, username, !isPlaying, position);
    };

    const handleSeek = async (position: number) => {
        await signalRService.togglePlayback(group, username, false, position);
    };

    const handlePlayPause = async (isPlaying: boolean, position: number) => {
        await signalRService.togglePlayback(group, username, isPlaying, position);
    };

    const sendMessage = async () => {
        if (messageInput.trim() === '') return;
        await signalRService.sendMessage(group, username, messageInput);
        setMessageInput('');
    };

    return (
        <div className="App">
            <h2>Mobee Web Client</h2>
            <VideoPlayer
                ref={playerRef}
                source="/sample.mp4"
                onSeek={handleSeek}
                onPlayPause={handlePlayPause}
            />
            <div style={{ margin: '1rem 0' }}>
                <button onClick={togglePlayback}>Toggle Play/Pause (Sync)</button>
            </div>

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
