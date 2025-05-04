import React, { useEffect, useRef, useState } from 'react';
import './App.css';
import { SignalRService } from './services/SignalRService';
import * as signalR from '@microsoft/signalr';

import TopProgressBar from './components/TopProgressBar';
import VideoPlayer, { VideoPlayerHandle } from './components/VideoPlayer';
import ChatPanel from './components/ChatPanel';

import { ThemeProvider, createTheme } from '@mui/material/styles';
import { IconButton, Box } from '@mui/material';
import TextField from '@mui/material/TextField';
import SendIcon from '@mui/icons-material/Send';

const signalRService = new SignalRService();
const theme = createTheme({
    palette: {
        mode: 'dark',
        primary: { main: '#efb61d' },
    },
});

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

    function ticksToHMS(ticks: number) {
        const totalSeconds = Math.floor(ticks / 10000 / 1000);
        const hours = String(Math.floor(totalSeconds / 3600)).padStart(2, '0');
        const minutes = String(Math.floor((totalSeconds % 3600) / 60)).padStart(2, '0');
        const seconds = String(totalSeconds % 60).padStart(2, '0');
        return `${hours}:${minutes}:${seconds}`;
    }

    useEffect(() => {
        let isMounted = true;
        const connect = async () => {
            try {
                await signalRService.start();

                signalRService.connection?.onreconnecting(() => {
                    if (isMounted) setConnectionState(signalR.HubConnectionState.Reconnecting);
                });
                signalRService.connection?.onreconnected(() => {
                    if (isMounted) setConnectionState(signalR.HubConnectionState.Connected);
                });
                signalRService.connection?.onclose(() => {
                    if (isMounted) setConnectionState(signalR.HubConnectionState.Disconnected);
                });

                await new Promise<void>((resolve) => {
                    const check = () => {
                        const state = signalRService.connection?.state;
                        if (isMounted) setConnectionState(state ?? signalR.HubConnectionState.Disconnected);
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
                if (isMounted) setConnectionState(signalR.HubConnectionState.Disconnected);
                console.error('Failed to connect:', err);
            }
        };

        connect();

        return () => { isMounted = false; };
    }, [group, username]);

    useEffect(() => {
        const playbackToggledHandler = (user: string, isPlaying: boolean, position: number) => {
            if (user === username) return;
            const player = playerRef.current;
            if (!player) return;

            playbackSyncLock.current = true;
            player.setPosition(position);
            isPlaying ? player.play() : player.pause();
            setTimeout(() => {
                playbackSyncLock.current = false;
            }, 200);

            setChatMessages((prev) => [...prev, `${isPlaying ? '▶️' : '⏸️'} ${ticksToHMS(position)}`]);
        };

        const receiveMessageHandler = (from: string, message: string) => {
            setChatMessages((prev) => [...prev, `${from}: ${message}`]);
        };

        const memberJoinedHandler = (user: string) => {
            setChatMessages((prev) => [...prev, `🟢 ${user} joined`]);
        };

        const memberLeftHandler = (user: string) => {
            setChatMessages((prev) => [...prev, `🔴 ${user} left`]);
        };

        signalRService.onPlaybackToggled(playbackToggledHandler);
        signalRService.onReceiveMessage(receiveMessageHandler);
        signalRService.onMemberJoined(memberJoinedHandler);
        signalRService.onMemberLeft(memberLeftHandler);

        return () => {
            signalRService.offPlaybackToggled(playbackToggledHandler);
            signalRService.offReceiveMessage(receiveMessageHandler);
            signalRService.offMemberJoined(memberJoinedHandler);
            signalRService.offMemberLeft(memberLeftHandler);
        };
    }, [username, ticksToHMS]);

    const handleSeek = async (isPlaying: boolean, position: number) => {
        await signalRService.togglePlayback(group, username, isPlaying, position);

        setChatMessages((prev) => [...prev, `${isPlaying ? '▶️' : '⏸️'} ${ticksToHMS(position)}`]);
    };

    const handlePlayPause = async (isPlaying: boolean, position: number) => {
        await signalRService.togglePlayback(group, username, isPlaying, position);

        setChatMessages((prev) => [...prev, `${isPlaying ? '▶️' : '⏸️'} ${ticksToHMS(position)}`]);
    };

    const sendMessage = async () => {
        if (messageInput.trim() === '') return;
        await signalRService.sendMessage(group, username, messageInput);

        setMessageInput('');
        setChatMessages((prev) => [...prev, `${username}: ${messageInput}`]);
    };

    return (
        <ThemeProvider theme={theme}>

            <div className="App">
                <TopProgressBar connectionState={connectionState} />

                <VideoPlayer
                    ref={playerRef}
                    source="/web/sample.mp4"
                    onSeek={handleSeek}
                    onPlayPause={handlePlayPause}
                    playbackSyncLock={playbackSyncLock}
                />

                <ChatPanel
                    chatMessages={chatMessages}
                    messageInput={messageInput}
                    setMessageInput={setMessageInput}
                    sendMessage={sendMessage}
                    hubService={signalRService}
                    groupName={group}
                    username={username}
                />

            </div>
        </ThemeProvider>
    );
}

export default App;