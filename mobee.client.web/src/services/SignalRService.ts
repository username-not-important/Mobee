// src/services/signalRService.ts
import * as signalR from '@microsoft/signalr';

const SERVER_URL = import.meta.env.VITE_SERVER_URL;

export class SignalRService {
    public connection: signalR.HubConnection;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(SERVER_URL)
            .withAutomaticReconnect()
            .build();
    }

    // Wait for connection to be established, up to timeoutMs
    private async ensureConnected(timeoutMs = 5000) {
        if (this.connection.state === signalR.HubConnectionState.Connected) return;
        if (this.connection.state === signalR.HubConnectionState.Connecting) {
            // Wait for connecting to finish, or timeout
            await this.waitForConnection(timeoutMs);
        } else if (this.connection.state === signalR.HubConnectionState.Disconnected) {
            await this.connection.start();
        } else if (this.connection.state === signalR.HubConnectionState.Reconnecting) {
            await this.waitForConnection(timeoutMs);
        }

        //if (this.connection.state !== signalR.HubConnectionState.Connected) {
        //    throw new Error('SignalR is not connected.');
        //}
    }

    // Helper to wait for connection state
    private waitForConnection(timeoutMs = 5000): Promise<void> {
        return new Promise((resolve, reject) => {
            const start = Date.now();
            const check = () => {
                if (this.connection.state === signalR.HubConnectionState.Connected) {
                    resolve();
                } else if (Date.now() - start > timeoutMs) {
                    reject(new Error('SignalR connection timeout.'));
                } else {
                    setTimeout(check, 100);
                }
            };
            check();
        });
    }

    async start() {
        try {
            await this.ensureConnected();
            console.log('SignalR connected.');
        } catch (err) {
            console.error('SignalR failed to connect:', err);
            throw err;
        }
    }

    async joinGroup(group: string, user: string) {
        await this.ensureConnected();
        return this.connection.invoke('JoinGroup', group, user);
    }

    queryGroupUsers = async (group: string, user: string) => {
        await this.ensureConnected();
        return this.connection.invoke<string[]>('QueryGroupUsers', group, user);
    };

    togglePlayback = async (group: string, user: string, isPlaying: boolean, position: number) => {
        await this.ensureConnected();
        return this.connection.invoke('TogglePlayback', group, user, isPlaying, position);
    };

    sendMessage = async (group: string, user: string, message: string) => {
        await this.ensureConnected();
        return this.connection.invoke('SendMessage', group, user, message);
    };

    onPlaybackToggled = (callback: (user: string, isPlaying: boolean, position: number) => void) => {
        this.connection.on('PlaybackToggled', callback);
    };

    onReceiveMessage = (callback: (from: string, message: string) => void) => {
        this.connection.on('ReceiveMessage', callback);
    };

    onMemberJoined = (callback: (user: string) => void) => {
        this.connection.on('MemberJoined', callback);
    };

    onMemberLeft = (callback: (user: string) => void) => {
        this.connection.on('MemberLeft', callback);
    };

    offPlaybackToggled = (callback: (user: string, isPlaying: boolean, position: number) => void) => {
        this.connection.off('PlaybackToggled', callback);
    };

    offReceiveMessage = (callback: (from: string, message: string) => void) => {
        this.connection.off('ReceiveMessage', callback);
    };

    offMemberJoined = (callback: (user: string) => void) => {
        this.connection.off('MemberJoined', callback);
    };

    offMemberLeft = (callback: (user: string) => void) => {
        this.connection.off('MemberLeft', callback);
    };

}