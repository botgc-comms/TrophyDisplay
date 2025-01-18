const COOKIE_NAME = "recentSlugs";
const API_URL = "/api/trophies/next"; // Update with your actual API endpoint
const DISPLAY_TIME_SECONDS = 5; // How long each trophy is displayed
const AVOID_REPEAT_MINUTES = 180; // Avoid trophies within the last 3 hours
const REFRESH_INTERVAL_MINUTES = 30; // Fetch new slugs every 30 minutes
const INTERACTION_DELAY_SECONDS = 20; // Delay after user interaction

let currentSlugIndex = 0; // Track which trophy is currently being displayed
let currentSlugs = []; // Store the current set of slugs retrieved from the server
let autoRotateTimer = null; // Timer for auto-rotation
let interactionTimeout = null; // Timer for interaction delay

// Utility to get a cookie value
const getCookie = (name) => {
    const cookies = document.cookie.split(";").reduce((acc, cookie) => {
        const [key, val] = cookie.trim().split("=");
        acc[key] = decodeURIComponent(val);
        return acc;
    }, {});
    return cookies[name] || "[]";
};

// Utility to set a cookie
const setCookie = (name, value, minutes) => {
    const expires = new Date(Date.now() + minutes * 60 * 1000).toUTCString();
    document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires}; path=/`;
};

// Update the cookie with a new batch of slugs
const updateRecentSlugsCookie = (newSlugs) => {
    const now = new Date();
    const recentData = JSON.parse(getCookie(COOKIE_NAME));

    // Remove batches older than 3 hours
    const recentFiltered = recentData.filter(
        (batch) => new Date(batch.timestamp).getTime() > now.getTime() - AVOID_REPEAT_MINUTES * 60 * 1000
    );

    // Add the new batch
    recentFiltered.push({ timestamp: now.toISOString(), slugs: newSlugs });

    // Update the cookie
    setCookie(COOKIE_NAME, JSON.stringify(recentFiltered), AVOID_REPEAT_MINUTES);
};

// Fetch the next set of slugs from the server
const fetchNextSlugs = async () => {
    const recentData = JSON.parse(getCookie(COOKIE_NAME));
    const cookiePayload = recentData.map(batch => ({
        timestamp: batch.timestamp,
        slugs: batch.slugs
    }));

    try {
        const response = await fetch(API_URL, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Cookie": `${COOKIE_NAME}=${encodeURIComponent(JSON.stringify(cookiePayload))}`
            }
        });

        if (response.ok) {
            const data = await response.json();
            currentSlugs = data; // Update the current slugs
            currentSlugIndex = 0; // Reset the index
            updateRecentSlugsCookie(data); // Update the cookie with the new slugs
        } else {
            console.error("Failed to fetch slugs from the server.");
        }
    } catch (error) {
        console.error("Error fetching slugs:", error);
    }
};

// Display the current trophy
const displayTrophy = (slug) => {
    // Set the slug as a bookmark or update the display logic here
    window.location.hash = `#${slug}`;
};

// Auto-rotate through the current slugs
const startAutoRotate = () => {
    if (autoRotateTimer) clearInterval(autoRotateTimer);

    autoRotateTimer = setInterval(() => {
        if (currentSlugs.length > 0) {
            displayTrophy(currentSlugs[currentSlugIndex]);
            currentSlugIndex = (currentSlugIndex + 1) % currentSlugs.length;
        }
    }, DISPLAY_TIME_SECONDS * 1000);
};

// Pause auto-rotation after user interaction
const delayAutoRotate = () => {
    if (autoRotateTimer) clearInterval(autoRotateTimer);
    if (interactionTimeout) clearTimeout(interactionTimeout);

    interactionTimeout = setTimeout(() => {
        startAutoRotate();
    }, INTERACTION_DELAY_SECONDS * 1000);
};

// Initialise the trophy rotation
const initialiseTrophyRotation = async () => {
    await fetchNextSlugs(); // Fetch the initial set of slugs
    startAutoRotate(); // Start auto-rotation

    // Fetch new slugs every 30 minutes
    setInterval(fetchNextSlugs, REFRESH_INTERVAL_MINUTES * 60 * 1000);

    // Add event listeners for user interaction
    document.addEventListener("click", delayAutoRotate);
    document.addEventListener("keydown", delayAutoRotate);
    document.addEventListener("mousemove", delayAutoRotate);
};

// Start the script
initialiseTrophyRotation();
