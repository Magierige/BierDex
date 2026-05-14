import AbstractView from "../abstractView.js";
import { getSingleBeer, getRandomBeerRating } from "../api/beerApi.js";
import { BeerService } from "../services/beerService.js";
import { getReviewByBeerId, createReview } from "../api/reviewApi.js";

export default class extends AbstractView {
    constructor(params) {
        super();
        this.setTitle = "bier details"
        this.sku = params.sku;
        this.currentBeerId = null;
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
            this.currentBeerId = beer.id;

            // 2. Vul de HTML met de data (gebruik de ID's uit de vorige stap)
            document.getElementById('beer-detail-img').src = BeerService.getImageUrl(beer.imagePath);
            document.getElementById('beer-detail-img').alt = beer.name;

            document.getElementById('beer-detail-name').textContent = beer.name;
            document.getElementById('beer-detail-type').textContent = beer.type;
            document.getElementById('beer-detail-abv').textContent = beer.abv;
            document.getElementById('beer-detail-barcode').textContent = beer.barcode

            // Rating en andere details
            if (document.getElementById('beer-detail-rating')) {
                document.getElementById('beer-detail-rating').textContent = beer.rating || getRandomBeerRating();;
            }

            const form = document.getElementById('review-form');
            if (form) {
                form.addEventListener('submit', (e) => this.handleReviewSubmit(e));
            }

            this.loadReviews();

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

    async loadReviews() {
        const reviews = await getReviewByBeerId(this.currentBeerId);
        // Normalize reviews to array if single object returned
        const reviewsArray = Array.isArray(reviews) ? reviews : (reviews ? [reviews] : []);
        this.renderReviews(reviewsArray);
    }

    renderReviews(reviews) {
        const container = document.getElementById('reviews-container');
        const emptyState = document.getElementById('no-reviews');

        if (!reviews || reviews.length === 0) {
            container.innerHTML = '';
            emptyState.classList.remove('hidden');
            return;
        }

        emptyState.classList.add('hidden');
        container.innerHTML = reviews.map(review => `
        <div class="bg-white border border-gray-100 p-6 rounded-3xl shadow-sm hover:shadow-md transition-shadow">
            <div class="flex justify-between items-start mb-4">
                <div>
                    <p class="font-black text-gray-900">${review.user?.userName || 'unknown'}</p>
                </div>
                <div class="bg-amber-50 text-amber-600 px-3 py-1 rounded-full text-sm font-bold">
                    ★ ${review.rating}
                </div>
            </div>
            <p class="text-gray-600 italic leading-relaxed">"${review.content}"</p>
        </div>
    `).join('');
    }

    async handleReviewSubmit(e) {
        e.preventDefault();

        const submitBtn = e.target.querySelector('button');
        submitBtn.disabled = true;
        submitBtn.textContent = "BEZIG...";

        const formData = {
            content: document.getElementById('review-content').value,
            rating: parseInt(document.getElementById('review-rating').value),
            beerId: this.currentBeerId
        };

        try {
            const success = await createReview(formData);
            if (success) {
                e.target.reset(); // Clear the form
                await this.loadReviews(); // Reload the list
                alert("Bedankt voor je review!");
            }
        } catch (error) {
            alert("Er is iets misgegaan: " + error.message);
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = "REVIEW PLAATSEN";
        }
    }
}