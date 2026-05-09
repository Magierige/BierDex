import AbstractView from "../abstractView.js";
import { resetPassword } from "../api/authApi.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Wachtwoord herstellen");
    }

    async getHtml() {
        const response = await fetch("/pages/resetPassword.html");
        const html = await response.text();
        return html;
    }

    async afterRenderer() {
        const form = document.getElementById("reset-password-form");
        const errorEl = document.getElementById("reset-password-error");

        if (!form) return;

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (errorEl) errorEl.textContent = "";

            const params = new URLSearchParams(window.location.search);

            const email = params.get("email");
            const token = params.get("code");
            const password = form.elements["password"].value; const confirmPassword = form.elements["confirm-password"].value;

            // --- FRONTEND VALIDATION ---
            let validationErrors = [];

            if (password !== confirmPassword) {
                validationErrors.push("Wachtwoorden komen niet overeen.");
            }
            if (password.length < 6) {
                validationErrors.push("Wachtwoord moet minimaal 6 tekens lang zijn.");
            }
            if (!/[0-9]/.test(password)) {
                validationErrors.push("Wachtwoord moet minimaal één cijfer bevatten (0-9).");
            }
            if (!/[A-Z]/.test(password)) {
                validationErrors.push("Wachtwoord moet minimaal één hoofdletter bevatten (A-Z).");
            }
            if (!/[^a-zA-Z0-9]/.test(password)) {
                validationErrors.push("Wachtwoord moet minimaal één speciaal teken bevatten (bijv. ! @ #).");
            }

            // Als er fouten zijn, toon ze en stop de submit
            if (validationErrors.length > 0) {
                const list = document.createElement("ul");
                list.className = "list-disc list-inside text-xs mt-2 text-red-500 font-bold";
                validationErrors.forEach(err => {
                    const li = document.createElement("li");
                    li.textContent = err;
                    list.appendChild(li);
                });
                errorEl.appendChild(list);
                return; // STOP HIER
            }

            try {
                await resetPassword(email, token, password);

                window.dispatchEvent(
                    new CustomEvent("spa-navigation", { detail: "/login" }) // Navigate to login page after successful password reset
                );
            } catch (err) {
                if (errorEl) {
                    errorEl.textContent = err.message || "Wachtwoord resetten is niet gelukt.";
                } else {
                    alert(err.message || "Wachtwoord resetten is niet gelukt.");
                }
            }
        });
    }
}