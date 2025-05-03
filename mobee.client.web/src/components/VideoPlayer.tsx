import React, {
    forwardRef,
    useImperativeHandle,
    useRef,
    useEffect,
    useCallback,
} from 'react';
import ReactPlayer from 'react-player';

export interface VideoPlayerHandle {
    setPosition: (position: number) => void;
    play: () => void;
    pause: () => void;
    getPlaying: () => boolean;
}

interface Props {
    source: string;
    playbackSyncLock: React.RefObject<boolean>;
    onSeek: (isPlaying: boolean, position: number) => void;
    onPlayPause: (isPlaying: boolean, position: number) => void;
}

const VideoPlayer = React.memo(forwardRef<VideoPlayerHandle, Props>(
    ({ source, playbackSyncLock, onSeek, onPlayPause }, ref) => {
        const playerRef = useRef<ReactPlayer>(null);
        const playingStatusRef = useRef(false);
        const [isPlaying, setIsPlaying] = React.useState(false);

        // Expose imperative methods to parent
        useImperativeHandle(ref, () => ({
            setPosition: (position: number) => {
                // position is in 100-nanosecond units, convert to seconds
                const seconds = position / 1000 / 10000;
                playerRef.current?.seekTo(seconds, 'seconds');
            },
            play: () => setIsPlaying(true),
            pause: () => setIsPlaying(false),
            getPlaying: () => playingStatusRef.current,
        }));

        // Handle play/pause and seeking events
        const handlePlay = useCallback(() => {
            if (playbackSyncLock?.current) return;

            playingStatusRef.current = true;

            if (playerRef.current) {
                const sec = playerRef.current.getCurrentTime();
                onPlayPause(true, Math.round(sec * 1000 * 10000));
            }
        }, [onPlayPause, playbackSyncLock]);

        const handlePause = useCallback(() => {
            if (playbackSyncLock?.current) return;

            playingStatusRef.current = false;

            if (playerRef.current) {
                const sec = playerRef.current.getCurrentTime();
                onPlayPause(false, Math.round(sec * 1000 * 10000));
            }
        }, [onPlayPause, playbackSyncLock]);

        const handleSeek = useCallback((seconds: number) => {
            if (playbackSyncLock?.current) return;

            onSeek(playingStatusRef.current, Math.round(seconds * 1000 * 10000));
        }, [onSeek, playbackSyncLock]);

        useEffect(() => {
            // Sync playing status with internal ref
            playingStatusRef.current = isPlaying;
        }, [isPlaying]);

        return (
            <div>
                <ReactPlayer
                    ref={playerRef}
                    url={source}
                    playing={isPlaying}
                    controls
                    width="100%"
                    height="auto"
                    onPlay={handlePlay}
                    onPause={handlePause}
                    onSeek={handleSeek}
                />
            </div>
        );
    }
));

export default VideoPlayer;