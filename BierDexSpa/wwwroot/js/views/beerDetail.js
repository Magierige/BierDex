import AbstractView from "../abstractView.js";
import { getSingleBeer, getRandomBeerRating } from "../api/beerApi.js";
import { BeerService } from "../services/beerService.js";

export default class extends AbstractView {
    constructor(params) {
        super();
        this.setTitle = "bier details"
        this.sku = params.sku;
    }

    async getHtml() {
        try {
            const response = await fetch("/pages/beerDetail.html");
            return await response.text();
        } catch (error) {
            return "<h1>Fout bij laden</h1>";
        }
    }

    afterRenderer() {
        this.init();
    }

    async init() {
        if (!this.sku) return;

        try {
            const beerResult = await getSingleBeer(this.sku);
            const beer = beerResult[0];

            // 2. Vul de HTML met de data (gebruik de ID's uit de vorige stap)
            document.getElementById('beer-detail-img').src = BeerService.getImageUrl(beer.imagePath);
            document.getElementById('beer-detail-img').alt = beer.name;

            document.getElementById('beer-detail-name').textContent = beer.name;
            document.getElementById('beer-detail-type').textContent = beer.type;
            document.getElementById('beer-detail-abv').textContent = beer.abv;

            // Rating en andere details
            if (document.getElementById('beer-detail-rating')) {
                document.getElementById('beer-detail-rating').textContent = beer.rating || getRandomBeerRating();;
            }

        } catch (error) {
            console.error("Fout bij ophalen bier details:", error);
            document.getElementById('app').innerHTML = `
                <div class="max-w-7xl mx-auto p-6">
                    <h1 class="text-2xl font-black">Bier niet gevonden</h1>
                    <a href="/home" class="text-amber-600 underline">Terug naar overzicht</a>
                </div>
            `;
        }
    }
}