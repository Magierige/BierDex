export async function getAllReviews() {
    try {
        const response = await fetch("/api/review/all");
        if (!response.ok) throw new Error("Could not fetch reviews");

        const reviews = await response.json();
        return reviews;
    } catch (error) {
        console.error("Error:", error);
        throw error;
    }
}

export async function createReview(formData) {
    try {
        const response = await fetch("/api/review/create", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(formData)
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

export async function getReviewByBeerId(beerId) {
    try {
        const response = await fetch(`/api/review/beer/${beerId}`);
        if (!response.ok) throw new Error("Could not fetch reviews for beer");
        
        const review = await response.json();
        return review;
    } catch (error) {
        console.error("API Error:", error);
        throw error;
    }
}