const API_BASE_URL = window.location.origin;

let currentUserId = null;

async function fetchWithRetry(url, options, maxRetries = 1) {
    for (let i = 0; i <= maxRetries; i++) {
        try {
            const response = await fetch(url, options);
            return response;
        } catch (error) {
            if (i === maxRetries) {
                throw error;
            }
            await new Promise(resolve => setTimeout(resolve, Math.pow(2, i) * 1000));
        }
    }
}

function checkPasswordStrength(password) {
    if (!password) return { strength: 'Слабый', class: 'bg-secondary' };

    let score = 0;
    if (password.length >= 6) score++;
    if (password.length >= 8) score++;
    if (password.length >= 12) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/\d/.test(password)) score++;
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) score++;

    if (score <= 2) return { strength: 'Слабый', class: 'bg-danger' };
    if (score <= 4) return { strength: 'Средний', class: 'bg-warning' };
    if (score <= 5) return { strength: 'Сильный', class: 'bg-info' };
    return { strength: 'Очень сильный', class: 'bg-success' };
}

document.getElementById('registerPassword')?.addEventListener('input', (e) => {
    const password = e.target.value;
    const strengthBadge = document.getElementById('passwordStrength');
    
    if (strengthBadge) {
        const { strength, class: badgeClass } = checkPasswordStrength(password);
        strengthBadge.textContent = strength;
        strengthBadge.className = `badge ${badgeClass}`;
    }
});

function showMessage(elementId, message, isSuccess) {
    const messageDiv = document.getElementById(elementId);
    if (messageDiv) {
        messageDiv.textContent = message;
        messageDiv.className = isSuccess ? 'alert alert-success success' : 'alert alert-danger error';
        messageDiv.style.display = 'block';
    }
}

document.querySelectorAll('button[data-tab]').forEach(button => {
    button.addEventListener('click', () => {
        document.getElementById('loginMessage').style.display = 'none';
        document.getElementById('registerMessage').style.display = 'none';
    });
});

document.getElementById('loginForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const resultDiv = document.getElementById('loginResult');
    const submitButton = e.target.querySelector('button[type="submit"]');
    
    resultDiv.innerHTML = '<div class="alert alert-info">Выполняется вход...</div>';
    submitButton.disabled = true;
    submitButton.classList.add('loading');

    const username = document.getElementById('loginUsername').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const response = await fetchWithRetry(`${API_BASE_URL}/api/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                login: username,
                password: password
            })
        });

        const data = await response.json();

        if (response.ok && data.success) {
            if (data.entity && data.entity.userId) {
                currentUserId = data.entity.userId;
                document.getElementById('profileUserId').value = currentUserId;
            }
            
            resultDiv.innerHTML = `<div class="alert alert-success">
                <strong>Успешно!</strong> ${data.message}
                <pre class="mt-2">${JSON.stringify(data.entity, null, 2)}</pre>
            </div>`;
            showMessage('loginMessage', 'Вход выполнен успешно', true);
        } else {
            resultDiv.innerHTML = `<div class="alert alert-danger">
                <strong>Ошибка!</strong> ${data.message || 'Неверные учетные данные'}
            </div>`;
            showMessage('loginMessage', data.message || 'Неверные учетные данные', false);
        }
    } catch (error) {
        resultDiv.innerHTML = `<div class="alert alert-danger">
            <strong>Ошибка сети!</strong> ${error.message}
        </div>`;
        showMessage('loginMessage', 'Ошибка сети', false);
    } finally {
        submitButton.disabled = false;
        submitButton.classList.remove('loading');
    }
});

document.getElementById('registerForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const resultDiv = document.getElementById('registerResult');
    resultDiv.innerHTML = '<div class="alert alert-info">Выполняется регистрация...</div>';

    const username = document.getElementById('registerUsername').value;
    const password = document.getElementById('registerPassword').value;
    const email = document.getElementById('registerEmail').value;
    const firstName = document.getElementById('registerFirstName').value;
    const lastName = document.getElementById('registerLastName').value;

    if (password.length < 3) {
        showMessage('registerMessage', 'Пароль должен содержать минимум 3 символа', false);
        resultDiv.innerHTML = '<div class="alert alert-danger">Пароль должен содержать минимум 3 символа</div>';
        return;
    }

    try {
        const response = await fetchWithRetry(`${API_BASE_URL}/api/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                login: username,
                password: password,
                email: email || null,
                firstName: firstName || null,
                lastName: lastName || null
            })
        });

        const data = await response.json();

        if (response.ok && data.success) {
            if (data.createdCustomerId) {
                currentUserId = data.createdCustomerId;
                document.getElementById('profileUserId').value = currentUserId;
            }
            
            resultDiv.innerHTML = `<div class="alert alert-success">
                <strong>Успешно!</strong> ${data.message}
                ${data.createdCustomerId ? `<br><small>ID пользователя: ${data.createdCustomerId}</small>` : ''}
            </div>`;
            showMessage('registerMessage', `Регистрация успешна. ID: ${data.createdCustomerId}`, true);
        } else {
            resultDiv.innerHTML = `<div class="alert alert-danger">
                <strong>Ошибка!</strong> ${data.message || 'Ошибка регистрации'}
            </div>`;
            showMessage('registerMessage', data.message || 'Ошибка регистрации', false);
        }
    } catch (error) {
        resultDiv.innerHTML = `<div class="alert alert-danger">
            <strong>Ошибка сети!</strong> ${error.message}
        </div>`;
        showMessage('registerMessage', 'Ошибка сети', false);
    }
});

async function loadProfile() {
    const userId = document.getElementById('profileUserId').value;
    const resultDiv = document.getElementById('profileResult');

    if (!userId) {
        resultDiv.innerHTML = '<div class="alert alert-warning">Введите ID пользователя</div>';
        return;
    }

    resultDiv.innerHTML = '<div class="alert alert-info">Загрузка профиля...</div>';

    try {
        const response = await fetchWithRetry(`${API_BASE_URL}/api/userprofile/${userId}`);
        const data = await response.json();

        if (response.ok && data.success) {
            currentUserId = userId;
            
            document.getElementById('profileEmail').value = data.profile.email || '';
            document.getElementById('profileFirstName').value = data.profile.firstName || '';
            document.getElementById('profileLastName').value = data.profile.lastName || '';
            
            document.getElementById('profileContent').style.display = 'block';
            
            resultDiv.innerHTML = `<div class="alert alert-success">
                <strong>Профиль загружен</strong><br>
                Логин: ${data.profile.login}<br>
                Полное имя: ${data.profile.fullName}<br>
                Создан: ${new Date(data.profile.createdAt).toLocaleString('ru-RU')}
            </div>`;
        } else {
            resultDiv.innerHTML = `<div class="alert alert-danger">
                <strong>Ошибка!</strong> ${data.message || 'Не удалось загрузить профиль'}
            </div>`;
        }
    } catch (error) {
        resultDiv.innerHTML = `<div class="alert alert-danger">
            <strong>Ошибка сети!</strong> ${error.message}
        </div>`;
    }
}

document.getElementById('profileForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const resultDiv = document.getElementById('profileResult');

    if (!currentUserId) {
        resultDiv.innerHTML = '<div class="alert alert-warning">Сначала загрузите профиль</div>';
        return;
    }

    resultDiv.innerHTML = '<div class="alert alert-info">Обновление профиля...</div>';

    const email = document.getElementById('profileEmail').value;
    const firstName = document.getElementById('profileFirstName').value;
    const lastName = document.getElementById('profileLastName').value;

    try {
        const response = await fetchWithRetry(`${API_BASE_URL}/api/userprofile/${currentUserId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                email: email || null,
                firstName: firstName || null,
                lastName: lastName || null
            })
        });

        const data = await response.json();

        if (response.ok && data.success) {
            resultDiv.innerHTML = `<div class="alert alert-success">
                <strong>Успешно!</strong> ${data.message}
            </div>`;
        } else {
            resultDiv.innerHTML = `<div class="alert alert-danger">
                <strong>Ошибка!</strong> ${data.message || 'Ошибка обновления профиля'}
            </div>`;
        }
    } catch (error) {
        resultDiv.innerHTML = `<div class="alert alert-danger">
            <strong>Ошибка сети!</strong> ${error.message}
        </div>`;
    }
});

document.getElementById('changePasswordForm')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const resultDiv = document.getElementById('profileResult');

    if (!currentUserId) {
        resultDiv.innerHTML = '<div class="alert alert-warning">Сначала загрузите профиль</div>';
        return;
    }

    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (newPassword !== confirmPassword) {
        resultDiv.innerHTML = '<div class="alert alert-danger">Новый пароль и подтверждение не совпадают</div>';
        return;
    }

    resultDiv.innerHTML = '<div class="alert alert-info">Изменение пароля...</div>';

    try {
        const response = await fetchWithRetry(`${API_BASE_URL}/api/userprofile/${currentUserId}/change-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                currentPassword,
                newPassword,
                confirmPassword
            })
        });

        const data = await response.json();

        if (response.ok && data.success) {
            resultDiv.innerHTML = `<div class="alert alert-success">
                <strong>Успешно!</strong> ${data.message}
            </div>`;
            
            document.getElementById('currentPassword').value = '';
            document.getElementById('newPassword').value = '';
            document.getElementById('confirmPassword').value = '';
        } else {
            resultDiv.innerHTML = `<div class="alert alert-danger">
                <strong>Ошибка!</strong> ${data.message || 'Ошибка изменения пароля'}
            </div>`;
        }
    } catch (error) {
        resultDiv.innerHTML = `<div class="alert alert-danger">
            <strong>Ошибка сети!</strong> ${error.message}
        </div>`;
    }
});

async function checkHealth() {
    const resultDiv = document.getElementById('healthResult');
    resultDiv.innerHTML = '<div class="alert alert-info">Проверка здоровья API...</div>';

    try {
        const response = await fetch(`${API_BASE_URL}/api/health`);
        const data = await response.json();

        if (response.ok) {
            const soapStatus = data.checks.soapService.status;
            const soapClass = soapStatus === 'healthy' ? 'success' : soapStatus === 'degraded' ? 'warning' : 'danger';
            
            resultDiv.innerHTML = `<div class="alert alert-success">
                <h5><strong>✓ API Здорово</strong></h5>
                <hr>
                <div class="row">
                    <div class="col-md-6">
                        <strong>Сервис:</strong> ${data.service}<br>
                        <strong>Версия:</strong> ${data.version}<br>
                        <strong>Статус:</strong> <span class="badge bg-success">${data.status}</span>
                    </div>
                    <div class="col-md-6">
                        <strong>API:</strong> <span class="badge bg-success">${data.checks.api.status}</span><br>
                        <strong>SOAP Сервис:</strong> <span class="badge bg-${soapClass}">${soapStatus}</span><br>
                        <small class="text-muted">${data.checks.soapService.message}</small>
                    </div>
                </div>
                <hr>
                <small class="text-muted">Проверено: ${new Date(data.timestamp).toLocaleString('ru-RU')}</small>
            </div>`;
        } else {
            resultDiv.innerHTML = `<div class="alert alert-danger">
                <strong>Ошибка!</strong> API недоступен
            </div>`;
        }
    } catch (error) {
        resultDiv.innerHTML = `<div class="alert alert-danger">
            <strong>Ошибка сети!</strong> ${error.message}
        </div>`;
    }
}
