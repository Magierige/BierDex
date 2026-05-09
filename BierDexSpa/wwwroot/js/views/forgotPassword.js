import AbstractView from "../abstractView.js";
import { forgotPassword } from "../api/authApi.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Wachtwoord vergeten?");
    }

    async getHtml() {
        const response = await fetch("/pages/forgotPassword.html");
        const html = await response.text();
        return html;
    }

    async afterRenderer() {
        const form = document.getElementById("forgot-password-form");
        const errorEl = document.getElementById("forgot-password-error");

        if (!form) return;

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (errorEl) errorEl.textContent = "";

            const email = form.elements["email"].value;

            try {
                await forgotPassword(email);

                window.dispatchEvent(
                    new CustomEvent("spa-navigation", { detail: "/login?resetSent=true" }) // Navigate to login page after successful password reset
                );
            } catch (err) {
                if (errorEl) {
                    errorEl.textContent = err.message || "Wachtwoord vergeten is niet gelukt.";
                } else {
                    alert(err.message || "Wachtwoord vergeten is niet gelukt.");
                }
            }
        });
    }
}