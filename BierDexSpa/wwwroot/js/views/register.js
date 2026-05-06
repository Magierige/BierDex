import AbstractView from "../abstractView.js";
import { register } from "../api/authApi.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Registreren");
    }

    async getHtml() {
        const response = await fetch("/pages/register.html");
        const html = await response.text();
        return html;
    }

    async afterRenderer() {
        const form = document.getElementById("register-form");
        const errorEl = document.getElementById("register-error");

        if (!form) return;

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (errorEl) errorEl.textContent = "";

            const email = form.elements["email"].value;
            const password = form.elements["password"].value;

            try {
                await register(email, password);

                window.dispatchEvent(
                    new CustomEvent("spa-navigation", { detail: "/home" }) // Navigate to home page after successful authentication
                );
            } catch (err) {
                if (errorEl) {
                    errorEl.textContent = err.message || "Registreren is niet gelukt.";
                } else {
                    alert(err.message || "Registreren is niet gelukt.");
                }
            }
        });
    }
}