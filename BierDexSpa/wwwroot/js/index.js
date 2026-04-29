import Home from './views/home.js';

const navigateTo = url => {
    history.pushState(null, null, url); // place each route in the history list
    router();
};

function matchRoute(route, pathname) {
    const routeParts = route.path.split("/").filter(Boolean);
    const pathParts = pathname.split("/").filter(Boolean);

    if (routeParts.length !== pathParts.length) return null;

    const params = {};

    for (let i = 0; i < routeParts.length; i++) {
        if (routeParts[i].startsWith(":")) {
            const paramName = routeParts[i].slice(1);
            params[paramName] = pathParts[i];
        } else if (routeParts[i] !== pathParts[i]) {
            return null;
        }
    }

    return params; // success! return object met param values
}

const routes = [
    { path: "/", view: Home },
    { path: "/home", view: Home }
];

const router = async () => {
    const currentPath =
        location.pathname === "/index.html" ? "/" : location.pathname;

    let match = null;
    let params = {};

    for (const route of routes) {
        const result = matchRoute(route, currentPath);
        if (result) {
            match = route;
            params = result; // e.g. { sku: "ABC123" }
            break;
        }
    }

    if (!match) {
        // fallback to home
        match = routes[0];
        params = {};
    }

    // Frontend route guard
    if (match.requiresAuth) {
        const loggedIn = await isAuthenticated();
        if (!loggedIn) {
            // redirect to /login when user is not authenticated
            navigateTo("/login");
            return; // exit router to prevent further execution
        }
    }

    const view = new match.view(params);
    document.querySelector("#app").innerHTML = await view.getHtml();

    if (typeof view.afterRenderer === "function") {
        view.afterRenderer(); // run if the view has an afterRenderer function
    }
};

document.addEventListener("DOMContentLoaded", () => {
    // listen to clicks in the body
    document.body.addEventListener("click", e => {
        // const link = e.target.closest("[data-link]"); will not work in web component!
        const path = e.composedPath();
        const link = path.find(
            el =>
                el instanceof HTMLElement &&
                el.matches &&
                el.matches("[data-link]")
        );
        if (!link) return; // if it is not a datalink then do nothing
        e.preventDefault(); // else do not refresh the page (preventDefault),
        navigateTo(link.href); // but call our navigateTo function to load the right view
    });

    router();
});

window.addEventListener("popstate", router); // browsing the history list.

window.addEventListener("spa-navigation", e => {
    history.pushState(null, null, e.detail); // e.detail contains href from nav-bar
    router();
});