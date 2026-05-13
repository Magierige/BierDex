export async function getAllBeers() {
    try {
        const response = await fetch("/api/beer/all");
        if (!response.ok) throw new Error("Could not fetch beers");

        const beers = await response.json();
        return beers;
    } catch (error) {
        console.error("Error:", error);
        throw error;
    }
}

export async function getAllBeersAdmin() {
    try {
        const response = await fetch("/api/beer/all-admin");
        if (!response.ok) throw new Error("Could not fetch beers");

        const beers = await response.json();
        return beers;
    } catch (error) {
        console.error("Error:", error);
        throw error;
    }
}

export async function getMyBeers() {
    try {
        const response = await fetch("/api/beer/my-beers");
        if (!response.ok) throw new Error("Could not fetch beers");

        const beers = await response.json();
        return beers;
    } catch (error) {
        console.error("Error:", error);
        throw error;
    }
}

export async function updateBeer(beer) {
    try {
        const response = await fetch(`/api/beer/${beer.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(beer)
        });

        if (!response.ok) throw new Error("Could not update beer");

        const updatedBeer = await response.json();
        return updatedBeer;
    } catch (error) {
        console.error("Error:", error);
        throw error
    }
}

export async function deleteBeer(beerId) {
    try {
        const response = await fetch(`/api/beer/${beerId}`, {
            method: "DELETE"
        });

        if (!response.ok) {
            const errorData = await response.text();
            throw new Error(errorData || "kon bier niet updaten");
        }

        return true;
    } catch (error) {
        console.error("Error:", error);
        throw error;
    }
}

export async function createBeer(formData) {
    try {
        const response = await fetch("/api/beer/upload-beer", {
            method: "POST",
            // Geen headers nodig, de browser zet automatisch 'multipart/form-data'
            body: formData
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || "Fout bij aanmaken bier");
        }

        return true;
    } catch (error) {
        console.error("API Error:", error);
        throw error;
    }
}

export async function approveBeer(beer) {
    try {
        const response = await fetch(`/api/beer/approve/${beer.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
        });

        if (!response.ok) throw new Error("Could not update beer");

        //const updatedBeer = await response.json();
        return true;
    } catch (error) {
        console.error("Error:", error);
        throw error
    }
}

export function getRandomBeerRating() {
    // Math.random() * 9 gives 0 to 9.0
    // + 1 shifts the range to 1.0 to 10.0
    const num = (Math.random() * 9) + 1;
    return parseFloat(num.toFixed(1));
}