// API Base URL
const API_BASE_URL = window.location.origin;

// Retry logic with exponential backoff
async function fetchWithRetry(url, options = {}, maxRetries = 1, retryDelay = 1000) {
    let lastError;
    
    for (let attempt = 0; attempt <= maxRetries; attempt++) {
        try {
            const response = await fetch(url, options);
            return response;
        } catch (error) {
            lastError = error;
            
            // Only retry on network errors, not on HTTP errors
            if (attempt < maxRetries) {
                // Exponential backoff: delay * 2^attempt
                const delay = retryDelay * Math.pow(2, attempt);
                console.log(`Network error, retrying in ${delay}ms... (attempt ${attempt + 1}/${maxRetries + 1})`);
                await new Promise(resolve => setTimeout(resolve, delay));
            } else {
                throw error;
            }
        }
    }
    
    throw lastError;
}

// Tab Switching
document.querySelectorAll('.tab-button').forEach(button => {
    button.addEventListener('click', () => {
        const tabName = button.getAttribute('data-tab');
        
        // Update active tab button
        document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
        button.classList.add('active');
        
        // Update active form
        document.querySelectorAll('.form').forEach(form => form.classList.remove('active'));
        document.getElementById(`${tabName}Form`).classList.add('active');
        
        // Clear messages
        clearMessages();
    });
});

// Login Form Handler
document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    clearMessages();
    
    const form = e.target;
    const submitButton = form.querySelector('button[type="submit"]');
    const messageDiv = document.getElementById('loginMessage');
    
    const formData = {
        login: form.login.value.trim(),
        password: form.password.value
    };
    
    // Validation
    if (!formData.login || !formData.password) {
        showMessage(messageDiv, 'Пожалуйста, заполните все поля', 'error');
        return;
    }
    
    // Disable button and show loading
    submitButton.disabled = true;
    submitButton.classList.add('loading');
    
    try {
        const response = await fetchWithRetry(`${API_BASE_URL}/api/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        }, 1, 1000); // 1 retry with 1 second initial delay
        
        const data = await response.json();
        
        if (response.ok && data.success) {
            showMessage(messageDiv, 'Вход выполнен успешно!', 'success');
            console.log('Login successful:', data.entity);
            // Here you can redirect or update UI
        } else {
            const status = response.status;
            if (status === 401) {
                showMessage(messageDiv, data.message || 'Неверный логин или пароль', 'error');
            } else if (status === 400) {
                showMessage(messageDiv, data.message || 'Ошибка валидации данных', 'error');
            } else {
                showMessage(messageDiv, data.message || 'Произошла ошибка при входе', 'error');
            }
        }
    } catch (error) {
        console.error('Login error:', error);
        showMessage(messageDiv, 'Ошибка сети. Проверьте подключение к интернету.', 'error');
    } finally {
        submitButton.disabled = false;
        submitButton.classList.remove('loading');
    }
});

// Register Form Handler
document.getElementById('registerForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    clearMessages();
    
    const form = e.target;
    const submitButton = form.querySelector('button[type="submit"]');
    const messageDiv = document.getElementById('registerMessage');
    
    const formData = {
        login: form.login.value.trim(),
        password: form.password.value,
        email: form.email?.value.trim() || null,
        firstName: form.firstName?.value.trim() || null,
        lastName: form.lastName?.value.trim() || null
    };
    
    // Validation
    if (!formData.login || !formData.password) {
        showMessage(messageDiv, 'Логин и пароль обязательны для заполнения', 'error');
        return;
    }
    
    if (formData.password.length < 3) {
        showMessage(messageDiv, 'Пароль должен содержать минимум 3 символа', 'error');
        return;
    }
    
    // Remove null values
    Object.keys(formData).forEach(key => {
        if (formData[key] === null || formData[key] === '') {
            delete formData[key];
        }
    });
    
    // Disable button and show loading
    submitButton.disabled = true;
    submitButton.classList.add('loading');
    
    try {
        const response = await fetchWithRetry(`${API_BASE_URL}/api/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        }, 1, 1000); // 1 retry with 1 second initial delay
        
        const data = await response.json();
        
        if (response.ok && data.success) {
            const successMsg = data.createdCustomerId 
                ? `Регистрация успешна! ID клиента: ${data.createdCustomerId}`
                : 'Регистрация успешна!';
            showMessage(messageDiv, successMsg, 'success');
            console.log('Registration successful:', data);
            // Optionally reset form or switch to login tab
            setTimeout(() => {
                form.reset();
                document.querySelector('[data-tab="login"]').click();
            }, 2000);
        } else {
            const status = response.status;
            if (status === 400) {
                showMessage(messageDiv, data.message || 'Ошибка регистрации', 'error');
            } else {
                showMessage(messageDiv, data.message || 'Произошла ошибка при регистрации', 'error');
            }
        }
    } catch (error) {
        console.error('Registration error:', error);
        showMessage(messageDiv, 'Ошибка сети. Проверьте подключение к интернету.', 'error');
    } finally {
        submitButton.disabled = false;
        submitButton.classList.remove('loading');
    }
});

// Helper Functions
function showMessage(element, message, type) {
    element.textContent = message;
    element.className = `message ${type} show`;
    
    // Auto-hide success messages after 5 seconds
    if (type === 'success') {
        setTimeout(() => {
            element.classList.remove('show');
        }, 5000);
    }
}

function clearMessages() {
    document.querySelectorAll('.message').forEach(msg => {
        msg.classList.remove('show', 'success', 'error', 'warning');
        msg.textContent = '';
    });
}

