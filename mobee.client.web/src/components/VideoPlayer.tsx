import React, { useEffect, useRef } from 'react';
import videojs from 'video.js';
import 'video.js/dist/video-js.css';

type Props = {
    onSeek: (position: number) => void;
    onPlayPause: (isPlaying: boolean, position: number) => void;
    source: string;
};

const VideoPlayer: React.FC<Props> = ({ onSeek, onPlayPause, source }) => {
    const videoRef = useRef<HTMLVideoElement | null>(null);
    const playerRef = useRef<videojs.Player>();

    useEffect(() => {
        if (videoRef.current) {
            playerRef.current = videojs(videoRef.current, {
                controls: true,
                preload: 'auto',
                width: 640,
                height: 360,
                sources: [{ src: source, type: 'video/mp4' }],
            });

            const player = playerRef.current;

            player.on('play', () => {
                onPlayPause(true, player.currentTime() * 1000 * 10000);
            });

            player.on('pause', () => {
                onPlayPause(false, player.currentTime() * 1000 * 10000);
            });

            player.on('seeked', () => {
                onSeek(player.currentTime() * 1000 * 10000);
            });
        }

        return () => {
            playerRef.current?.dispose();
        };
    }, [source]);

    return (
        <div data-vjs-player>
            <video ref={videoRef} className="video-js vjs-default-skin" />
        </div>
    );
};

export default VideoPlayer;
