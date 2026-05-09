import { isAuthenticated, isHigherUser, getUsername, logout } from "./api/authApi.js";

async function getGuestNav() {
    const response = await fetch("/navGuest.html");
    const html = await response.text();
    return html;
}

async function getNav() {
    const response = await fetch("/nav.html");
    const html = await response.text();
    return html;
}

export async function loadNavBar() {
    const nav = document.getElementById("main-nav");
    if (!nav) return;

    if (await isAuthenticated()) {
        nav.innerHTML = await getNav();

        // 1. Setup Mobile Toggle Logic
        const btn = document.getElementById("mobile-menu-button");
        const menu = document.getElementById("mobile-menu");
        if (btn && menu) {
            btn.onclick = () => menu.classList.toggle("hidden");
        }

        // 2. Handle Higher User Links (Desktop AND Mobile)
        if (await isHigherUser()) {
            const containers = ["nav-links", "nav-links-mobile"];
            
            containers.forEach(id => {
                const container = document.getElementById(id);
                if (container && !document.getElementById(`${id}-manage-beer`)) {
                    const li = document.createElement("li");
                    li.id = `${id}-manage-beer`;
                    li.innerHTML = `<a class="hover:text-amber-600 transition-colors" href="/manage-beers">Manage Beers</a>`;
                    container.appendChild(li);
                }
            });
        }

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
        const guestContent = await getGuestNav();
        nav.innerHTML = guestContent;

        // Re-attach the toggle logic for the Guest menu
        const btn = document.getElementById("mobile-menu-button");
        const menu = document.getElementById("mobile-menu");

        if (btn && menu) {
            btn.addEventListener("click", () => {
                menu.classList.toggle("hidden");
            });
        }
    }
}

window.addEventListener("auth-changed", (event) => {
    loadNavBar(); // Herlaad de balk direct als de status verandert
});

// Eerste keer laden bij het opstarten van de pagina
loadNavBar();