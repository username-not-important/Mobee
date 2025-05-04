import React, { useState, useEffect, useRef } from 'react';
import { TextField, IconButton } from '@mui/material';
import SendIcon from '@mui/icons-material/Send';
import { SignalRService } from '../services/SignalRService';


interface ChatPanelProps {
    chatMessages: string[]; // Array of chat messages
    messageInput: string; // Current input value
    setMessageInput: (value: string) => void; // Function to update input value
    sendMessage: () => void; // Function to send a message

    hubService: SignalRService; // Pass the SignalR service instance
    groupName: string; // The group name for querying users
    username: string // Current User Name
}

const ChatPanel: React.FC<ChatPanelProps> = ({
    chatMessages,
    messageInput,
    setMessageInput,
    sendMessage,

    hubService,
    groupName,
    username
}) => {
    const [isVisible, setIsVisible] = useState(true); // Chat panel visibility state
    const [onlineUsers, setOnlineUsers] = useState<number>(0); // Online users state

    const activityTimeoutRef = useRef<number | null>(null); // Reference for the inactivity timeout

    // Query the number of online users periodically
    useEffect(() => {
        const queryUsers = async () => {
            const users = await hubService.queryGroupUsers(groupName, username);
            setOnlineUsers(users.length); // Update the state
        };

        // Start the timer (query every 5 seconds)
        const interval = setInterval(() => {
            queryUsers();
        }, 5000);

        // Cleanup on component unmount
        return () => clearInterval(interval);
    }, [hubService, groupName]);

    // Function to reset inactivity timer
    const resetInactivityTimer = () => {
        setIsVisible(true); // Show the panel on interaction
        if (activityTimeoutRef.current) clearTimeout(activityTimeoutRef.current); // Clear existing timeout
        activityTimeoutRef.current = window.setTimeout(() => {
            setIsVisible(false); // Hide the panel after 5 seconds of inactivity
        }, 5000);
    };

    // Add event listeners for user activity
    useEffect(() => {
        const handleUserActivity = () => {
            resetInactivityTimer();
        };

        // Listen to activity events
        window.addEventListener('mousemove', handleUserActivity);
        window.addEventListener('touchstart', handleUserActivity);

        // Cleanup event listeners on component unmount
        return () => {
            window.removeEventListener('mousemove', handleUserActivity);
            window.removeEventListener('touchstart', handleUserActivity);
            if (activityTimeoutRef.current) clearTimeout(activityTimeoutRef.current);
        };
    }, []);

    return (
        <div
            className="chat-panel"
            style={{
                opacity: isVisible ? 1 : 0, // Fade out when hidden
                transition: 'opacity 0.5s ease', // Smooth transition
                pointerEvents: isVisible ? 'auto' : 'none', // Disable interaction when hidden
            }}
        >
            <div className="chat-header">
                <span>Chat</span>
                <span className="chat-online">{onlineUsers} online</span>
            </div>
            <div className="chat-messages">
                {chatMessages.map((msg, idx) => (
                    <div key={idx}>{msg}</div>
                ))}
            </div>
            <div className="chat-controls">
                <TextField
                    id="outlined-multiline-flexible"
                    label="Type Message"
                    multiline
                    value={messageInput}
                    onChange={(e) => setMessageInput(e.target.value)}
                    maxRows={4}
                />
                <IconButton onClick={sendMessage} color="primary">
                    <SendIcon />
                </IconButton>
            </div>
        </div>
    );
};

export default ChatPanel;