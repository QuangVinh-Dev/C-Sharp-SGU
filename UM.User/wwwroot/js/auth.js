document.addEventListener('DOMContentLoaded', () => {

    // Toggle password visibility
    const toggleBtns = document.querySelectorAll('.password-toggle');
    toggleBtns.forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const input = this.parentElement.querySelector('input');
            if (input.type === 'password') {
                input.type = 'text';
                this.textContent = '🙈';
            } else {
                input.type = 'password';
                this.textContent = '👁';
            }
        });
    });

    // Helper functions
    function showError(inputId, message) {
        const input = document.getElementById(inputId);
        if (!input) return;
        const group = input.closest('.form-group');
        group.classList.add('has-error');
        let errorEl = group.querySelector('.error-text');
        if (!errorEl) {
            errorEl = document.createElement('div');
            errorEl.className = 'error-text';
            group.appendChild(errorEl);
        }
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    }

    function clearError(inputId) {
        const input = document.getElementById(inputId);
        if (!input) return;
        const group = input.closest('.form-group');
        group.classList.remove('has-error');
        const errorEl = group.querySelector('.error-text');
        if (errorEl) errorEl.style.display = 'none';
    }

    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    // Login Form Validation
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', (e) => {
            e.preventDefault();
            
            const emailInput = document.getElementById('email');
            const passInput = document.getElementById('password');
            const btn = loginForm.querySelector('button[type="submit"]');
            
            let isValid = true;
            
            if (!emailInput.value.trim()) {
                showError('email', 'Please enter your email.');
                isValid = false;
            } else if (!isValidEmail(emailInput.value.trim())) {
                showError('email', 'Please enter a valid email address.');
                isValid = false;
            } else {
                clearError('email');
            }

            if (!passInput.value) {
                showError('password', 'Password is required.');
                isValid = false;
            } else {
                clearError('password');
            }

            if (isValid) {
                // Call existing backend login API here.
                const originalText = btn.innerHTML;
                btn.innerHTML = 'Logging in...';
                btn.disabled = true;

                window.api.auth.login(emailInput.value.trim(), passInput.value)
                    .then(data => {
                        if (data && data.success) {
                            window.api.setTokens(data.data.accessToken, data.data.refreshToken);
                            window.location.href = 'main.html';
                        } else {
                            showError('password', data?.message || 'Login failed.');
                        }
                    })
                    .catch(err => {
                        showError('password', 'Network error. Please try again.');
                    })
                    .finally(() => {
                        btn.innerHTML = originalText;
                        btn.disabled = false;
                    });
            }
        });
    }

    // Register Form Validation & Password Strength
    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        const passInput = document.getElementById('password');
        const strengthMeters = document.querySelectorAll('.strength-meter');
        const strengthText = document.querySelector('.strength-text');
        const strengthContainer = document.querySelector('.password-strength');

        if (passInput && strengthMeters.length) {
            passInput.addEventListener('input', (e) => {
                const val = e.target.value;
                if (val.length > 0) {
                    strengthContainer.style.display = 'flex';
                    strengthText.style.display = 'block';
                } else {
                    strengthContainer.style.display = 'none';
                    strengthText.style.display = 'none';
                }
                
                let score = 0;
                if (val.length >= 8) score++;
                if (/[A-Z]/.test(val)) score++;
                if (/[0-9]/.test(val)) score++;
                if (/[^A-Za-z0-9]/.test(val)) score++;

                strengthMeters.forEach(m => m.style.backgroundColor = 'var(--border)');

                if (score === 1 || (val.length > 0 && score === 0)) {
                    strengthMeters[0].style.backgroundColor = 'var(--danger)';
                    strengthText.textContent = 'Weak';
                    strengthText.style.color = 'var(--danger)';
                } else if (score === 2) {
                    strengthMeters[0].style.backgroundColor = 'var(--warning)';
                    strengthMeters[1].style.backgroundColor = 'var(--warning)';
                    strengthText.textContent = 'Fair';
                    strengthText.style.color = 'var(--warning)';
                } else if (score === 3) {
                    strengthMeters[0].style.backgroundColor = 'var(--success)';
                    strengthMeters[1].style.backgroundColor = 'var(--success)';
                    strengthMeters[2].style.backgroundColor = 'var(--success)';
                    strengthText.textContent = 'Good';
                    strengthText.style.color = 'var(--success)';
                } else if (score >= 4) {
                    strengthMeters.forEach(m => m.style.backgroundColor = 'var(--success)');
                    strengthText.textContent = 'Strong';
                    strengthText.style.color = 'var(--success)';
                }
            });
        }

        registerForm.addEventListener('submit', (e) => {
            e.preventDefault();
            
            const name = document.getElementById('name');
            const email = document.getElementById('email');
            const phone = document.getElementById('phone');
            const dob = document.getElementById('dob');
            const password = document.getElementById('password');
            const confirm = document.getElementById('confirmPassword');
            const terms = document.getElementById('terms');
            const btn = registerForm.querySelector('button[type="submit"]');

            let isValid = true;

            if (!name.value.trim()) { showError('name', 'Full name is required'); isValid = false; } else clearError('name');
            if (!email.value.trim() || !isValidEmail(email.value.trim())) { showError('email', 'Valid work email is required'); isValid = false; } else clearError('email');
            if (!phone.value.trim()) { showError('phone', 'Phone number is required'); isValid = false; } else clearError('phone');
            if (!dob.value) { showError('dob', 'Date of birth is required'); isValid = false; } else clearError('dob');
            if (!password.value) { showError('password', 'Password is required'); isValid = false; } else clearError('password');
            
            if (password.value !== confirm.value) {
                showError('confirmPassword', 'Passwords do not match');
                isValid = false;
            } else if (!confirm.value) {
                showError('confirmPassword', 'Please confirm your password');
                isValid = false;
            } else {
                clearError('confirmPassword');
            }

            if (!terms.checked) {
                alert('You must agree to the Terms of Service');
                isValid = false;
            }

            if (isValid) {
                const originalText = btn.innerHTML;
                btn.innerHTML = 'Creating account...';
                btn.disabled = true;

                const registerData = {
                    fullName: name.value.trim(),
                    email: email.value.trim(),
                    phoneNumber: phone.value.trim(),
                    dateOfBirth: dob.value,
                    password: password.value
                };

                window.api.auth.register(registerData)
                    .then(data => {
                        if (data && data.success) {
                            alert(data.message || 'Registration successful!');
                            window.location.href = 'login.html';
                        } else {
                            showError('email', data?.message || 'Registration failed.');
                        }
                    })
                    .catch(err => {
                        showError('email', 'Network error. Please try again.');
                    })
                    .finally(() => {
                        btn.innerHTML = originalText;
                        btn.disabled = false;
                    });
            }
        });
    }

    // Forgot Password Form Validation
    const forgotForm = document.getElementById('forgotForm');
    if (forgotForm) {
        forgotForm.addEventListener('submit', (e) => {
            e.preventDefault();
            const email = document.getElementById('email');
            const btn = forgotForm.querySelector('button[type="submit"]');
            
            if (!email.value.trim() || !isValidEmail(email.value.trim())) {
                showError('email', 'Please enter a valid work email.');
            } else {
                clearError('email');
                btn.innerHTML = 'Sending reset link...';
                btn.disabled = true;

                window.api.auth.forgotPassword(email.value.trim())
                    .then(data => {
                        window.location.href = 'email-sent.html?email=' + encodeURIComponent(email.value);
                    })
                    .catch(err => {
                        showError('email', 'Network error. Please try again.');
                        btn.innerHTML = 'Reset Password';
                        btn.disabled = false;
                    });
            }
        });
    }

    // Email Sent UI mapping
    if (window.location.pathname.includes('email-sent.html')) {
        const params = new URLSearchParams(window.location.search);
        const email = params.get('email');
        if (email) {
            const emailChip = document.getElementById('displayEmail');
            if (emailChip) emailChip.textContent = email;
        }
    }

});
