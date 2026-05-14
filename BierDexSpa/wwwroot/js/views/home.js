import AbstractView from "../abstractView.js";
import { getAllBeers, getRandomBeerRating } from "../api/beerApi.js";
import { BeerService } from "../services/beerService.js";
import { ScannerService } from "../services/scannerService.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("BierDex");
        this.beerData = [];
        this.scanner = null;
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
        this.scanner = new ScannerService("reader");
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
            clone.querySelector('.beer-img').src = BeerService.getImageUrl(beer.imagePath);
            clone.querySelector('.beer-rating').textContent = beer.rating || getRandomBeerRating();

            const detailLink = clone.querySelector('.beer-link');
            if (detailLink) {
                detailLink.setAttribute('href', `/beer/${beer.slug}`);
            }

            grid.appendChild(clone);
        });
    }

    setupEventListeners() {
        const modal = document.getElementById("searchModal");

        // Open/Sluiten
        document.getElementById("openSearchBtn")?.addEventListener("click", () => modal.classList.remove("hidden"));

        [document.getElementById("closeModalBtn"), document.getElementById("modalOverlay")].forEach(el => {
            el?.addEventListener("click", () => {
                modal.classList.add("hidden");
                this.scanner.stop();
            });
        });

        // Zoeken
        document.getElementById("searchBtn")?.addEventListener("click", (e) => {
            e.preventDefault();
            this.handleSearch();
        });

        // Scannen (Nieuw toegevoegd voor Home)
        document.getElementById("startScanBtn")?.addEventListener("click", () => {
            document.getElementById("reader").classList.remove("hidden");
            this.scanner.start((code) => {
                document.getElementById("barcodeInput").value = code;
                this.handleSearch();
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

            resultDiv.onclick = () => {
                // Gebruik je router om te navigeren (vaak via een 'navigateTo' functie)
                window.location.href = `/beer/${foundBeer.slug}`;
            };

            resultDiv.classList.remove("hidden");
            resultDiv.classList.add("cursor-pointer", "hover:bg-gray-100", "transition-colors");
        } else {
            alert("Bier niet gevonden!");
        }
    }
}