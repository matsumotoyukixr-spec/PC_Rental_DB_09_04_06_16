import React, { useState, useEffect } from 'react';
import Login from './Login/Login.jsx';
import Dashboard from './Dashboard.jsx';
import UserList from './UserList.jsx';
import './App.css';
import './Dashboard.css';

const App = () => {
    const [currentPage, setCurrentPage] = useState('login');
    const [dashboardPage, setDashboardPage] = useState('dashboard');

    useEffect(() => {
        const auth = localStorage.getItem('auth');
        if (auth) {
            setCurrentPage('dashboard');
        }
    }, []);

    const handleLoginSuccess = (employeeNo) => {
        localStorage.setItem('auth', JSON.stringify({ employeeNo, loginAt: Date.now() }));
        setCurrentPage('dashboard');
        setDashboardPage('dashboard');
    };

    const handleLogout = () => {
        localStorage.removeItem('auth');
        setCurrentPage('login');
    };

    const renderDashboardContent = () => {
        switch (dashboardPage) {
            case 'dashboard':
                // Dashboardコンポーネントをそのまま返す
                return <Dashboard />;
            case 'userList':
                return <UserList />;
            default:
                return <Dashboard />;
        }
    };

    return (
        <div className="app">
            {currentPage === 'login' && <Login onLoginSuccess={handleLoginSuccess} />}
            {currentPage === 'dashboard' && (
                <div className="layout">
                    <aside className="sidebar">
                        <div className="hello">こんにちは</div>
                        <div className="username">{JSON.parse(localStorage.getItem('auth'))?.employeeNo || 'USER名'}</div>
                        <nav className="menu">
                            <button className="menu-btn" onClick={() => setDashboardPage('dashboard')}>貸出状況</button>
                            <button className="menu-btn">機器一覧</button>
                            <button className="menu-btn" onClick={() => setDashboardPage('userList')}>ユーザー一覧</button>
                        </nav>
                        <button className="logout" onClick={handleLogout}>LOGOUT</button>
                    </aside>
                    {/* メインのコンテンツエリアにpanel-containerクラスを適用 */}
                    <main className="panel-container">
                        {renderDashboardContent()}
                    </main>
                </div>
            )}
        </div>
    );
};

export default App;
/*
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
   /*
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
//*

/*
import React, { useState } from 'react';
import Login from './Login/Login.jsx';
import Dashboard from './Dashboard.jsx';
import './App.css';

const App = () => {
const [currentPage, setCurrentPage] = useState('login');

const handleLoginSuccess = (employeeNo) => {
    localStorage.setItem('auth', JSON.stringify({ employeeNo, loginAt: Date.now() }));
    setCurrentPage('dashboard');
};

const handleLogout = () => {
    localStorage.removeItem('auth');
    setCurrentPage('login');
};

return (
    <div className={`app ${currentPage === 'login' ? 'center' : ''}`}>
        {currentPage === 'login' && <Login onLoginSuccess={handleLoginSuccess} />}
        {currentPage === 'dashboard' && <Dashboard onLogout={handleLogout} />}
    </div>
);
};

export default App;
*/

///*