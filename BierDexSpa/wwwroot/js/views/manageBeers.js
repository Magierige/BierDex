import AbstractView from "../abstractView.js";
import { getMyBeers, updateBeer, deleteBeer, createBeer, getAllBeersAdmin, approveBeer, getRandomBeerRating } from "../api/beerApi.js";
import { isAdmin } from "../api/authApi.js";
import { BeerService } from "../services/beerService.js";
import { ScannerService } from "../services/scannerService.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Manage Beers");
        this.beerData = [];
        this.userIsAdmin = false;
        this.scanner = null;
    }

    async getHtml() {
        try {
            const response = await fetch("/pages/manageBeers.html");
            return await response.text();
        } catch (error) {
            return "<h1>Fout bij laden</h1>";
        }
    }

    afterRenderer() {
        this.init();
    }

    async init() {
        this.userIsAdmin = await isAdmin();
        this.beerData = this.userIsAdmin ? await getAllBeersAdmin() : await getMyBeers();

        this.renderBeers();
        this.setupEventListeners();
        this.scanner = new ScannerService("reader");
    }

    renderBeers() {
        const grid = document.getElementById('beer-grid');
        const template = document.getElementById('beer-card-template');
        const toggle = document.getElementById('unapprovedToggle');

        if (!grid || !template) return;
        grid.innerHTML = '';

        const filteredBeers = BeerService.filterByStatus(this.beerData, toggle?.checked);

        filteredBeers.forEach(beer => {
            const clone = template.content.cloneNode(true);
            const approveBtn = clone.querySelector('.approve-beer-btn');
            const cardContainer = clone.querySelector('.group');

            if (beer.approved === false) {
                cardContainer.classList.add('border-amber-200', 'bg-amber-50/30');
                if (this.userIsAdmin) approveBtn.classList.remove('hidden');
            }

            clone.querySelector('.beer-name').textContent = beer.name;
            clone.querySelector('.beer-type').textContent = beer.type;
            clone.querySelector('.beer-abv').textContent = beer.abv;
            clone.querySelector('.beer-img').src = BeerService.getImageUrl(beer.imagePath);
            clone.querySelector('.beer-rating').textContent = beer.rating || getRandomBeerRating();

            const detailLink = clone.querySelector('.beer-link');
            if (detailLink) {
                detailLink.setAttribute('href', `/beer/${beer.slug}`);
            }

            clone.querySelector('.edit-beer-btn').addEventListener('click', () => this.openEditModal(beer));

            approveBtn.addEventListener('click', async () => {
                try {
                    if (await approveBeer(beer)) {
                        const index = this.beerData.findIndex(b => b.id === beer.id);
                        this.beerData[index].approved = true;
                        this.renderBeers();
                    }
                } catch (error) {
                    alert("Fout bij goedkeuren: " + error.message);
                }
            });

            grid.appendChild(clone);
        });

        if (filteredBeers.length === 0) {
            grid.innerHTML = '<p class="col-span-full text-center py-10 text-gray-400 italic">Geen biertjes gevonden.</p>';
        }
    }

    setupEventListeners() {
        // Toggle Filter
        document.getElementById('unapprovedToggle')?.addEventListener('change', () => this.renderBeers());

        // Modals openen/sluiten
        this.bindModalEvents("searchModal", "openSearchBtn", ["closeModalBtn", "modalOverlay"]);
        this.bindModalEvents("editModal", null, ["closeEditModal", "editModalOverlay"]);
        this.bindModalEvents("addBeerModal", "openAddModalBtn", ["closeAddModal", "addModalOverlay"]);

        // Delete actie
        document.getElementById("deleteBeerBtn")?.addEventListener("click", async () => {
            const id = document.getElementById("editBeerId").value;
            if (confirm("Weet je zeker dat je dit bier wilt verwijderen?")) {
                if (await deleteBeer(id)) {
                    this.beerData = this.beerData.filter(b => b.id != id);
                    this.renderBeers();
                    document.getElementById("editModal").classList.add("hidden");
                }
            }
        });

        // Forms
        document.getElementById("editBeerForm")?.addEventListener("submit", (e) => this.handleEditSubmit(e));
        document.getElementById("addBeerForm")?.addEventListener("submit", (e) => this.handleAddSubmit(e));
        document.getElementById("searchBtn")?.addEventListener("click", () => this.handleSearch());

        // Scanner
        document.getElementById("startScanBtn")?.addEventListener("click", () => {
            document.getElementById("reader").classList.remove("hidden");
            this.scanner.start((code) => {
                document.getElementById("barcodeInput").value = code;
                this.handleSearch();
            });
        });
    }

    // Helper voor modals om code duplicatie te voorkomen
    bindModalEvents(modalId, openBtnId, closeSelectors) {
        const modal = document.getElementById(modalId);
        if (openBtnId) {
            document.getElementById(openBtnId)?.addEventListener("click", () => modal.classList.remove("hidden"));
        }
        closeSelectors.forEach(sel => {
            document.getElementById(sel)?.addEventListener("click", () => {
                modal.classList.add("hidden");
                this.scanner?.stop();
            });
        });
    }

    handleSearch() {
        const query = document.getElementById("barcodeInput").value;
        const foundBeer = BeerService.findBeerByBarcode(this.beerData, query);
        const resultDiv = document.getElementById("searchResult");

        if (foundBeer) {
            document.getElementById("resultImg").src = BeerService.getImageUrl(foundBeer.imagePath);
            document.getElementById("resultName").innerText = foundBeer.name;
            document.getElementById("resultType").innerText = foundBeer.type;
            resultDiv.classList.remove("hidden");

            resultDiv.style.cursor = "pointer";
            resultDiv.onclick = () => {
                // Als je een router functie hebt zoals navigateTo('/...') gebruik die, 
                // anders werkt window.location ook:
                window.location.href = `/beer/${foundBeer.slug}`;
            };
        } else {
            alert("Bier niet gevonden!");
            resultDiv.classList.add("hidden");
        }
    }

    async handleEditSubmit(e) {
        e.preventDefault();

        const submitBtn = e.target.querySelector('button[type="submit"]');
        const errorEl = document.getElementById("edit-beer-error");

        // Reset de foutmelding en zet de knop op bezig
        if (errorEl) {
            errorEl.textContent = "";
        }
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = "BEZIG...";
        }

        const id = document.getElementById("editBeerId").value;
        const beerIndex = this.beerData.findIndex(b => b.id == id);

        // Mocht het bier om een of andere reden niet gevonden worden in de lokale array
        if (beerIndex === -1) {
            if (errorEl) errorEl.textContent = "Bier niet gevonden in lokale data.";
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = "OPSLAAN";
            }
            return;
        }

        const updatedFields = {
            ...this.beerData[beerIndex],
            name: document.getElementById("editName").value,
            type: document.getElementById("editType").value,
            abv: document.getElementById("editAbv").value + '%'
        };

        try {
            const serverResponse = await updateBeer(updatedFields);

            if (serverResponse) {
                this.beerData[beerIndex] = serverResponse;
                this.renderBeers();
                document.getElementById("editModal").classList.add("hidden");
            }
        } catch (error) {
            let errorMessage = error.message;

            // --- DE JSON CRUSHER ---
            // Mocht C# een validatiefout (400 Bad Request) terugsturen, slopen we de JSON hier uit elkaar
            if (errorMessage && errorMessage.trim().startsWith("{")) {
                try {
                    const parsedJson = JSON.parse(errorMessage);
                    if (parsedJson.errors) {
                        errorMessage = Object.values(parsedJson.errors).flat().join("\n");
                    } else if (parsedJson.title) {
                        errorMessage = parsedJson.title;
                    }
                } catch (p) {
                    // Fallback naar originele message als parsen mislukt
                }
            }

            // Standaard melding als er niets overblijft
            if (!errorMessage || errorMessage.trim() === "") {
                errorMessage = "Er is iets misgegaan bij het bijwerken van het bier.";
            }

            // Toon de fout in de modal
            if (errorEl) {
                errorEl.textContent = errorMessage;
                errorEl.style.color = "#dc3545"; // Rood
                errorEl.style.marginTop = "1rem";
                errorEl.style.fontWeight = "bold";
            } else {
                alert(errorMessage);
            }
        } finally {
            // Zet de knop altijd weer netjes terug
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = "OPSLAAN";
            }
        }
    }

    async handleAddSubmit(e) {
        e.preventDefault();

        const submitBtn = e.target.querySelector('button[type="submit"]');
        const errorEl = document.getElementById("add-beer-error");

        // 1. Reset eventuele oude fouten en zet de knop op 'bezig'
        if (errorEl) {
            errorEl.textContent = "";
        }
        if (submitBtn) {
            submitBtn.disabled = true;
            submitBtn.textContent = "BEZIG...";
        }

        const formData = new FormData(e.target);
        const rawAbv = formData.get("abv");

        // 2. Formatteer ABV zodat deze voldoet aan de C# Regex (moet eindigen met %)
        if (rawAbv) {
            const formattedAbv = `${parseFloat(rawAbv)}%`;
            formData.set("abv", formattedAbv);
        }

        try {
            // Verstuur de FormData (inclusief het bestand) naar je API
            const newBeer = await createBeer(formData);

            if (newBeer) {
                this.beerData.push(newBeer);
                this.renderBeers();
                document.getElementById("addBeerModal").classList.add("hidden");
                e.target.reset(); // Maak het formulier en de geselecteerde file leeg
            }
        } catch (error) {
            let errorMessage = error.message;

            // --- DE JSON CRUSHER ---
            // Als de backend de invoer weigert (bijv. barcode te kort of geen afbeelding), 
            // slopen we de JSON hier live uit elkaar.
            if (errorMessage && errorMessage.trim().startsWith("{")) {
                try {
                    const parsedJson = JSON.parse(errorMessage);
                    if (parsedJson.errors) {
                        errorMessage = Object.values(parsedJson.errors).flat().join("\n");
                    } else if (parsedJson.title) {
                        errorMessage = parsedJson.title;
                    }
                } catch (p) {
                    // Fallback naar de originele tekst als parsen mislukt
                }
            }

            // Standaard tekst als de message leeg is gebleven
            if (!errorMessage || errorMessage.trim() === "") {
                errorMessage = "Er is iets misgegaan bij het toevoegen van het bier.";
            }

            // Toon de foutmelding netjes onderaan het formulier
            if (errorEl) {
                errorEl.textContent = errorMessage;
                errorEl.style.color = "#dc3545"; // Rood
                errorEl.style.marginTop = "1rem";
                errorEl.style.fontWeight = "bold";
            } else {
                alert(errorMessage);
            }
        } finally {
            // 3. Zet de knop ALTIJD weer terug in de originele staat
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.textContent = "BIER OPSLAAN";
            }
        }
    }

    openEditModal(beer) {
        document.getElementById("editBeerId").value = beer.id;
        document.getElementById("editName").value = beer.name;
        document.getElementById("editType").value = beer.type;
        document.getElementById("editAbv").value = beer.abv.replace("%", "");
        document.getElementById("editModal").classList.remove("hidden");
    }
}