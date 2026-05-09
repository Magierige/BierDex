import AbstractView from "../abstractView.js";
import { getUsername } from "../api/authApi.js";
import { changeUsername } from "../api/authApi.js";


export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Profiel");
    }

    async getHtml() {
        const response = await fetch("/pages/manageProfile.html");
        const html = await response.text();
        return html;
    }

    async afterRenderer() {
        const form = document.getElementById("profile-form");
        const errorEl = document.getElementById("profile-error");

        if (!form) return;

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (errorEl) errorEl.textContent = "";

            const newUsername = form.elements["Username"].value;

            try {
                await changeUsername(newUsername);

                window.dispatchEvent(
                    new CustomEvent("spa-navigation", { detail: "/home" }) // Navigate to home page after successful username change
                );
            } catch (err) {
                if (errorEl) {
                    errorEl.textContent = err.message || "Gebruikersnaam wijzigen is niet gelukt.";
                } else {
                    alert(err.message || "Gebruikersnaam wijzigen is niet gelukt.");
                }
            }
        });
    }
}