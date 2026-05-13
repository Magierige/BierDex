import AbstractView from "../abstractView.js";
import { getAllBeers, getMyBeers, getRandomBeerRating, updateBeer, deleteBeer, createBeer } from "../api/beerApi.js";
import { isAdmin } from "../api/authApi.js";

export default class extends AbstractView {
    constructor() {
        super();
        this.setTitle("Manage Beers");
        this.beerData = [
        ];
    }

    async getHtml() {
        try {
            const response = await fetch("/pages/manageBeers.html");
            return await response.text();
        } catch (error) {
            return "<h1>Fout bij laden</h1>";
        }
    }

    afterRenderer() {
        this.init();
    }

    async init() {
        if (await isAdmin()) {
            this.beerData = await getAllBeers();
        } else {
            this.beerData = await getMyBeers();
        }
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
            clone.querySelector('.beer-img').src = 'https://localhost:7228/' + beer.imagePath;
            clone.querySelector('.edit-beer-btn').addEventListener('click', () => {
                this.openEditModal(beer);
            });
            grid.appendChild(clone);
        });
    }

    openEditModal(beer) {
        document.getElementById("editBeerId").value = beer.id;
        document.getElementById("editBeerBarcode").value = beer.barcode; 
        document.getElementById("editName").value = beer.name;
        document.getElementById("editType").value = beer.type;
        document.getElementById("editAbv").value = beer.abv.replace("%", "");
        document.getElementById("editModal").classList.remove("hidden");
    }

    setupEventListeners() {
        // Search Modal Elements
        const openBtn = document.getElementById("openSearchBtn");
        const closeBtn = document.getElementById("closeModalBtn");
        const overlay = document.getElementById("modalOverlay");
        const searchBtn = document.getElementById("searchBtn");
        const modal = document.getElementById("searchModal");

        // Edit Modal Elements
        const editModal = document.getElementById("editModal");
        const closeEdit = document.getElementById("closeEditModal");
        const editOverlay = document.getElementById("editModalOverlay");
        const deleteBtn = document.getElementById("deleteBeerBtn");
        const editForm = document.getElementById("editBeerForm");

        // Add Modal Elements
        const addModal = document.getElementById("addBeerModal");
        const openAddBtn = document.getElementById("openAddModalBtn");
        const closeAddBtn = document.getElementById("closeAddModal");
        const addOverlay = document.getElementById("addModalOverlay");
        const addForm = document.getElementById("addBeerForm");

        // Open and close listeners
        [closeEdit, editOverlay].forEach(el => {
            el?.addEventListener("click", () => editModal.classList.add("hidden"));
        });
        openAddBtn?.addEventListener("click", () => addModal.classList.remove("hidden"));
        [closeAddBtn, addOverlay].forEach(el => {
            el?.addEventListener("click", () => addModal.classList.add("hidden"));
        });

        // Handle Delete
        deleteBtn?.addEventListener("click", async () => {
            const id = document.getElementById("editBeerId").value;
            if (confirm("Weet je zeker dat je dit bier wilt verwijderen?")) {
                try {
                    const serverResponse = await deleteBeer(id);
                    if (serverResponse) {
                        this.beerData = this.beerData.filter(b => b.barcode != id);
                        this.renderBeers();
                        editModal.classList.add("hidden");
                        alert("Biertje succesvol verwijderd!");
                    }
                } catch (error) {
                    alert("Er ging iets mis bij het verwijderen: " + error.message);
                }
            }
        });

        // Handle Save
        editForm?.addEventListener("submit", async (e) => { // Added async here
            e.preventDefault();
            console.log("Save edit button clicked, starting update process...");

            // 1. Get the values from the form
            const id = document.getElementById("editBeerId").value;
            const name = document.getElementById("editName").value;
            const type = document.getElementById("editType").value;
            const abv = document.getElementById("editAbv").value + '%';

            // 2. Find the beer in your local array to get all its current data
            const beerIndex = this.beerData.findIndex(b => b.id == id);

            if (beerIndex > -1) {
                // Create the object to send to the API
                // We spread the existing beer data and overwrite the changed fields
                const updatedFields = {
                    ...this.beerData[beerIndex],
                    name: name,
                    type: type,
                    abv: abv
                };
                console.log(updatedFields)

                try {
                    // 3. Call the API function
                    // Note: Ensure 'updatedFields' has the correct 'id' property your API expects
                    const serverResponse = await updateBeer(updatedFields);

                    if (serverResponse) {
                        // 4. Update the local state with the server's version
                        this.beerData[beerIndex] = serverResponse;

                        // 5. Refresh the UI and close modal
                        this.renderBeers();
                        editModal.classList.add("hidden");

                        alert("Biertje succesvol bijgewerkt!");
                    }
                } catch (error) {
                    alert("Er ging iets mis bij het opslaan: " + error.message);
                }
            }
        });

        addForm?.addEventListener("submit", async (e) => {
            e.preventDefault();

            // Gebruik FormData om zowel tekst als bestanden te versturen
            const formData = new FormData();
            formData.append("barcode", document.getElementById("addBarcode").value);
            formData.append("name", document.getElementById("addName").value);
            formData.append("type", document.getElementById("addType").value);
            formData.append("abv", document.getElementById("addAbv").value);

            const imageFile = document.getElementById("addImage").files[0];
            if (imageFile) {
                formData.append("image", imageFile); // 'image' moet matchen met je C# record naam
            }

            try {
                const newBeer = await createBeer(formData);

                if (newBeer) {
                    this.beerData.push(newBeer);
                    this.renderBeers();
                    addModal.classList.add("hidden");
                    addForm.reset();
                    alert("Biertje succesvol toegevoegd!");
                }
            } catch (error) {
                alert("Fout: " + error.message);
            }
        });

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