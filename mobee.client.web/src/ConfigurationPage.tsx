import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Box, Typography, TextField, Button, Card, CardContent, IconButton } from '@mui/material';
import PersonIcon from '@mui/icons-material/Person';
import VideoLibraryIcon from '@mui/icons-material/VideoLibrary';
import FileUploadIcon from '@mui/icons-material/FileUpload';

const ConfigurationPage: React.FC = () => {
    const [username, setUsername] = useState('');
    const [groupName, setGroupName] = useState('default');
    const [videoFile, setVideoFile] = useState<File | null>(null);

    const navigate = useNavigate();

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        if (event.target.files && event.target.files[0]) {
            setVideoFile(event.target.files[0]);
        }
    };

    const handleSubmit = () => {
        if (!username || !groupName || !videoFile) {
            alert('Please fill in all fields.');
            return;
        }

        // Generate a temporary URL for the selected video file
        const videoURL = URL.createObjectURL(videoFile);

        // Save configurations to localStorage
        localStorage.setItem('username', username);
        localStorage.setItem('groupName', groupName);
        localStorage.setItem('videoURL', videoURL); // Save the generated URL instead of the file name

        // Navigate to the main page
        navigate('/web/main');
    };

    return (
        <Box className="config-page" sx={{ backgroundColor: '#2b2b2b', height: '100vh', padding: '20px', color: '#fff' }}>
            {/* Header */}
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <Typography variant="h4" sx={{ fontWeight: 'bold' }}>
                    Welcome to Mobee
                </Typography>
                <Box sx={{ width: '100px', height: '100px', backgroundColor: '#444', borderRadius: '8px', textAlign: 'center', lineHeight: '100px', color: '#fff' }}>
                    <img src="./Icon.png" style={{ width: '100%', }} />
                </Box>
            </Box>

            {/* Subtitle */}
            <Typography variant="subtitle1" sx={{ marginBottom: '20px', fontStyle: 'italic' }}>
                Synced Video and Movie Player
            </Typography>

            {/* User Configuration */}
            <Card sx={{ backgroundColor: '#3c3c3c', marginBottom: '20px' }}>
                <CardContent sx={{ display: 'flex', alignItems: 'center' }}>
                    <PersonIcon sx={{ fontSize: '40px', marginRight: '20px' }} />
                    <Box sx={{ flex: 1 }}>
                        <Typography variant="h6" sx={{ marginBottom: '10px' }}>
                            User
                        </Typography>
                        <TextField
                            fullWidth
                            label="Username"
                            variant="outlined"
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            sx={{ marginBottom: '10px',  borderRadius: '4px' }}
                        />
                        <TextField
                            fullWidth
                            label="Group Name"
                            variant="outlined"
                            value={groupName}
                            onChange={(e) => setGroupName(e.target.value)}
                            sx={{ borderRadius: '4px' }}
                        />
                    </Box>
                </CardContent>
            </Card>

            {/* Media File Configuration */}
            <Card sx={{ backgroundColor: '#3c3c3c', marginBottom: '20px' }}>
                <CardContent sx={{ display: 'flex', alignItems: 'center' }}>
                    <VideoLibraryIcon sx={{ fontSize: '40px', marginRight: '20px' }} />
                    <Box sx={{ flex: 1 }}>
                        <Typography variant="h6" sx={{ marginBottom: '10px' }}>
                            Media File
                        </Typography>
                        <Button
                            variant="contained"
                            component="label"
                            color="primary"
                            sx={{ width: '150px', marginBottom: '10px' }}
                        >
                            Browse
                            <input type="file" hidden onChange={handleFileChange} />
                        </Button>
                        {videoFile && (
                            <Typography variant="body2" sx={{ marginTop: '10px', color: '#aaa' }}>
                                {videoFile.name}
                            </Typography>
                        )}
                    </Box>
                </CardContent>
            </Card>

            {/* Submit Button */}
            <Button
                variant="contained"
                color="warning"
                fullWidth
                onClick={handleSubmit}
                sx={{
                    fontSize: '18px',
                    fontWeight: 'bold',
                    padding: '10px',
                    backgroundColor: '#f9b233',
                    '&:hover': { backgroundColor: '#f9a623' },
                }}
            >
                Let's Go!
            </Button>
        </Box>
    );
};

export default ConfigurationPage;