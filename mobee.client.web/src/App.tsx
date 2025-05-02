import React, { useEffect, useRef, useState } from 'react';
import './App.css';
import { SignalRService } from './services/signalRService';
import * as signalR from '@microsoft/signalr';

const signalRService = new SignalRService();

function App() {
    const videoRef = useRef < HTMLVideoElement | null > (null);
    const [group] = useState('default'); // Static group for now
    const [username] = useState('ali'); // You can change this dynamically
    const [isConnected, setIsConnected] = useState(false);
    const [chatMessages, setChatMessages] = useState < string[] > ([]);
    const [messageInput, setMessageInput] = useState('');

    useEffect(() => {
        const connect = async () => {
            try {
                await signalRService.start();

                // Wait until the connection is actually in the 'Connected' state
                const waitUntilConnected = () =>
                    new Promise<void>((resolve) => {
                        const check = () => {
                            if (signalRService.connection?.state === signalR.HubConnectionState.Connected) {
                                resolve();
                            } else {
                                setTimeout(check, 100); // check every 100ms
                            }
                        };
                        check();
                    });

                await waitUntilConnected(); // 🔒 ensure ready

                await signalRService.joinGroup(group, username); // ✅ safe to call now
            } catch (err) {
                console.error('Failed to connect:', err);
            }

            signalRService.onPlaybackToggled((user, isPlaying, position) => {
                const video = videoRef.current;
                if (!video) return;

                video.currentTime = position / 1000 / 10000;
                isPlaying ? video.play() : video.pause();
                console.log(`Playback toggled by ${user}: ${isPlaying ? 'play' : 'pause'} at ${position}`);
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
        const video = videoRef.current;
        if (!video) return;

        const isPlaying = !video.paused;
        const position = Math.floor(video.currentTime * 1000 * 10000);

        console.log(position);

        if (isPlaying) {
            video.pause();
        } else {
            video.play();
        }

        await signalRService.togglePlayback(group, username, !isPlaying, position);
    };

    const sendMessage = async () => {
        if (messageInput.trim() === '') return;
        await signalRService.sendMessage(group, username, messageInput);
        setMessageInput('');
    };

    return (
        <div className="App">
            <h2>Mobee Web Client</h2>
            <video
                ref={videoRef}
                width="640"
                height="360"
                controls
                src="/sample.mp4" // Put your video file in public folder or use file picker
            ></video>

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
