import React, {
    forwardRef,
    useImperativeHandle,
    useRef,
    useEffect,
    useCallback,
    useState,
} from 'react';
import ReactPlayer from 'react-player';
import { IconButton, Slider, Box } from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import PauseIcon from '@mui/icons-material/Pause';

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

const VideoPlayer = React.memo(
    forwardRef<VideoPlayerHandle, Props>(
        (
            {
                source,
                playbackSyncLock,
                onSeek,
                onPlayPause,
                // Pass chat props here
            },
            ref
        ) => {
            const playerRef = useRef<ReactPlayer>(null);
            const [isPlaying, setIsPlaying] = useState(false);
            const [duration, setDuration] = useState(0);
            const [played, setPlayed] = useState(0); // 0..1
            const [seeking, setSeeking] = useState(false);
            const playingStatusRef = useRef(false);

            // Imperative handle for parent
            useImperativeHandle(ref, () => ({
                setPosition: (position: number) => {
                    const seconds = position / 1000 / 10000;
                    playerRef.current?.seekTo(seconds, 'seconds');
                },
                play: () => setIsPlaying(true),
                pause: () => setIsPlaying(false),
                getPlaying: () => playingStatusRef.current,
            }));

            // Play/Pause events
            const handlePlay = useCallback(() => {
                if (playbackSyncLock?.current) return;
                playingStatusRef.current = true;
                setIsPlaying(true);
                if (playerRef.current) {
                    const sec = playerRef.current.getCurrentTime();
                    onPlayPause(true, Math.round(sec * 1000 * 10000));
                }
            }, [onPlayPause, playbackSyncLock]);

            const handlePause = useCallback(() => {
                if (playbackSyncLock?.current) return;
                playingStatusRef.current = false;
                setIsPlaying(false);
                if (playerRef.current) {
                    const sec = playerRef.current.getCurrentTime();
                    onPlayPause(false, Math.round(sec * 1000 * 10000));
                }
            }, [onPlayPause, playbackSyncLock]);

            // Seek events
            const handleSeekMouseDown = () => setSeeking(true);

            const handleSeekChange = (e: React.ChangeEvent<HTMLInputElement>) => {
                setPlayed(parseFloat(e.target.value));
            };

            const handleSeekMouseUp = (e: React.ChangeEvent<HTMLInputElement>) => {
                setSeeking(false);
                const newPlayed = parseFloat(e.target.value);
                const newTime = newPlayed * duration;
                playerRef.current?.seekTo(newTime, 'seconds');
                onSeek(isPlaying, Math.round(newTime * 1000 * 10000));
            };

            // Progress from player
            const handleProgress = (state: { played: number }) => {
                if (!seeking) setPlayed(state.played);
            };

            // Duration from player
            const handleDuration = (dur: number) => setDuration(dur);

            // Keyboard controls (optional): Space to play/pause, arrows to seek
            useEffect(() => {
                const onKeyDown = (e: KeyboardEvent) => {
                    if (e.code === 'Space') {
                        setIsPlaying(p => !p);
                    } else if (e.code === 'ArrowRight') {
                        playerRef.current?.seekTo(playerRef.current.getCurrentTime() + 5, 'seconds');
                    } else if (e.code === 'ArrowLeft') {
                        playerRef.current?.seekTo(playerRef.current.getCurrentTime() - 5, 'seconds');
                    }
                };
                window.addEventListener('keydown', onKeyDown);
                return () => window.removeEventListener('keydown', onKeyDown);
            }, []);

            // Format time helper
            const formatTime = (seconds: number) => {
                if (isNaN(seconds)) return '00:00';
                const m = Math.floor(seconds / 60);
                const s = Math.floor(seconds % 60);
                return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
            };

            return (
                <Box className="video-wrapper">
                    {/* Video fills the wrapper */}
                    <ReactPlayer
                        ref={playerRef}
                        url={source}
                        playing={isPlaying}
                        controls={false} // Hide default controls
                        width="100%"
                        height="100%"
                        onPlay={handlePlay}
                        onPause={handlePause}
                        onProgress={handleProgress}
                        onDuration={handleDuration}
                        style={{ position: 'absolute', top: 0, left: 0 }}
                    />

                    <Box display="flex" alignItems="center" width="100%" className="controls-bar">
                        <IconButton onClick={() => setIsPlaying(p => !p)} color="primary">
                            {isPlaying ? <PauseIcon /> : <PlayArrowIcon />}
                        </IconButton>
                        <span className="time">{formatTime(duration * played)}</span>
                        <input
                            type="range"
                            min={0}
                            max={1}
                            step="any"
                            value={played}
                            onMouseDown={handleSeekMouseDown}
                            onChange={handleSeekChange}
                            onMouseUp={handleSeekMouseUp}
                            className="seekbar"
                        />
                        <span className="time">{formatTime(duration)}</span>
                        {/* Add more controls: volume, captions, etc. */}
                    </Box>
                </Box>
            );
        }
    )
);

export default VideoPlayer;