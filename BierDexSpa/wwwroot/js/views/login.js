import AbstractView from "../abstractView.js";
import { login } from "../api/authApi.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Inloggen");
    }

    async getHtml() {
        const response = await fetch("/pages/login.html");
        const html = await response.text();
        return html;
    }

    async afterRenderer() {
        const params = new URLSearchParams(window.location.search);
        if (params.get("resetSent") === "true") {
            const notifyEl = document.getElementById("success-notification");
            const messageEl = document.getElementById("success-message");

            if (notifyEl && messageEl) {
                messageEl.textContent = "Check je inbox! Als het e-mailadres bekend is en geverifieerd, hebben we een herstelmail gestuurd.";
                notifyEl.classList.remove("hidden");
                window.history.replaceState({}, document.title, "/login");
            }
        }

        const form = document.getElementById("login-form");
        const errorEl = document.getElementById("login-error");

        if (!form) return;

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (errorEl) errorEl.textContent = "";

            const email = form.elements["email"].value;
            const password = form.elements["password"].value;

            try {
                await login(email, password);

                window.dispatchEvent(
                    new CustomEvent("spa-navigation", { detail: "/home" }) // Navigate to home page after successful authentication
                );
            } catch (err) {
                if (errorEl) {
                    errorEl.textContent = err.message || "Inloggen is niet gelukt.";
                } else {
                    alert(err.message || "Inloggen is niet gelukt.");
                }
            }
        });
    }
}