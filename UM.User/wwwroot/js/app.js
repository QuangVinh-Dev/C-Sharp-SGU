document.addEventListener('DOMContentLoaded', async () => {
    // Prevent unauthenticated access
    const tokens = window.api.getTokens();
    if (!tokens.accessToken) {
        window.location.href = 'login.html';
        return;
    }

    // Load current user for navbar
    async function loadCurrentUser() {
        const data = await window.api.users.getMe();
        if (data && data.success) {
            const user = data.data;
            const initials = user.fullName ? user.fullName.substring(0, 2).toUpperCase() : 'U';

            // Update header avatar and name
            document.querySelectorAll('.rounded-full').forEach(el => {
                if (el.textContent.trim() === 'TU') el.textContent = initials;
            });
            const nameSpan = document.querySelector('.group-hover\\:text-blue-600');
            if (nameSpan && nameSpan.textContent === 'Tuấn') {
                nameSpan.textContent = user.fullName || 'User';
            }
        }
    }

    await loadCurrentUser();
});
