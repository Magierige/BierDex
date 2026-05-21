import AbstractView from "../abstractView.js";
import { getSingleBeer, getRandomBeerRating } from "../api/beerApi.js";
import { BeerService } from "../services/beerService.js";
import { getReviewByBeerId, createReview, deleteReview } from "../api/reviewApi.js";
import { isAdmin, getUserId } from "../api/authApi.js";

export default class extends AbstractView {
    constructor(params) {
        super();
        this.setTitle = "bier details"
        this.sku = params.sku;
        this.currentBeerId = null;
        this.isAdmin = false;
        this.userId = null;
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
        this.isAdmin = await isAdmin();
        this.userId = await getUserId();
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
        container.innerHTML = reviews.map(review => {
            const canDelete = this.isAdmin || this.userId === review.user.id;

            return `
        <div class="bg-white border border-gray-100 p-6 rounded-3xl shadow-sm hover:shadow-md transition-shadow relative">
            <div class="flex justify-between items-start mb-4">
                <div>
                    <p class="font-black text-gray-900">${review.user?.userName || 'unknown'}</p>
                </div>
                <div class="flex items-center gap-2">
                    <div class="bg-amber-50 text-amber-600 px-3 py-1 rounded-full text-sm font-bold">
                        ★ ${review.rating}
                    </div>
                    
                    ${canDelete ? `
                        <button 
                            data-review-id="${review.id}" 
                            class="delete-review-btn text-gray-300 hover:text-red-600 transition-colors p-1"
                        >
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 pointer-events-none" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                            </svg>
                        </button>
                    ` : ''}
                </div>
            </div>
            <p class="text-gray-600 italic leading-relaxed">"${review.content}"</p>
        </div>
    `}).join('');

        // --- ENERGIEKE EVENT LISTENERS TOEVOEGEN ---
        // We zoeken alle verwijder-knoppen op die we net in de HTML hebben gezet
        const deleteButtons = container.querySelectorAll('.delete-review-btn');
        deleteButtons.forEach(button => {
            button.addEventListener('click', (e) => this.handleReviewDelete(e));
        });
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

        const errorEl = document.getElementById('create-review-error');
        if (errorEl) {
            errorEl.textContent = "";
        }

        try {
            const success = await createReview(formData);
            if (success) {
                e.target.reset();
                await this.loadReviews();
                alert("Bedankt voor je review!");
            }
        } catch (error) {
            let errorMessage = error.message;

            // --- DE ULTIEME JSON CRUSHER ---
            // Als de error-tekst (of de JSON-brij) begint met een accolade, slopen we hem hier live uit elkaar!
            if (errorMessage && errorMessage.trim().startsWith("{")) {
                try {
                    const parsedJson = JSON.parse(errorMessage);
                    if (parsedJson.errors) {
                        // Pak alle meldingen samen en zet ze onder elkaar
                        errorMessage = Object.values(parsedJson.errors).flat().join("\n");
                    } else if (parsedJson.title) {
                        errorMessage = parsedJson.title;
                    }
                } catch (p) {
                    // Mocht het parsen mislukken, behouden we de originele tekst
                }
            }

            // Als de boodschap na het filteren nog steeds leeg is, tonen we een standaardtekst
            if (!errorMessage || errorMessage.trim() === "") {
                errorMessage = "Er is iets misgegaan bij het plaatsen van de review.";
            }

            if (errorEl) {
                errorEl.textContent = errorMessage;
                errorEl.style.color = "#dc3545"; // Rood
            } else {
                alert(errorMessage);
            }
        } finally {
            submitBtn.disabled = false;
            submitBtn.textContent = "REVIEW PLAATSEN";
        }
    }

    async handleReviewDelete(e) {
        // Haal het ID van de review op uit het data-attribuut van de knop
        const reviewId = e.target.getAttribute('data-review-id');

        // Vraag om een bevestiging, wel zo netjes voor de gebruiker
        if (!confirm("Weet je zeker dat je deze review wilt verwijderen?")) {
            return;
        }

        try {
            // Roep de API aan om te deleten
            const success = await deleteReview(reviewId);

            if (success) {
                // Herlaad de reviews zodat de verwijderde review meteen verdwijnt
                await this.loadReviews();
            } else {
                alert("Kon de review niet verwijderen.");
            }
        } catch (error) {
            console.error("Fout bij verwijderen review:", error);
            alert("Er is iets misgegaan bij het verwijderen: " + error.message);
        }
    }
}