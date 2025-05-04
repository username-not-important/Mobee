import React, {
    forwardRef,
    useImperativeHandle,
    useRef,
    useEffect,
    useCallback,
    useState,
} from 'react';
import ReactPlayer from 'react-player';
import { Button, IconButton, Slider, Box } from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import VolumeUpIcon from '@mui/icons-material/VolumeUp';
import VolumeOffIcon from '@mui/icons-material/VolumeOff';
import PauseIcon from '@mui/icons-material/Pause';
import FileOpenIcon from '@mui/icons-material/FileOpen';


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
            const [volume, setVolume] = useState(0.8); // Default volume is 80%
            const [lastVolume, setLastVolume] = useState(0.8); // Track last volume for muting/unmuting
            const [videoSource, setVideoSource] = useState<string | null>(null); // Video source state
            const [uploadedVideoURL, setUploadedVideoURL] = useState<string | null>(null); // Temporary URL for uploaded video

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

            const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
                const file = event.target.files?.[0]; // Get the selected file
                if (file) {
                    // Generate a temporary URL for the selected file
                    const newVideoURL = URL.createObjectURL(file);
                    setUploadedVideoURL(newVideoURL); // Save the temporary URL
                    setVideoSource(newVideoURL); // Update the video player's source
                }
            };

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

            const handleProgress = (state: { played: number }) => {
                if (!seeking) setPlayed(state.played);
            };

            const handleDuration = (dur: number) => setDuration(dur);

            // Keyboard controls: Space to play/pause, arrows to seek
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

            useEffect(() => {
                return () => {
                    if (uploadedVideoURL) {
                        URL.revokeObjectURL(uploadedVideoURL); // Revoke the object URL to free up memory
                    }
                };
            }, [uploadedVideoURL]);

            const formatTime = (seconds: number) => {
                if (!seconds || isNaN(seconds)) return '00:00';
                const m = Math.floor(seconds / 60);
                const s = Math.floor(seconds % 60);
                return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
            };

            return (
                <Box className="video-wrapper">
                    <ReactPlayer
                        ref={playerRef}
                        url={source}
                        playing={isPlaying}
                        volume={volume}
                        muted={volume === 0}
                        controls={false}
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
                        <Slider
                            value={played * 100 || 0}
                            min={0}
                            max={100}
                            step={0.1}
                            onChange={(e, newValue) => {
                                const newPlayed = (newValue as number) / 100;
                                setPlayed(newPlayed);
                            }}
                            onChangeCommitted={(e, newValue) => {
                                const newPlayed = (newValue as number) / 100;
                                const newTime = newPlayed * duration;
                                playerRef.current?.seekTo(newTime, 'seconds');
                                onSeek(isPlaying, Math.round(newTime * 1000 * 10000));
                            }}
                            sx={{
                                mx: 2, flex: 1,
                                color: 'primary.main',
                                height: 8,
                                '& .MuiSlider-thumb': {
                                    width: 16,
                                    height: 16,
                                },
                                '& .MuiSlider-rail': {
                                    opacity: 0.28,
                                },
                                '& .MuiSlider-track': {
                                    border: 'none',
                                },
                            }}
                        />
                        <span className="time">{formatTime(duration)}</span>

                        <Box display="flex" alignItems="center" className="volume-controls" sx={{ ml: 2 }}>
                            <IconButton
                                onClick={() => {
                                    if (volume > 0) {
                                        setLastVolume(volume); // Save current volume before muting
                                        setVolume(0); // Mute
                                    } else {
                                        setVolume(lastVolume || 0.8); // Restore last volume or default to 80%
                                    }
                                }}
                                color="primary"
                            >
                                {volume > 0 ? <VolumeUpIcon /> : <VolumeOffIcon />}
                            </IconButton>
                            <Slider
                                value={volume * 100 || 0}
                                min={0}
                                max={100}
                                step={1}
                                onChange={(e, newValue) => {
                                    const newVolume = (newValue as number) / 100;
                                    setVolume(newVolume);
                                }}
                                sx={{
                                    width: 100,
                                    color: 'primary.main',
                                    '& .MuiSlider-thumb': {
                                        width: 12,
                                        height: 12,
                                    },
                                    '& .MuiSlider-rail': {
                                        opacity: 0.28,
                                    },
                                    '& .MuiSlider-track': {
                                        border: 'none',
                                    },
                                }}
                            />
                        </Box>

                        <Box display="flex" justifyContent="center" mx={1}>
                            <Button variant="contained" component="label" color="primary" className="buttonFile">
                                <FileOpenIcon/>
                                <input
                                    type="file"
                                    accept="video/*" // Restrict file types to video
                                    hidden
                                    onChange={handleFileSelect}
                                />
                            </Button>
                        </Box>
                    </Box>
                </Box>
            );
        }
    )
);

export default VideoPlayer;