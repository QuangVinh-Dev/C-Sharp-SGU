const API_BASE_URL = 'http://localhost:5001/api';

const api = {
    getTokens() {
        return {
            accessToken: localStorage.getItem('accessToken'),
            refreshToken: localStorage.getItem('refreshToken')
        };
    },

    setTokens(accessToken, refreshToken) {
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
    },

    clearTokens() {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    },

    async request(endpoint, options = {}) {
        const url = `${API_BASE_URL}${endpoint}`;
        
        let headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        const { accessToken } = this.getTokens();
        if (accessToken) {
            headers['Authorization'] = `Bearer ${accessToken}`;
        }

        const config = {
            ...options,
            headers
        };

        let response = await fetch(url, config);

        // Handle 401 Unauthorized (Token expired)
        if (response.status === 401 && accessToken) {
            const { refreshToken } = this.getTokens();
            if (refreshToken) {
                const refreshed = await this.refreshToken(refreshToken);
                if (refreshed) {
                    // Retry original request with new token
                    headers['Authorization'] = `Bearer ${this.getTokens().accessToken}`;
                    config.headers = headers;
                    response = await fetch(url, config);
                } else {
                    this.clearTokens();
                    window.location.href = 'login.html';
                    return null;
                }
            } else {
                this.clearTokens();
                window.location.href = 'login.html';
                return null;
            }
        }

        // Handle 403 Forbidden
        if (response.status === 403) {
            alert('Access Denied: You do not have permission to perform this action.');
            return null;
        }

        return response;
    },

    async refreshToken(token) {
        try {
            const response = await fetch(`${API_BASE_URL}/Auth/refresh`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ refreshToken: token })
            });

            if (response.ok) {
                const data = await response.json();
                if (data.success && data.data) {
                    this.setTokens(data.data.accessToken, data.data.refreshToken);
                    return true;
                }
            }
            return false;
        } catch (error) {
            console.error('Refresh token error:', error);
            return false;
        }
    },

    // Auth API
    auth: {
        login: async (email, password) => {
            const response = await api.request('/Auth/login', {
                method: 'POST',
                body: JSON.stringify({ email, password })
            });
            if (!response) return null;
            return await response.json();
        },
        register: async (data) => {
            const response = await api.request('/Auth/register', {
                method: 'POST',
                body: JSON.stringify(data)
            });
            if (!response) return null;
            return await response.json();
        },
        logout: async () => {
            const { refreshToken } = api.getTokens();
            if (refreshToken) {
                await api.request('/Auth/logout', {
                    method: 'POST',
                    body: JSON.stringify({ refreshToken })
                });
            }
            api.clearTokens();
            window.location.href = 'login.html';
        },
        forgotPassword: async (email) => {
            const response = await api.request('/Auth/forgot-password', {
                method: 'POST',
                body: JSON.stringify({ email })
            });
            if (!response) return null;
            return await response.json();
        }
    },

    // Users API
    users: {
        getMe: async () => {
            const response = await api.request('/Users/me', { method: 'GET' });
            if (!response) return null;
            return await response.json();
        },
        updateMe: async (data) => {
            const response = await api.request('/Users/me', {
                method: 'PUT',
                body: JSON.stringify(data)
            });
            if (!response) return null;
            return await response.json();
        },
        getUsers: async () => {
            const response = await api.request('/Users', { method: 'GET' });
            if (!response) return null;
            return await response.json();
        }
    }
};

window.api = api;
