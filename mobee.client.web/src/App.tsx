import React from 'react';
import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import CssBaseline from '@mui/material/CssBaseline';
import { ThemeProvider, createTheme } from '@mui/material/styles';

import ConfigurationPage from './ConfigurationPage';
import MainPage from './MainPage';

import './app.css'

const theme = createTheme({
    palette: {
        mode: 'dark',
        primary: { main: '#efb61d' },
    },
});

const App: React.FC = () => {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Router>
                <Routes>
                    <Route path="/web/config" element={<ConfigurationPage />} />
                    <Route path="/web/main" element={<MainPage />} />
                    <Route path="*" element={<Navigate to="/web/config" />} />
                </Routes>
            </Router>
        </ThemeProvider>
    );
};

export default App;