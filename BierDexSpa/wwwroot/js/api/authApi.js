let cachedAuthStatus = null; // Slaat het volledige JSON object van /api/auth/status op
let authPromise = null;

export async function login(email, password) {
    const response = await fetch("/api/auth/email-login?useCookies=true&useSessionCookies=true", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        credentials: "include", // no same-origin, because Identity usually use a 302 redirect. This may cause strange behaviour (such as a 401 when the user is actually authenticated)
        body: JSON.stringify({ email, password })
    });

    if (!response.ok) {
        let message = "Inloggen mislukt.";
        try {
            const problem = await response.json();
            if (problem && problem.title) {
                message = problem.title;
            }
        } catch {
        }
        throw new Error(message);
    }

    // Notify SPA that user is now authenticated
    window.dispatchEvent(
        new CustomEvent("auth-changed", { detail: { isAuthenticated: true } })
    );
}

export async function logout() {
    const response = await fetch("/api/auth/logout", {
        method: "POST"
    });

    // Notify SPA that user is now logged out
    window.dispatchEvent(
        new CustomEvent("auth-changed", { detail: { isAuthenticated: false } })
    );
}

export async function isAuthenticated() {
    const status = await getAuthStatus();

    if (!status || !status.isAuthenticated) {
        return false;
    }

    return status.isAuthenticated;
}

export async function getUsername() {
    const status = await getAuthStatus();

    if (!status || !status.name) {
        return false;
    }

    return status.name;
}

export async function register(email, password) {
    const response = await fetch("/api/auth/register", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, password })
    });

    if (!response.ok) {
        let message = "Registreren mislukt.";
        try {
            const problem = await response.json();
            if (problem && problem.title) {
                message = problem.title;
            }
        } catch {
        }
        throw new Error(message);
    } else {
        login(email, password);
    }

}

export async function forgotPassword(email) {
    const response = await fetch("/api/auth/forgotPassword", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ email })
    });

    if (!response.ok) {
        let message = "Wachtwoord vergeten mislukt.";
        try {
            const problem = await response.json();
            if (problem && problem.title) {
                message = problem.title;
            }
        } catch {
        }
        throw new Error(message);
    }
}

export async function resetPassword(email, token, newPassword) {
    const response = await fetch("/api/auth/resetPassword", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            email: email,
            resetCode: token,    
            newPassword: newPassword
        })
    });

    if (!response.ok) {
        let message = "Wachtwoord resetten mislukt.";
        try {
            const problem = await response.json();
            if (problem && problem.title) {
                message = problem.title;
            }
        } catch {
        }
        throw new Error(message);
    }
}

export async function changeUsername(newUsername) {
    const response = await fetch("/api/auth/change-username", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ newUsername })
    });

    if (!response.ok) {
        let message = "Gebruikersnaam wijzigen mislukt.";
        try {
            const problem = await response.json();
            if (problem && problem.title) {
                message = problem.title;
            }
        } catch {
        }
        throw new Error(message);
    }
}

async function getAuthStatus() {
    if (cachedAuthStatus) {
        return cachedAuthStatus;
    }

    if (authPromise) {
        return authPromise;
    }

    authPromise = fetch("/api/auth/status", {
        method: "GET",
        credentials: "include"
    }).then(async (response) => {
        if (response.status !== 200) {
            cachedAuthStatus = null;
            return null;
        }
        cachedAuthStatus = await response.json();
        return cachedAuthStatus;
    }).finally(() => {
        // Maak de promise leeg zodra deze klaar is (de data zit nu in cachedAuthStatus)
        authPromise = null;
    });

    return authPromise;
}

export async function isAdmin() {
    const status = await getAuthStatus();

    if (!status || !status.roles) {
        return false;
    }

    return status.roles.includes("Admin");
} 

export async function isSupplier() {
    const status = await getAuthStatus();

    if (!status || !status.roles) {
        return false;
    }

    return status.roles.includes("Supplier");
} 

export async function isHigherUser() {
    const status = await getAuthStatus();

    if (!status || !status.roles) {
        return false;
    }

    return status.roles.includes("Admin") || status.roles.includes("Supplier");
} 

export async function createUser(email, username, role) {
    const response = await fetch("/api/auth/create-user", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, username, role })
    });

    if (!response.ok) {
        let message = "Gebruiker aanmaken mislukt.";
        try {
            const problem = await response.json();
            if (problem && problem.title) {
                message = problem.title;
            }
        } catch {
        }
        throw new Error(message);
    }
}