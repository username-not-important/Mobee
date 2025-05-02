// src/services/signalRService.ts
import * as signalR from '@microsoft/signalr';

export class SignalRService {
    public connection: signalR.HubConnection;

    constructor() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7016/playersHub') // ⬅️ match casing
            .withAutomaticReconnect()
            .build();
    }

    async start() {
        if (this.connection.state === signalR.HubConnectionState.Disconnected) {
            await this.connection.start();
            console.log('SignalR connected.');
        }
    }

    async joinGroup(group: string, user: string) {
        return this.connection.invoke('JoinGroup', group, user);
    }

    queryGroupUsers = (group: string, user: string) => {
        return this.connection.invoke<string[]>('QueryGroupUsers', group, user);
    };

    togglePlayback = (group: string, user: string, isPlaying: boolean, position: number) => {
        return this.connection.invoke('TogglePlayback', group, user, isPlaying, position);
    };

    sendMessage = (group: string, user: string, message: string) => {
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
}
