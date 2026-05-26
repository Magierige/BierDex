import { isAuthenticated, isHigherUser, getUsername, logout, isAdmin } from "./api/authApi.js";

async function getGuestNav() {
    const response = await fetch("/navGuest.html");
    return await response.text();
}

async function getNav() {
    const response = await fetch("/nav.html");
    return await response.text();
}

// Hulpfunctie om het mobiele menu te toggelen met de juiste ARIA-statussen
function setupMobileMenu() {
    const btn = document.getElementById("mobile-menu-button");
    const menu = document.getElementById("mobile-menu");

    if (btn && menu) {
        // Verwijder oude listeners om dubbele events te voorkomen bij herladen
        const newBtn = btn.cloneNode(true);
        btn.parentNode.replaceChild(newBtn, btn);

        newBtn.addEventListener("click", () => {
            const isHidden = menu.classList.toggle("hidden");
            newBtn.setAttribute("aria-expanded", !isHidden);
        });
    }
}

export async function loadNavBar() {
    const nav = document.getElementById("main-nav");
    if (!nav) return;

    if (await isAuthenticated()) {
        nav.innerHTML = await getNav();

        // 1. Setup Mobile Toggle Logic
        setupMobileMenu();

        // 2. Handle Higher User Links (Desktop AND Mobile)
        const isHigher = await isHigherUser();
        const isAnAdmin = await isAdmin();
        const containers = ["nav-links", "nav-links-mobile"];

        containers.forEach(id => {
            const container = document.getElementById(id);
            if (!container) return;

            if (isHigher && !document.getElementById(`${id}-manage-beer`)) {
                const li = document.createElement("li");
                li.id = `${id}-manage-beer`;
                li.innerHTML = `<a class="hover:text-amber-600 transition-colors" href="/manage-beers">Manage Beers</a>`;
                container.appendChild(li);
            }

            if (isAnAdmin && !document.getElementById(`${id}-create-user`)) {
                const li = document.createElement("li");
                li.id = `${id}-create-user`;
                li.innerHTML = `<a class="hover:text-amber-600 transition-colors" href="/create-user">Create User</a>`;
                container.appendChild(li);
            }
        });

        // 3. Update Usernames
        const username = await getUsername();
        ["username-link", "username-link-mobile"].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.textContent = username;
        });

        // 4. Logout Listeners (Standard & Mobile)
        const handleLogout = async (e) => {
            e.preventDefault();
            try {
                await logout();
                window.dispatchEvent(new CustomEvent("auth-changed", { detail: { isAuthenticated: false } }));
            } catch (err) { console.error(err); }
        };

        document.getElementById("logout-form")?.addEventListener("submit", handleLogout);
        document.getElementById("logout-btn-mobile")?.addEventListener("click", handleLogout);

    } else {
        nav.innerHTML = await getGuestNav();
        // Re-attach the toggle logic for the Guest menu
        setupMobileMenu();
    }
}

window.addEventListener("auth-changed", () => {
    loadNavBar();
});

// Eerste keer laden
loadNavBar();