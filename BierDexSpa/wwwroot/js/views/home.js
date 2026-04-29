import AbstractView from "../abstractView.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("BierDex");
        this.beerData = [
            { id: 1, barcode: "123", name: "Desperados", type: "Tequila Beer", abv: "5.9%", rating: "4.9", img: "images/Despo.png" },
            { id: 2, barcode: "456", name: "BrewDog Punk IPA", type: "IPA", abv: "5.4%", rating: "4.5", img: "images/Despo.png" },
            { id: 3, barcode: "789", name: "Hertog Jan", type: "Pilsner", abv: "5.1%", rating: "4.8", img: "images/Despo.png" },
            { id: 4, barcode: "000", name: "Guinness", type: "Stout", abv: "4.2%", rating: "4.7", img: "images/Despo.png" }
        ];
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
            clone.querySelector('.beer-rating').textContent = beer.rating;
            clone.querySelector('.beer-img').src = beer.img;
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

        const foundBeer = this.beerData.find(b => b.barcode === query);

        if (foundBeer) {
            document.getElementById("resultImg").src = foundBeer.img;
            document.getElementById("resultName").innerText = foundBeer.name;
            document.getElementById("resultType").innerText = foundBeer.type;
            resultDiv.classList.remove("hidden");
        } else {
            alert("Bier niet gevonden! Probeer '123' of '456'.");
            resultDiv.classList.add("hidden");
        }
    }
}