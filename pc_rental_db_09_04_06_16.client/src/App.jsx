import React, { useState, useEffect } from 'react';
import Login from './Login/Login.jsx';
import Dashboard from './Dashboard.jsx';
import UserList from './UserList.jsx';
import './App.css';
import './Dashboard.css';

const App = () => {
    const [currentPage, setCurrentPage] = useState('login');
    const [dashboardPage, setDashboardPage] = useState('dashboard'); // 新しい状態を追加

    // ページロード時にlocalStorageを確認し、ログイン状態を維持
    useEffect(() => {
        const auth = localStorage.getItem('auth');
        if (auth) {
            setCurrentPage('dashboard');
        }
    }, []);

    const handleLoginSuccess = (employeeNo) => {
        localStorage.setItem('auth', JSON.stringify({ employeeNo, loginAt: Date.now() }));
        setCurrentPage('dashboard');
        // 再度me情報を取得して画面を更新
        window.location.reload();
    };

    /*
    const handleLoginSuccess = (employeeNo) => {
        localStorage.setItem('auth', JSON.stringify({ employeeNo, loginAt: Date.now() }));
        setCurrentPage('dashboard');
    };
    */

    const handleLogout = () => {
        localStorage.removeItem('auth');
        setCurrentPage('login');
    };
    

    return (
        <div className={`app ${currentPage === 'login' ? 'center' : ''}`}>
            {currentPage === 'login' && <Login onLoginSuccess={handleLoginSuccess} />}
            {currentPage === 'dashboard' && <Dashboard onLogout={handleLogout} dashboardPage={dashboardPage} setDashboardPage={setDashboardPage} />}
        </div>
    );
};

export default App;