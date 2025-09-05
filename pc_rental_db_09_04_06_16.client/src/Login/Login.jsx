/* eslint-disable no-unused-vars */
import React, { useState } from 'react';
import './Login.css';

const Login = ({ onLoginSuccess }) => {
    const [id, setId] = useState('');
    const [password, setPassword] = useState('');
    const [errors, setErrors] = useState({});
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setErrors({});
        setLoading(true);
        try {
            const res = await fetch('/auth/login', { // Vite��proxy���g���Ȃ瑊�΃p�X
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ employeeNo: id, password })
            });
            const data = await res.json().catch(() => ({}));

            if (res.ok) {
                // �e�iApp�j�֒ʒm�BemployeeNo ��n��
                onLoginSuccess?.(data.employeeNo ?? id);
            } else {
                setErrors({ general: data.message || '���O�C���Ɏ��s���܂���' });
            }
        } catch {
            setErrors({ general: '�T�[�o�[�ɐڑ��ł��܂���' });
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="login-box">
            <h1>Login</h1>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label>ID</label>
                    <input type="text" value={id} onChange={(e) => setId(e.target.value)} />
                </div>
                <div className="form-group">
                    <label>Password</label>
                    <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
                </div>
                {errors.general && <div className="error general">{errors.general}</div>}
                <button type="submit" disabled={loading}>{loading ? '���M��...' : 'Login'}</button>
            </form>
        </div>
    );
};

export default Login;
