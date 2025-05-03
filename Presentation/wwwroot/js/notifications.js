const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

connection.on("ReceiveNotification", function (notification) {
    const notifications = document.querySelector('.notifications');

    const item = document.createElement('div');
    item.className = 'notification';
    item.setAttribute('data-id', notification.id);
    item.innerHTML = `
        <div class="image">
            <img src="${notification.image}" />
        </div>
        <span class="message">${notification.message}</span>
        <div class="time" data-created="${new Date(notification.created).toISOString()}">${notification.created}</div>
        <button class="btn-remove" onclick="dismissNotification('${notification.id}')"></button>
    `;

    notifications.insertBefore(item, notifications.firstChild);

    updateRelativeTimes();
    updateNotificationCount();
});

connection.on("NotificationDismissed", function (notificationId) {
    removeNotification(notificationId);
});

connection.start().catch(error => console.error(error));

// --- Functions ---

async function dismissNotification(notificationId) {
    try {
        const res = await fetch(`/api/notifications/dismiss/${notificationId}`, { method: 'POST' });
        if (res.ok) {
            removeNotification(notificationId);
        } else {
            console.error('Error removing notification');
        }
    } catch (error) {
        console.error('Error removing notification:', error);
    }
}

function removeNotification(notificationId) {
    const element = document.querySelector(`.notification[data-id="${notificationId}"]`);
    if (element) {
        element.remove();
        updateNotificationCount();
    }
}

function updateNotificationCount() {
    const notifications = document.querySelector('.notifications');
    const count = notifications.querySelectorAll('.notification').length;

    const badge = document.querySelector('.notificationNumber');
    if (badge) {
        badge.textContent = count;
    }

    const notificationDropdownButton = document.querySelector('#notification-dropdown-button');
    let dot = notificationDropdownButton.querySelector('.dot.dot-red.fa-solid.fa-circle');

    if (count > 0 && !dot) {
        dot = document.createElement('div');
        dot.className = 'dot dot-red fa-solid fa-circle';
        notificationDropdownButton.appendChild(dot);
    }

    if (count === 0 && dot) {
        dot.remove();
    }
}

function updateRelativeTimes() {
    const elements = document.querySelectorAll(".notification .time");
    const now = new Date();

    elements.forEach(el => {
        const created = new Date(el.getAttribute('data-created'));
        const diff = now - created;
        const diffMinutes = Math.floor(diff / 60000);
        const diffHours = Math.floor(diffMinutes / 60);
        const diffDays = Math.floor(diffHours / 24);
        const diffWeeks = Math.floor(diffDays / 7);

        let relativeTime = '';

        if (diffMinutes < 1) {
            relativeTime = '0 min ago';
        } else if (diffMinutes < 60) {
            relativeTime = `${diffMinutes} min ago`;
        } else if (diffHours < 2) {
            relativeTime = `${diffHours} hour ago`;
        } else if (diffHours < 24) {
            relativeTime = `${diffHours} hours ago`;
        } else if (diffDays < 2) {
            relativeTime = `${diffDays} day ago`;
        } else if (diffDays < 7) {
            relativeTime = `${diffDays} days ago`;
        } else {
            relativeTime = `${diffWeeks} weeks ago`;
        }

        el.textContent = relativeTime;
    });
}

// Auto-update relative times every 60 seconds
document.addEventListener('DOMContentLoaded', () => {
    updateRelativeTimes();
    setInterval(updateRelativeTimes, 60000);
});