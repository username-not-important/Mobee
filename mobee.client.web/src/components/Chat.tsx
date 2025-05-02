import React, { useState } from 'react';

const Chat = ({ connection }) => {
    const [message, setMessage] = useState('');
    const [chat, setChat] = useState([]);

    const sendMessage = async () => {
        await connection.invoke('SendMessage', 'User', message);
        setMessage('');
    };

    connection.on('ChatMessage', (user, receivedMessage) => {
        setChat(prevChat => [...prevChat, { user, message: receivedMessage }]);
    });

    return (
        <div>
        <div>
        {
            chat.map((msg, index) => (
                <div key= { index } > <strong>{ msg.user }: </strong> {msg.message}</div >
        ))
        }
        < /div>
        < input value = { message } onChange = { e => setMessage(e.target.value)} />
            < button onClick = { sendMessage } > Send < /button>
                < /div>
  );
};

export default Chat;
