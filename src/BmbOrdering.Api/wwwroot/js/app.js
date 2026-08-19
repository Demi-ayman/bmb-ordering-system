"use strict";

const API_ROOT = "/api/v1";
const SESSION_KEY = "bmb-ordering-session";
const ADMIN_ROLE = "Administrator";
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

const state = {
    token: null,
    profile: null,
    roles: [],
    orderView: "mine"
};

const elements = {
    authView: document.querySelector("#authView"),
    dashboardView: document.querySelector("#dashboardView"),
    sessionActions: document.querySelector("#sessionActions"),
    headerUserName: document.querySelector("#headerUserName"),
    logoutButton: document.querySelector("#logoutButton"),
    loginTab: document.querySelector("#loginTab"),
    registerTab: document.querySelector("#registerTab"),
    loginForm: document.querySelector("#loginForm"),
    registerForm: document.querySelector("#registerForm"),
    loginEmail: document.querySelector("#loginEmail"),
    customerName: document.querySelector("#customerName"),
    customerEmail: document.querySelector("#customerEmail"),
    roleBadge: document.querySelector("#roleBadge"),
    banNotice: document.querySelector("#banNotice"),
    createOrderForm: document.querySelector("#createOrderForm"),
    orderItems: document.querySelector("#orderItems"),
    orderItemTemplate: document.querySelector("#orderItemTemplate"),
    addItemButton: document.querySelector("#addItemButton"),
    estimatedTotal: document.querySelector("#estimatedTotal"),
    refreshOrdersButton: document.querySelector("#refreshOrdersButton"),
    myOrdersTab: document.querySelector("#myOrdersTab"),
    allOrdersTab: document.querySelector("#allOrdersTab"),
    ordersTitle: document.querySelector("#ordersTitle"),
    ordersList: document.querySelector("#ordersList"),
    toast: document.querySelector("#toast"),
    loadingOverlay: document.querySelector("#loadingOverlay")
};

let activeRequests = 0;
let toastTimer;

function showLoading() {
    activeRequests += 1;
    elements.loadingOverlay.hidden = false;
}

function hideLoading() {
    activeRequests = Math.max(0, activeRequests - 1);
    elements.loadingOverlay.hidden = activeRequests === 0;
}

function showToast(message, type = "success") {
    clearTimeout(toastTimer);
    elements.toast.textContent = message;
    elements.toast.classList.toggle("error", type === "error");
    elements.toast.hidden = false;
    toastTimer = setTimeout(() => {
        elements.toast.hidden = true;
    }, 4500);
}

function formatMoney(value) {
    return new Intl.NumberFormat("en-EG", {
        style: "currency",
        currency: "EGP"
    }).format(Number(value) || 0);
}

function formatDate(value) {
    if (!value) {
        return "—";
    }

    return new Intl.DateTimeFormat("en-GB", {
        dateStyle: "medium",
        timeStyle: "short"
    }).format(new Date(value));
}

function decodeToken(token) {
    try {
        const encodedPayload = token.split(".")[1]
            .replace(/-/g, "+")
            .replace(/_/g, "/");
        const padding = "=".repeat((4 - encodedPayload.length % 4) % 4);
        return JSON.parse(atob(encodedPayload + padding));
    } catch {
        return null;
    }
}

function getRoles(token) {
    const payload = decodeToken(token);
    const value = payload?.[ROLE_CLAIM] ?? payload?.role ?? [];

    if (Array.isArray(value)) {
        return value;
    }

    return value ? [value] : [];
}

function tokenIsExpired(token) {
    const expiration = decodeToken(token)?.exp;
    return !expiration || expiration * 1000 <= Date.now();
}

function persistSession() {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify({
        token: state.token,
        profile: state.profile
    }));
}

function restoreSession() {
    try {
        const saved = JSON.parse(sessionStorage.getItem(SESSION_KEY));

        if (!saved?.token || tokenIsExpired(saved.token)) {
            sessionStorage.removeItem(SESSION_KEY);
            return false;
        }

        state.token = saved.token;
        state.profile = saved.profile;
        state.roles = getRoles(saved.token);
        return true;
    } catch {
        sessionStorage.removeItem(SESSION_KEY);
        return false;
    }
}

function clearSession() {
    sessionStorage.removeItem(SESSION_KEY);
    state.token = null;
    state.profile = null;
    state.roles = [];
    state.orderView = "mine";
}

function getErrorMessage(problem, status) {
    if (problem?.errors) {
        const messages = Object.values(problem.errors).flat();
        if (messages.length > 0) {
            return messages.join(" ");
        }
    }

    return problem?.detail || problem?.title || `Request failed (${status}).`;
}

async function apiRequest(path, options = {}) {
    const headers = new Headers(options.headers || {});

    if (options.body) {
        headers.set("Content-Type", "application/json");
    }

    if (state.token) {
        headers.set("Authorization", `Bearer ${state.token}`);
    }

    showLoading();

    try {
        const response = await fetch(`${API_ROOT}${path}`, {
            ...options,
            headers
        });

        const contentType = response.headers.get("content-type") || "";
        const body = contentType.includes("json")
            ? await response.json()
            : null;

        if (!response.ok) {
            if (response.status === 401 && state.token) {
                clearSession();
                showAuthView();
            }

            throw new Error(getErrorMessage(body, response.status));
        }

        return body;
    } finally {
        hideLoading();
    }
}

function setAuthMode(mode) {
    const loginMode = mode === "login";
    elements.loginForm.hidden = !loginMode;
    elements.registerForm.hidden = loginMode;
    elements.loginTab.classList.toggle("active", loginMode);
    elements.registerTab.classList.toggle("active", !loginMode);
}

function showAuthView() {
    elements.authView.hidden = false;
    elements.dashboardView.hidden = true;
    elements.sessionActions.hidden = true;
    setAuthMode("login");
}

function updateBanNotice(bannedUntilUtc) {
    if (!bannedUntilUtc || new Date(bannedUntilUtc) <= new Date()) {
        elements.banNotice.hidden = true;
        return;
    }

    elements.banNotice.textContent =
        `Ordering is temporarily blocked until ${formatDate(bannedUntilUtc)}. ` +
        "You can still review and delete existing orders.";
    elements.banNotice.hidden = false;
}

async function showDashboard() {
    const isAdministrator = state.roles.includes(ADMIN_ROLE);
    elements.authView.hidden = true;
    elements.dashboardView.hidden = false;
    elements.sessionActions.hidden = false;
    elements.headerUserName.textContent = state.profile.fullName;
    elements.customerName.textContent = state.profile.fullName;
    elements.customerEmail.textContent = state.profile.email;
    elements.roleBadge.textContent = isAdministrator ? "Administrator" : "Customer";
    elements.roleBadge.classList.toggle("admin", isAdministrator);
    elements.allOrdersTab.hidden = !isAdministrator;
    updateBanNotice(state.profile.bannedUntilUtc);

    if (elements.orderItems.children.length === 0) {
        addOrderItem();
    }

    await loadOrders();
}

function addOrderItem(values = {}) {
    const fragment = elements.orderItemTemplate.content.cloneNode(true);
    const row = fragment.querySelector(".order-item-row");
    row.querySelector(".product-name").value = values.productName || "";
    row.querySelector(".quantity").value = values.quantity || 1;
    row.querySelector(".unit-price").value = values.unitPrice || "0.00";

    row.querySelector(".remove-item").addEventListener("click", () => {
        if (elements.orderItems.children.length === 1) {
            showToast("An order requires at least one item.", "error");
            return;
        }

        row.remove();
        updateEstimatedTotal();
    });

    row.querySelectorAll("input").forEach(input => {
        input.addEventListener("input", updateEstimatedTotal);
    });

    elements.orderItems.appendChild(fragment);
    updateEstimatedTotal();
}

function updateEstimatedTotal() {
    const total = [...elements.orderItems.querySelectorAll(".order-item-row")]
        .reduce((sum, row) => {
            const quantity = Number(row.querySelector(".quantity").value) || 0;
            const price = Number(row.querySelector(".unit-price").value) || 0;
            return sum + quantity * price;
        }, 0);

    elements.estimatedTotal.textContent = formatMoney(total);
}

function readOrderItems() {
    return [...elements.orderItems.querySelectorAll(".order-item-row")]
        .map(row => ({
            productName: row.querySelector(".product-name").value.trim(),
            quantity: Number(row.querySelector(".quantity").value),
            unitPrice: Number(row.querySelector(".unit-price").value)
        }));
}

function createTextElement(tag, className, text) {
    const element = document.createElement(tag);
    element.className = className;
    element.textContent = text;
    return element;
}

function renderOrders(orders) {
    elements.ordersList.replaceChildren();

    if (orders.length === 0) {
        const empty = createTextElement(
            "div",
            "empty-state",
            state.orderView === "all"
                ? "No orders are available in the system."
                : "You have no active orders yet."
        );
        elements.ordersList.appendChild(empty);
        return;
    }

    orders.forEach(order => {
        const card = document.createElement("article");
        card.className = "order-card";

        const top = document.createElement("div");
        top.className = "order-card-top";
        const identity = document.createElement("div");
        identity.appendChild(createTextElement("h3", "order-number", order.orderNumber));
        identity.appendChild(createTextElement("p", "order-date", formatDate(order.createdAtUtc)));

        if (state.orderView === "all") {
            identity.appendChild(createTextElement(
                "p",
                "customer-reference",
                `Customer: ${order.customerId}`
            ));
        }

        const badge = createTextElement("span", "status-badge", order.status);
        badge.classList.toggle("deleted", order.status === "Deleted");
        top.append(identity, badge);

        const lines = document.createElement("div");
        lines.className = "order-lines";
        order.items.forEach(item => {
            const line = document.createElement("div");
            line.className = "order-line";
            line.append(
                createTextElement("span", "", `${item.productName} × ${item.quantity}`),
                createTextElement("strong", "", formatMoney(item.lineTotal))
            );
            lines.appendChild(line);
        });

        const bottom = document.createElement("div");
        bottom.className = "order-card-bottom";
        bottom.appendChild(createTextElement("strong", "order-total", formatMoney(order.totalAmount)));

        if (state.orderView === "mine" && order.status !== "Deleted") {
            const deleteButton = createTextElement("button", "button button-danger", "Delete order");
            deleteButton.type = "button";
            deleteButton.addEventListener("click", () => deleteOrder(order));
            bottom.appendChild(deleteButton);
        }

        card.append(top, lines, bottom);
        elements.ordersList.appendChild(card);
    });
}

async function loadOrders() {
    try {
        const path = state.orderView === "all" ? "/orders/all" : "/orders";
        const orders = await apiRequest(path);
        renderOrders(orders || []);
    } catch (error) {
        showToast(error.message, "error");
    }
}

async function deleteOrder(order) {
    if (!window.confirm(`Delete order ${order.orderNumber}?`)) {
        return;
    }

    try {
        const result = await apiRequest(`/orders/${order.id}`, {
            method: "DELETE"
        });

        if (result.bannedUntilUtc) {
            state.profile.bannedUntilUtc = result.bannedUntilUtc;
            persistSession();
            updateBanNotice(result.bannedUntilUtc);
        }

        showToast(
            `Order deleted. Qualifying deletions today: ${result.qualifyingDeletionCount}.`
        );
        await loadOrders();
    } catch (error) {
        showToast(error.message, "error");
    }
}

elements.loginTab.addEventListener("click", () => setAuthMode("login"));
elements.registerTab.addEventListener("click", () => setAuthMode("register"));
elements.addItemButton.addEventListener("click", () => addOrderItem());
elements.refreshOrdersButton.addEventListener("click", loadOrders);

elements.logoutButton.addEventListener("click", () => {
    clearSession();
    showAuthView();
    showToast("You have signed out.");
});

elements.myOrdersTab.addEventListener("click", async () => {
    state.orderView = "mine";
    elements.myOrdersTab.classList.add("active");
    elements.allOrdersTab.classList.remove("active");
    elements.ordersTitle.textContent = "My orders";
    await loadOrders();
});

elements.allOrdersTab.addEventListener("click", async () => {
    state.orderView = "all";
    elements.allOrdersTab.classList.add("active");
    elements.myOrdersTab.classList.remove("active");
    elements.ordersTitle.textContent = "All system orders";
    await loadOrders();
});

elements.registerForm.addEventListener("submit", async event => {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);

    try {
        await apiRequest("/auth/register", {
            method: "POST",
            body: JSON.stringify({
                fullName: data.get("fullName"),
                email: data.get("email"),
                password: data.get("password"),
                passwordConfirmation: data.get("passwordConfirmation")
            })
        });

        elements.loginEmail.value = data.get("email");
        form.reset();
        setAuthMode("login");
        showToast("Account created. You can now sign in.");
    } catch (error) {
        showToast(error.message, "error");
    }
});

elements.loginForm.addEventListener("submit", async event => {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);

    try {
        const result = await apiRequest("/auth/login", {
            method: "POST",
            body: JSON.stringify({
                email: data.get("email"),
                password: data.get("password")
            })
        });

        state.token = result.accessToken;
        state.profile = {
            customerId: result.customerId,
            fullName: result.fullName,
            email: result.email,
            bannedUntilUtc: result.bannedUntilUtc
        };
        state.roles = getRoles(result.accessToken);
        persistSession();
        form.reset();
        await showDashboard();
        showToast("Signed in successfully.");
    } catch (error) {
        showToast(error.message, "error");
    }
});

elements.createOrderForm.addEventListener("submit", async event => {
    event.preventDefault();

    try {
        const order = await apiRequest("/orders", {
            method: "POST",
            body: JSON.stringify({ items: readOrderItems() })
        });

        elements.orderItems.replaceChildren();
        addOrderItem();
        showToast(`Order ${order.orderNumber} was created.`);
        state.orderView = "mine";
        elements.myOrdersTab.click();
    } catch (error) {
        showToast(error.message, "error");
    }
});

if (restoreSession()) {
    showDashboard().catch(error => showToast(error.message, "error"));
} else {
    showAuthView();
}
