import AbstractView from "../abstractView.js";
import { createUser } from "../api/authApi.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Gebruiker aanmaken");
    }

    async getHtml() {
        const response = await fetch("/pages/adminCreateUser.html");
        const html = await response.text();
        return html;
    }

    async afterRenderer() {
        const modal = document.getElementById('createUserModal');
        const openBtn = document.getElementById('openCreateUserBtn');
        const closeBtn = document.getElementById('closeModalBtn');
        const overlay = document.getElementById('modalOverlay');
        const createUserForm = document.getElementById('create-user-form');

        // --- Modal Logic ---
        const toggleModal = () => {
            modal.classList.toggle('hidden');
            if (!modal.classList.contains('hidden')) {
                // Focus first input when opening
                createUserForm.querySelector('input').focus();
            }
        };

        if (openBtn) openBtn.addEventListener('click', toggleModal);
        if (closeBtn) closeBtn.addEventListener('click', toggleModal);
        if (overlay) overlay.addEventListener('click', toggleModal);

        // Close on Escape key
        const handleKeyDown = (e) => {
            if (e.key === 'Escape' && !modal.classList.contains('hidden')) {
                toggleModal();
            }
        };
        window.addEventListener('keydown', handleKeyDown);

        const form = document.getElementById("create-user-form");
        const errorEl = document.getElementById("create-user-error");
        const successEl = document.getElementById("create-user-success"); 

        if (!form) return;

        form.addEventListener("submit", async (e) => {
            e.preventDefault();
            if (errorEl) errorEl.textContent = "";
            if (successEl) successEl.textContent = "";

            const submitBtn = form.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerText;

            const email = form.elements["email"].value;
            const username = form.elements["username"].value;
            const role = form.elements["role"].value;

            try {
                submitBtn.disabled = true;
                submitBtn.innerText = "Bezig...";

                await createUser(email, username, role);

                // Toon succesmelding
                if (successEl) {
                    successEl.textContent = "Gebruiker succesvol aangemaakt! De activatiemail is verzonden.";
                    successEl.style.color = "#28a745"; // Groen
                }

                form.reset();

                // Wacht 2 seconden zodat de admin de melding kan lezen, dan pas navigeren of sluiten
                setTimeout(() => {
                    // Optioneel: sluit de modal
                    const modal = document.getElementById('createUserModal');
                    if (modal && !modal.classList.contains('hidden')) {
                        modal.classList.add('hidden');
                    }

                    // Navigeer of ververs de pagina
                    window.dispatchEvent(
                        new CustomEvent("spa-navigation", { detail: "/admin/users" }) // Pas dit aan naar je user-overzicht
                    );
                }, 2000);

            } catch (err) {
                submitBtn.disabled = false;
                submitBtn.innerText = originalText;

                if (errorEl) {
                    errorEl.textContent = err.message || "Gebruiker aanmaken is niet gelukt.";
                    errorEl.style.color = "#dc3545"; // Rood
                }
            }
        });
    }
}