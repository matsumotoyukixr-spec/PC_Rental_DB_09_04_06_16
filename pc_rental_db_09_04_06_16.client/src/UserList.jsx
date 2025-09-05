import React, { useEffect, useState } from 'react';
import './Dashboard.css';

export default function UserList() {
    const [users, setUsers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [expandedRows, setExpandedRows] = useState([]);

    const fetchUsers = async () => {
        setLoading(true);
        try {
            const res = await fetch('/user');
            if (!res.ok) {
                throw new Error('ユーザー情報の取得に失敗しました');
            }
            const data = await res.json();
            setUsers(data);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchUsers();
    }, []);

    const handleAddUser = () => {
        alert('ユーザー追加ボタンが押されました');
    };

    const handleRemoveUser = () => {
        alert('ユーザー論理削除ボタンが押されました');
    };

    const handleEdit = (employeeNo) => {
        alert(`ユーザー ${employeeNo} の編集ボタンが押されました`);
        // ポップアップ画面への遷移ロジックをここに実装
    };

    const handleToggleDetails = (employeeNo) => {
        if (expandedRows.includes(employeeNo)) {
            setExpandedRows(expandedRows.filter(id => id !== employeeNo));
        } else {
            setExpandedRows([...expandedRows, employeeNo]);
        }
    };

    return (
        <>
            <h1 className="panel-title">ユーザー一覧</h1>
            <div className="table-controls">
                <div className="button-group">
                    <button className="icon-button" onClick={handleAddUser}>+</button>
                    <button className="icon-button" onClick={handleRemoveUser}>-</button>
                    <button className="icon-button" onClick={fetchUsers}>↻</button>
                </div>
                <div className="search-group">
                    <input type="text" placeholder="検索" className="search-input" />
                    <button className="icon-button">
                        <img src="/icons/search.svg" alt="検索" />
                    </button>
                </div>
            </div>
            {loading ? (
                <div className="loading-panel">読み込み中...</div>
            ) : error ? (
                <div className="error-panel">{error}</div>
            ) : (
                <div className="table-wrapper">
                    <table className="data-table user-table">
                        <thead>
                            <tr>
                                <th className="action-column"></th>
                                <th className="action-column"></th>
                                <th>社員番号</th>
                                <th>氏名</th>
                                <th>氏名（フリガナ）</th>
                                <th>電話番号</th>
                                <th>メールアドレス</th>
                                <th>役職</th>
                                <th>PCアカウント権限</th>
                                <th>更新日</th>
                                {expandedRows.length > 0 && (
                                    <>
                                        <th className="detail-header">所属部署</th>
                                        <th className="detail-header">年齢</th>
                                        <th className="detail-header">性別</th>
                                        <th className="detail-header">退職日</th>
                                        <th className="detail-header">登録日</th>
                                        <th className="detail-header">論理削除フラグ</th>
                                    </>
                                )}
                            </tr>
                        </thead>
                        <tbody>
                            {users.map((user) => (
                                <tr key={user.employeeNo} className={user.deleteFlag ? 'retired' : ''}>
                                    <td className="action-column">
                                        <button className="icon-button edit-button" onClick={() => handleEdit(user.employeeNo)}>
                                            <img src="/icons/edit.svg" alt="編集" />
                                        </button>
                                    </td>
                                    <td className="action-column">
                                        <button className="icon-button detail-button" onClick={() => handleToggleDetails(user.employeeNo)}>...</button>
                                    </td>
                                    <td>{user.employeeNo}</td>
                                    <td>{user.name}</td>
                                    <td>{user.nameKana}</td>
                                    <td>{user.telNo}</td>
                                    <td>{user.mailAddress}</td>
                                    <td>{user.position}</td>
                                    <td>{user.accountLevel}</td>
                                    <td>{user.updateDate || '2025/08/20'}</td>
                                    {expandedRows.includes(user.employeeNo) && (
                                        <>
                                            <td className="detail-cell">{user.department}</td>
                                            <td className="detail-cell">{user.age}</td>
                                            <td className="detail-cell">{user.gender}</td>
                                            <td className="detail-cell">{user.retirementDate || 'N/A'}</td>
                                            <td className="detail-cell">{user.registrationDate}</td>
                                            <td className="detail-cell">{user.deleteFlag ? '削除済み' : '有効'}</td>
                                        </>
                                    )}
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            )}
        </>
    );
}