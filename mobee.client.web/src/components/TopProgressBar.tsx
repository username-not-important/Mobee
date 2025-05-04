import React from 'react';
import { HubConnectionState } from '@microsoft/signalr';
import { useTheme } from '@mui/material/styles'; // Import the theme hook

interface TopProgressBarProps {
    connectionState: HubConnectionState;
}

const TopProgressBar: React.FC<TopProgressBarProps> = ({ connectionState }) => {
    const theme = useTheme(); // Access the MUI theme

    if (connectionState === HubConnectionState.Connected) return null;

    return (
        <div
            style={{
                position: 'fixed',
                top: 0,
                left: 0,
                height: '4px',
                width: '100vw',
                background: `linear-gradient(90deg, ${theme.palette.primary.main} 0%, ${theme.palette.primary.light} 100%)`, // Use theme colors
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
                        'repeating-linear-gradient(90deg, rgba(0, 0, 0, 0.4), rgba(0, 0, 0, 0.4) 10px, transparent 10px, transparent 20px)',
                    animation: 'move 1.2s linear infinite',
                }}
            ></div>
        </div>
    );
};

export default TopProgressBar;