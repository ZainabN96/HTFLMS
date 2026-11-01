(function () {
    "use strict";

    const notificationToggle = document.getElementById("notificationToggle");
    const notificationDropdown = document.getElementById("notificationDropdown");
    const notificationList = document.getElementById("notificationList");
    const notificationUnreadDot = document.getElementById("notificationUnreadDot");
    const notificationCount = document.getElementById("notificationCount");
    const notificationMarkAllRead = document.getElementById("notificationMarkAllRead");

    let unreadCount = 0;

    if (!notificationToggle || !notificationDropdown || !notificationList) {
        return;
    }

    document.addEventListener("DOMContentLoaded", function () {
        loadNotifications();
        loadUnreadCount();
        initializeSignalR();
    });

    notificationToggle.addEventListener("click", function (e) {
        e.stopPropagation();
        notificationDropdown.classList.toggle("show");
    });

    notificationDropdown.addEventListener("click", function (e) {
        e.stopPropagation();
    });

    document.addEventListener("click", function () {
        notificationDropdown.classList.remove("show");
    });

    notificationList.addEventListener("click", async function (e) {
        const item = e.target.closest("[data-notification-id]");

        if (!item) {
            return;
        }

        const notificationId = item.dataset.notificationId;
        const redirectUrl = item.dataset.redirectUrl;

        await markAsRead(notificationId);

        if (redirectUrl) {
            window.location.href = redirectUrl;
        }
    });

    if (notificationMarkAllRead) {
        notificationMarkAllRead.addEventListener("click", async function () {
            await markAllAsRead();
        });
    }

    async function loadNotifications() {
        try {
            const response = await fetch("/api/notifications", {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                return;
            }

            const result = await response.json();

            if (!result.success) {
                return;
            }

            renderNotifications(result.data || []);
        }
        catch (error) {
            console.error("Could not load notifications.", error);
        }
    }

    async function loadUnreadCount() {
        try {
            const response = await fetch("/api/notifications/unread-count", {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                return;
            }

            const result = await response.json();

            if (!result.success) {
                return;
            }

            unreadCount = result.count || 0;

            updateUnreadDisplay();
        }
        catch (error) {
            console.error("Could not load unread notification count.", error);
        }
    }

    function renderNotifications(notifications) {
        notificationList.innerHTML = "";

        if (!notifications || notifications.length === 0) {
            const emptyItem = document.createElement("div");

            emptyItem.className = "dashboard-notification-item";
            emptyItem.textContent = "No notifications yet.";

            notificationList.appendChild(emptyItem);

            return;
        }

        notifications.forEach(function (notification) {
            const item = createNotificationElement(notification);

            notificationList.appendChild(item);
        });
    }

    function createNotificationElement(notification) {
        const item = document.createElement("div");

        item.className = "dashboard-notification-item";

        item.dataset.notificationId = notification.id;

        if (notification.redirectUrl) {
            item.dataset.redirectUrl = notification.redirectUrl;
        }

        const title = document.createElement("strong");
        title.textContent = notification.title || "Notification";

        const message = document.createElement("div");
        message.textContent = notification.message || "";

        item.appendChild(title);
        item.appendChild(message);

        return item;
    }

    async function markAsRead(notificationId) {
        if (!notificationId) {
            return;
        }

        try {
            const response = await fetch(
                `/api/notifications/${notificationId}/read`,
                {
                    method: "POST",
                    headers: {
                        "Accept": "application/json"
                    }
                });

            if (!response.ok) {
                return;
            }

            if (unreadCount > 0) {
                unreadCount--;
            }

            updateUnreadDisplay();
        }
        catch (error) {
            console.error("Could not mark notification as read.", error);
        }
    }

    async function markAllAsRead() {
        try {
            const response = await fetch("/api/notifications/read-all", {
                method: "POST",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                return;
            }

            unreadCount = 0;

            updateUnreadDisplay();

            await loadNotifications();
        }
        catch (error) {
            console.error("Could not mark all notifications as read.", error);
        }
    }

    function updateUnreadDisplay() {
        if (notificationUnreadDot) {
            notificationUnreadDot.hidden = unreadCount <= 0;
        }

        if (notificationCount) {
            notificationCount.textContent =
                unreadCount > 0
                    ? ` (${unreadCount})`
                    : "";
        }
    }

    function prependNotification(notification) {
        const emptyItem = notificationList.querySelector(
            ".dashboard-notification-item:not([data-notification-id])"
        );

        if (emptyItem &&
            emptyItem.textContent.trim() === "No notifications yet.") {

            emptyItem.remove();
        }

        const item = createNotificationElement(notification);

        notificationList.prepend(item);

        const notificationItems =
            notificationList.querySelectorAll("[data-notification-id]");

        if (notificationItems.length > 10) {
            notificationItems[notificationItems.length - 1].remove();
        }
    }

    function initializeSignalR() {
        if (typeof signalR === "undefined") {
            console.error("SignalR client library was not loaded.");
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveNotification", function (notification) {
            prependNotification(notification);

            unreadCount++;

            updateUnreadDisplay();
        });

        connection.start()
            .then(function () {
                console.log("Notification SignalR connected.");
            })
            .catch(function (error) {
                console.error(
                    "Notification SignalR connection failed.",
                    error
                );
            });

        connection.onreconnected(function () {
            loadNotifications();
            loadUnreadCount();
        });
    }

})();