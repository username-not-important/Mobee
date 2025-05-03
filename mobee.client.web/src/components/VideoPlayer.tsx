// src/components/VideoPlayer.tsx
import React, {
    forwardRef,
    useImperativeHandle,
    useRef,
    useEffect,
} from 'react';
import Plyr from 'plyr-react';
import 'plyr-react/plyr.css';

export interface VideoPlayerHandle {
    setPosition: (position: number) => void;
    play: () => void;
    pause: () => void;
    getPlaying: () => boolean;
}

interface Props {
    source: string;
    onSeek: (position: number) => void;
    onPlayPause: (isPlaying: boolean, position: number) => void;
}

const VideoPlayer = forwardRef<VideoPlayerHandle, Props>(
    ({ source, onSeek, onPlayPause }, ref) => {
        const playerRef = useRef<any>(null);

        useImperativeHandle(ref, () => ({
            setPosition: (position: number) => {
                const timeInSeconds = position / 1000 / 10000;
                if (playerRef.current?.plyr) {
                    const currentTime = playerRef.current.plyr.currentTime;
                    if (Math.abs(currentTime - timeInSeconds) > 1) {
                        playerRef.current.plyr.currentTime = timeInSeconds;
                    }
                }
            },
            play: () => {
                playerRef.current?.plyr?.play();
            },
            pause: () => {
                playerRef.current?.plyr?.pause();
            },
            getPlaying: () => { return !!playerRef.current?.plyr?.playing; },
        }));

        useEffect(() => {
            let interval: NodeJS.Timeout | null = null;

            function tryAttachListeners() {
                const plyr = playerRef.current?.plyr;
                if (plyr) {
                    const handlePlay = () =>
                        onPlayPause(true, plyr.currentTime * 1000 * 10000);
                    const handlePause = () =>
                        onPlayPause(false, plyr.currentTime * 1000 * 10000);
                    const handleSeeked = () =>
                        onSeek(plyr.currentTime * 1000 * 10000);

                    plyr.on('play', handlePlay);
                    plyr.on('pause', handlePause);
                    plyr.on('seeked', handleSeeked);

                    // Cleanup
                    return () => {
                        plyr.off('play', handlePlay);
                        plyr.off('pause', handlePause);
                        plyr.off('seeked', handleSeeked);
                    };
                }
                return null;
            }

            let cleanup: (() => void) | null = null;
            interval = setInterval(() => {
                if (!cleanup) {
                    const cleanupFn = tryAttachListeners();
                    if (cleanupFn) {
                        cleanup = cleanupFn;
                        if (interval) clearInterval(interval);
                    }
                }
            }, 100);

            return () => {
                if (interval) clearInterval(interval);
                if (cleanup) cleanup();
            };
        }, [onPlayPause, onSeek]);


        return (
            <Plyr
                ref={playerRef}
                source={{
                    type: 'video',
                    sources: [
                        {
                            src: source,
                            type: 'video/mp4',
                        },
                    ],
                }}
                options={{ controls: ['play', 'progress', 'current-time', 'mute', 'volume', 'fullscreen'] }}
            />
        );
    }
);

export default VideoPlayer;
