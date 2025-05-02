import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, HubConnection, LogLevel } from "@microsoft/signalr";

const SIGNALR_SERVER_URL = "https://localhost:7016/PlayersHub"; // <-- Change to your backend URL

const MobeeMainPage: React.FC = () => {
    const [isConnected, setIsConnected] = useState(false);
    const [videoUrl, setVideoUrl] = useState<string | undefined>();
    const videoRef = useRef<HTMLVideoElement | null>(null);
    const connectionRef = useRef<HubConnection | null>(null);

    // --- SignalR Setup ---
    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl(SIGNALR_SERVER_URL)
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        connection.onclose(async (error) => {
            setIsConnected(false);
            // Optional: try reconnecting here, or rely on automatic reconnect
            console.error("SignalR connection closed", error);
        });

        connection.onreconnected(async (connectionId) => {
            setIsConnected(true);
            // In future: re-join group etc.
            console.log("SignalR reconnected", connectionId);
        });

        connection.onreconnecting((error) => {
            setIsConnected(false);
            console.warn("SignalR reconnecting", error);
        });

        // Start the connection
        connection
            .start()
            .then(() => {
                setIsConnected(true);
                console.log("SignalR connected");
                // In future: join group
            })
            .catch((err) => {
                setIsConnected(false);
                console.error("SignalR connection error: ", err);
            });

        connectionRef.current = connection;

        // Cleanup on unmount
        return () => {
            connection.stop();
        };
    }, []);

    // --- Handle File Open ---
    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            const url = URL.createObjectURL(file);
            setVideoUrl(url);
        }
    };

    return (
        <div style={{ padding: 20 }}>
            <h1>Mobee Web Client</h1>
            <div>
                <label>
                    Open your video file:
                    <input type="file" accept="video/*" onChange={handleFileChange} />
                </label>
            </div>
            <div style={{ marginTop: 16 }}>
                {videoUrl && (
                    <video
                        ref={videoRef}
                        src={videoUrl}
                        controls
                        style={{ width: "100%", maxWidth: 600 }}
                    />
                )}
            </div>
            <div style={{ marginTop: 16 }}>
                SignalR status:{" "}
                <span style={{ color: isConnected ? "green" : "red" }}>
                    {isConnected ? "Connected" : "Disconnected"}
                </span>
            </div>
        </div>
    );
};

export default MobeeMainPage;