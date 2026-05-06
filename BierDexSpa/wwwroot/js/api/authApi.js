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
    console.log("Checking authentication status...");
    const response = await fetch("/api/auth/status", {
        method: "GET",
        credentials: "include"
    });
    console.log("Authentication status response:", response);
    return response.status === 200;
}

export async function getUsername() {
    const response = await fetch("/api/auth/status", {
        method: "GET",
        credentials: "include"
    });

    if (!response.ok) {
        return null;
    }

    const data = await response.json();
    return data.name;
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