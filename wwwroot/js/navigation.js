document.addEventListener("DOMContentLoaded", () => {
    const contentWrapper = document.getElementById("content-wrapper");

    async function loadTrophy(slug) {
        if (!contentWrapper) {
            console.error("Content wrapper not found.");
            return;
        }

        try {
            // Start slide-out animation
            contentWrapper.classList.add("slide-out");

            // Wait for animation to finish
            await new Promise((resolve) => setTimeout(resolve, 300));

            // Fetch the trophy details
            const response = await fetch(`/api/trophy/details/${slug}`);
            if (response.ok) {
                const html = await response.text();

                // Replace content and slide-in
                contentWrapper.innerHTML = html;
                contentWrapper.classList.remove("slide-out");
                contentWrapper.classList.add("slide-in");

                // Reset slide-in class after animation
                setTimeout(() => contentWrapper.classList.remove("slide-in"), 300);
            } else {
                showNotFoundMessage();
            }
        } catch (error) {
            console.error("Failed to load trophy details:", error);
            showNotFoundMessage();
        }
    }

    function showNotFoundMessage() {
        contentWrapper.innerHTML = "<p>Trophy not found. Please try again.</p>";
    }

    // Listen for hash changes
    window.addEventListener("hashchange", () => {
        const slug = window.location.hash.substring(1); // Remove '#'
        if (slug) {
            loadTrophy(slug);
        }
    });

    // Load trophy if hash exists on page load
    const initialHash = window.location.hash.substring(1);
    if (initialHash) {
        loadTrophy(initialHash);
    }
});
