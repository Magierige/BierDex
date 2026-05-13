import AbstractView from "../abstractView.js";
import { getMyBeers, updateBeer, deleteBeer, createBeer, getAllBeersAdmin, approveBeer } from "../api/beerApi.js";
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
        } else {
            alert("Bier niet gevonden!");
            resultDiv.classList.add("hidden");
        }
    }

    async handleEditSubmit(e) {
        e.preventDefault();
        const id = document.getElementById("editBeerId").value;
        const beerIndex = this.beerData.findIndex(b => b.id == id);

        const updatedFields = {
            ...this.beerData[beerIndex],
            name: document.getElementById("editName").value,
            type: document.getElementById("editType").value,
            abv: document.getElementById("editAbv").value + '%'
        };

        const serverResponse = await updateBeer(updatedFields);
        if (serverResponse) {
            this.beerData[beerIndex] = serverResponse;
            this.renderBeers();
            document.getElementById("editModal").classList.add("hidden");
        }
    }

    async handleAddSubmit(e) {
        e.preventDefault();
        const formData = new FormData(e.target);
        console.log(formData)// Haalt automatisch alle velden op
        const newBeer = await createBeer(formData);
        if (newBeer) {
            this.beerData.push(newBeer);
            this.renderBeers();
            document.getElementById("addBeerModal").classList.add("hidden");
            e.target.reset();
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