import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';

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
        <div className="config-page">
            <h1>Welcome to Mobee</h1>
            <div>
                <label>
                    Username:
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        placeholder="Enter your username"
                    />
                </label>
            </div>
            <div>
                <label>
                    Group Name:
                    <input
                        type="text"
                        value={groupName}
                        onChange={(e) => setGroupName(e.target.value)}
                        placeholder="Enter group name"
                    />
                </label>
            </div>
            <div>
                <label>
                    Media File:
                    <input type="file" onChange={handleFileChange} />
                </label>
            </div>
            <button onClick={handleSubmit}>Let's Go!</button>
        </div>
    );
};

export default ConfigurationPage;