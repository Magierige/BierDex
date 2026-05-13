import AbstractView from "../abstractView.js";
import { getAllBeers } from "../api/beerApi.js";
import { getRandomBeerRating } from "../api/beerApi.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("BierDex");
        this.beerData = [];
    }

    async getHtml() {
        try {
            const response = await fetch("/pages/home.html");
            return await response.text();
        } catch (error) {
            return "<h1>Fout bij laden</h1>";
        }
    }

    afterRenderer() {
        this.init();
    }

    async init() {
        this.beerData = await getAllBeers();
        this.renderBeers();
        this.setupEventListeners();
    }

    renderBeers() {
        
        const grid = document.getElementById('beer-grid');
        const template = document.getElementById('beer-card-template');
        if (!grid || !template) return;

        grid.innerHTML = '';
        this.beerData.forEach(beer => {
            const clone = template.content.cloneNode(true);
            clone.querySelector('.beer-name').textContent = beer.name;
            clone.querySelector('.beer-type').textContent = beer.type;
            clone.querySelector('.beer-abv').textContent = beer.abv;
            clone.querySelector('.beer-rating').textContent = getRandomBeerRating();
            clone.querySelector('.beer-img').src = 'https://localhost:7228' + beer.imagePath;
            grid.appendChild(clone);
        });
    }

    setupEventListeners() {
        const openBtn = document.getElementById("openSearchBtn");
        const closeBtn = document.getElementById("closeModalBtn");
        const overlay = document.getElementById("modalOverlay");
        const searchBtn = document.getElementById("searchBtn");
        const modal = document.getElementById("searchModal");

        // Open Modal
        if (openBtn) {
            openBtn.addEventListener("click", () => {
                modal.classList.remove("hidden");
                document.getElementById("barcodeInput").focus();
            });
        }

        // Close Modal (Button or Overlay)
        const closeActions = [closeBtn, overlay];
        closeActions.forEach(el => {
            if (el) el.addEventListener("click", () => {
                modal.classList.add("hidden");
                document.getElementById("searchResult").classList.add("hidden");
                document.getElementById("barcodeInput").value = "";
            });
        });

        // Search Action
        if (searchBtn) {
            searchBtn.addEventListener("click", (e) => {
                e.preventDefault();
                this.handleSearch();
            });
        }
    }

    handleSearch() {
        const query = document.getElementById("barcodeInput").value.trim();
        const resultDiv = document.getElementById("searchResult");

        const foundBeer = this.beerData.find(b => b.barcode == query);

        if (foundBeer) {
            document.getElementById("resultImg").src = 'https://localhost:7228/' + foundBeer.imagePath;
            document.getElementById("resultName").innerText = foundBeer.name;
            document.getElementById("resultType").innerText = foundBeer.type;
            resultDiv.classList.remove("hidden");
        } else {
            alert("Bier niet gevonden! Probeer '001' of '010'.");
            resultDiv.classList.add("hidden");
        }
    }
}