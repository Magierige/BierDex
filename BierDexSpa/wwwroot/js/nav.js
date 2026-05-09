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

        if (await isHigherUser()) {
            const navLinks = document.getElementById("nav-links");
            // Check if the link is already there using an ID
            if (navLinks && !document.getElementById("manage-beer-nav-link")) {
                const manageBeerLi = document.createElement("li");
                manageBeerLi.id = "manage-beer-nav-link"; // Set the ID here
                manageBeerLi.innerHTML = `
            <a class="hover:text-amber-600 transition-colors" href="/manage-beers">
                Manage Beers
            </a>`;
                navLinks.appendChild(manageBeerLi);
            }
        }

        const userLink = document.getElementById("username-link");
        if (userLink) userLink.textContent = await getUsername();

        const logoutForm = document.getElementById("logout-form");
        if (logoutForm) {
            logoutForm.addEventListener("submit", async (event) => {
                event.preventDefault(); // Voorkom dat de pagina echt de form verstuurt

                try {
                    await logout(); // Roep je API functie aan
                    // Na succesvol uitloggen, verstuur je het event zodat de nav ververst
                    window.dispatchEvent(new CustomEvent("auth-changed", {
                        detail: { isAuthenticated: false }
                    }));
                } catch (error) {
                    console.error("Logout mislukt:", error);
                }
            });
        }
    } else {
        nav.innerHTML = await getGuestNav();
    }
}

window.addEventListener("auth-changed", (event) => {
    loadNavBar(); // Herlaad de balk direct als de status verandert
});

// Eerste keer laden bij het opstarten van de pagina
loadNavBar();