document.addEventListener('DOMContentLoaded', async () => {
    // Prevent unauthenticated access
    const tokens = window.api.getTokens();
    if (!tokens.accessToken) {
        window.location.href = 'login.html';
        return;
    }

    // Load user profile
    async function loadProfile() {
        const data = await window.api.users.getMe();
        if (data && data.success) {
            const user = data.data;
            
            // Map data to DOM
            document.querySelector('h1').textContent = user.fullName || user.email;
            document.querySelector('.text-sm.text-slate-500.font-medium').textContent = `@${user.publicCode} • Dự án SGU`;
            
            // Inputs
            const inputs = document.querySelectorAll('input');
            inputs.forEach(input => {
                if (input.type === 'text' && input.value === 'Tuấn Nguyễn') {
                    input.value = user.fullName;
                    input.id = 'input-fullname'; // Add ID for easier access
                } else if (input.type === 'text' && input.value === 'tuan.um') {
                    input.value = user.publicCode;
                    input.readOnly = true;
                } else if (input.type === 'date') {
                    input.value = user.dateOfBirth ? user.dateOfBirth.split('T')[0] : '';
                    input.id = 'input-dob';
                } else if (input.type === 'email') {
                    input.value = user.email;
                    input.readOnly = true;
                } else if (input.type === 'tel') {
                    input.value = user.phoneNumber;
                    input.id = 'input-phone';
                }
            });
            
            // Avatar initials
            const initials = user.fullName ? user.fullName.substring(0,2).toUpperCase() : 'U';
            document.querySelectorAll('.rounded-full, .rounded-2xl').forEach(el => {
                if(el.textContent.trim() === 'TU') el.textContent = initials;
            });
        }
    }

    await loadProfile();

    // Update Profile Button
    const updateBtn = document.querySelector('button.bg-blue-600');
    if (updateBtn && updateBtn.textContent.includes('Cập nhật hồ sơ')) {
        updateBtn.addEventListener('click', async () => {
            const originalText = updateBtn.innerHTML;
            updateBtn.innerHTML = 'Đang lưu...';
            updateBtn.disabled = true;

            const fullName = document.getElementById('input-fullname')?.value;
            const dob = document.getElementById('input-dob')?.value;
            const phone = document.getElementById('input-phone')?.value;

            const data = await window.api.users.updateMe({
                fullName: fullName,
                dateOfBirth: dob ? new Date(dob).toISOString() : null,
                phoneNumber: phone
            });

            if (data && data.success) {
                alert('Cập nhật hồ sơ thành công!');
                loadProfile();
            } else {
                alert('Có lỗi xảy ra: ' + (data?.message || 'Không thể cập nhật'));
            }

            updateBtn.innerHTML = originalText;
            updateBtn.disabled = false;
        });
    }

    // Logout logic from all devices (just normal logout for now)
    const logoutBtn = document.querySelector('.border-red-200.text-red-600');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', async () => {
            await window.api.auth.logout();
        });
    }
});
