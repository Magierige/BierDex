export async function getAllBeers() {
    try {
        const response = await fetch("/api/beer/all");
        if (!response.ok) throw new Error("Could not fetch beers");

        const beers = await response.json();
        return beers;
    } catch (error) {
        console.error("Error:", error);
    }
}

export function getRandomBeerRating() {
    // Math.random() * 9 gives 0 to 9.0
    // + 1 shifts the range to 1.0 to 10.0
    const num = (Math.random() * 9) + 1;
    return parseFloat(num.toFixed(1));
}