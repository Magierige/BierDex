import { isAuthenticated } from "./api/authApi.js";
import { getUsername } from "./api/authApi.js"; 
import { logout } from "./api/authApi.js";

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
    console.log("loading nav...")
    const nav = document.getElementById("main-nav");
    if (!nav) return;
    if (await isAuthenticated()) {
        nav.innerHTML = await getNav();

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
    console.log("Auth status veranderd naar:", event.detail.isAuthenticated);
    loadNavBar(); // Herlaad de balk direct als de status verandert
});

// Eerste keer laden bij het opstarten van de pagina
loadNavBar();