import { useEffect, useMemo, useState } from 'react';
import UserList from './UserList.jsx';
import './Dashboard.css';

export default function Dashboard({ onLogout, dashboardPage, setDashboardPage }) {
    const auth = useMemo(() => {
        try { return JSON.parse(localStorage.getItem('auth') || '{}'); }
        catch { return {}; }
    }, []);

    const [me, setMe] = useState(null);
    const [loading, setLoading] = useState(true);
    const [err, setErr] = useState('');

    const fmtDate = (d) => (d ? new Date(d).toLocaleDateString() : '-');

    useEffect(() => {
        const emp = auth?.employeeNo;
        if (!emp) return;
        (async () => {
            setLoading(true);
            setErr('');
            try {
                const res = await fetch(`/auth/me?employeeNo=${encodeURIComponent(emp)}`);
                const data = await res.json();
                if (!res.ok) throw new Error(data?.message || '取得に失敗しました');
                setMe(data);
            } catch (e) {
                setErr(e.message || 'サーバーに接続できません');
            } finally {
                setLoading(false);
            }
        })();
    }, [auth?.employeeNo]);

    const handleReturn = async () => {
        if (!me?.employeeNo) return;
        if (!window.confirm('返却処理を実行します。よろしいですか？')) return;

        try {
            const res = await fetch('/auth/return', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ employeeNo: me.employeeNo })
            });
            const data = await res.json();
            if (!res.ok) throw new Error(data?.message || '返却に失敗しました');

            const r = await fetch(`/auth/me?employeeNo=${encodeURIComponent(me.employeeNo)}`);
            const next = await r.json();
            if (r.ok) setMe(next);
            alert('返却が完了しました');
        } catch (e) {
            alert(e.message || '返却に失敗しました');
        }
    };

    /*
    const handleEdit = () => {
        alert('編集ボタンが押されました');
    };




     //<button className="edit-btn" onClick={handleEdit}><img src="/icons/edit.svg" alt="編集" /></button>
    */

    return (
        <div className="layout">
            <aside className="sidebar">
                <div className="hello">こんにちは</div>
                <div className="username">{me?.name || 'USER名'}</div>
                <nav className="menu">
                    <div className="menu-item">
                        <button className="menu-btn" onClick={() => setDashboardPage('dashboard')}>貸出状況</button>
                    </div>
                    <div className="menu-item">
                        <button className="menu-btn">機器一覧</button>
                    </div>
                    <div className="menu-item">
                        <button className="menu-btn" onClick={() => setDashboardPage('userList')}>ユーザー一覧</button>
                    </div>
                </nav>
                <button className="logout" onClick={onLogout}>LOGOUT</button>
            </aside>
            <main className="panel">
                {loading && <div>読み込み中...</div>}
                {err && <div className="error">{err}</div>}
                {!loading && !err && (
                    <>
                        {dashboardPage === 'dashboard' && (
                            <>
                                <h1 className="emp-name">{me?.name || '社員氏名'}</h1>
                                <div className="status-row">
                                    <span className="label">貸出状態：</span>
                                    <span className={`badge ${me?.rental?.status === '貸出中' ? 'bad' : 'good'}`}>
                                        {me?.rental?.status === '貸出中' ? '貸出中' : 'なし'}
                                    </span>
                                </div>
                                {me?.rental?.status === '貸出中' && (
                                    <>
                                        <div className="detail-row">貸出機器：<strong>{me.rental.assetNo || '-'}</strong></div>
                                        <div className="detail-row">貸 出 日：{fmtDate(me.rental.rentalDate)}</div>
                                        <div className="detail-row">締 切 日：{fmtDate(me.rental.dueDate)}</div>
                                        <button className="return-btn" onClick={handleReturn}>返却</button>
                                    </>
                                )}
                            </>
                        )}
                        {dashboardPage === 'userList' && <UserList />}
                    </>
                )}
            </main>
        </div>
    );
}