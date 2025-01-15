document.addEventListener("DOMContentLoaded", () => {
    const keypadOverlay = document.getElementById("keypadOverlay");
    const keyInput = document.getElementById("keyInput");
    let inactivityTimer = null;

    const isDebugMode = window.location.hash.toLowerCase() === "#debug";

    if (!keypadOverlay || !keyInput) {
        console.error("Keypad overlay or key input element not found.");
        return;
    }

    function resetInactivityTimer() {
        if (isDebugMode) return;
        clearTimeout(inactivityTimer);
        inactivityTimer = setTimeout(() => {
            keypadOverlay.style.display = "none";
        }, 5000);
    }

    document.querySelectorAll(".keypad-buttons button").forEach((button) => {
        button.addEventListener("click", () => {
            const value = button.getAttribute("data-value");
            const action = button.getAttribute("data-action");

            if (!isDebugMode) resetInactivityTimer();

            if (value) {
                handleInput(value);
            } else if (action === "clear") {
                handleClear();
            }
        });
    });

    async function handleInput(value) {
        const currentValue = /^[A-D]$/.test(value) ? value : keyInput.textContent;

        if (!currentValue.match(/^[A-D] - /) && /^[A-D]$/.test(value)) {
            keyInput.textContent = `${value} - `;
        } else if (currentValue.match(/^[A-D] - \d{0,1}$/) && /^\d$/.test(value)) {
            keyInput.textContent += value;

            if (keyInput.textContent.match(/^[A-D] - \d{2}$/)) {
                const slug = keyInput.textContent.replace(/\s+/g, "").replace("-", "");
                if (await validateTrophyExists(slug)) {
                    // Update the hash to trigger navigation
                    window.location.hash = `#${slug}`;
                } else {
                    showNotFoundMessage();
                }
            }
        }
    }

    function handleClear() {
        keyInput.textContent = keyInput.textContent.trim().endsWith("-")
            ? ""
            : keyInput.textContent.slice(0, -1);
    }

    async function validateTrophyExists(slug) {
        try {
            const response = await fetch(`/api/trophy/search/${slug}`);
            if (response.ok) {
                const result = await response.json();
                if (result.url) {
                    window.location.href = result.url;
                } else {
                    showNotFoundMessage();
                }
            } else {
                showNotFoundMessage();
            }
        } catch (error) {
            showNotFoundMessage();
        }
    }

    function showNotFoundMessage() {
        keyInput.textContent = "Not Found";
        setTimeout(() => {
            keyInput.textContent = "-";
        }, 1500);
    }
});
