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
                    new CustomEvent("spa-navigation", { detail: "/glasses" }) // Navigate to glasses page after successfull authentication
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