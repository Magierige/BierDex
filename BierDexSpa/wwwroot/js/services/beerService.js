// src/services/beerService.js
export const BeerService = {
    // Zoek een bier in een array op basis van barcode
    findBeerByBarcode(beers, barcode) {
        return beers.find(b => b.barcode == barcode.trim());
    },

    // Formatteer de afbeeldings-URL (centrale plek voor je localhost URL)
    getImageUrl(path) {
        if (!path) return '/assets/placeholder-beer.png';
        const baseUrl = 'https://localhost:7228';
        return path.startsWith('/') ? `${baseUrl}${path}` : `${baseUrl}/${path}`;
    },

    // Filter bieren op status
    filterByStatus(beers, showOnlyUnapproved) {
        return showOnlyUnapproved
            ? beers.filter(beer => beer.approved === false)
            : beers;
    }
};