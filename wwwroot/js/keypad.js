document.addEventListener("DOMContentLoaded", () => {
    const keypadOverlay = document.getElementById("keypadOverlay");
    const keyInput = document.getElementById("keyInput");
    let inactivityTimer = null;

    // Check if #Debug is in the URL (case-insensitive)
    const isDebugMode = window.location.hash.toLowerCase() === "#debug";

    if (!keypadOverlay || !keyInput) {
        console.error("Keypad overlay or key input element not found.");
        return;
    }

    // Function to show the keypad
    function showKeypad() {
        keypadOverlay.style.display = "flex";
        setTimeout(() => {
            keypadOverlay.classList.add("visible");
        }, 10);
        if (!isDebugMode) {
            resetInactivityTimer();
        }
        console.log("Keypad displayed with fade-in");
    }

    // Function to hide the keypad
    function hideKeypad() {
        keypadOverlay.classList.remove("visible");
        setTimeout(() => {
            keypadOverlay.style.display = "none"; // Hide after fade-out completes
            console.log("Keypad hidden with fade-out");
        }, 300); // Match the transition duration (300ms)
    }

    // Function to reset the inactivity timer
    function resetInactivityTimer() {
        if (isDebugMode) {
            console.log("Debug mode enabled: inactivity timer disabled.");
            return;
        }
        clearTimeout(inactivityTimer);
        inactivityTimer = setTimeout(() => {
            hideKeypad(); // Hide the keypad after 5 seconds of inactivity
            console.log("Keypad hidden due to inactivity");
        }, 5000);
    }

    // Handle clicks on the body
    document.body.addEventListener("click", (event) => {
        if (!event.target.closest(".keypad")) {
            // If clicking outside the keypad, toggle it
            if (keypadOverlay.classList.contains("visible")) {
                hideKeypad();
            } else {
                keypadOverlay.style.display = "flex";
                setTimeout(() => showKeypad(), 10);
            }
        }
    });

    // Handle button clicks on the keypad
    document.querySelectorAll(".keypad-buttons button").forEach((button) => {
        button.addEventListener("click", () => {
            const value = button.getAttribute("data-value");
            const action = button.getAttribute("data-action");

            if (!isDebugMode) {
                resetInactivityTimer(); // Reset inactivity timer on button press
            }

            if (value) {
                handleInput(value);
            } else if (action === "clear") {
                handleClear();
            }
        });
    });

    // Handle user input
    function handleInput(value) {
        let currentValue = (/^[A-D]$/.test(value)) ? value : keyInput.textContent;

        if (!currentValue.match(/^[A-D] - /) && /^[A-D]$/.test(value)) {
            keyInput.textContent = `${value} - `;
        } else if (currentValue.match(/^[A-D] - \d{0,1}$/) && /^\d$/.test(value)) {
            keyInput.textContent += value;

            if (keyInput.textContent.match(/^[A-D] - \d{2}$/)) {
                validateTrophyCombination(keyInput.textContent);
            }
        }
    }

    // Clear the display
    function handleClear() {
        let currentValue = keyInput.textContent.trim();

        if (currentValue.endsWith("-")) {
            keyInput.textContent = ""; // Clear everything
        } else if (currentValue.match(/^[A-D] - \d$/)) {
            keyInput.textContent = currentValue.slice(0, -1); // Remove the last digit
        } else if (currentValue.match(/^[A-D] - $/)) {
            keyInput.textContent = currentValue.slice(0, -3); // Remove the letter and dash
        }
    }

    // Simulate validation (replace with actual API call)
    async function validateTrophyCombination(combination) {
        const slug = combination.replace(/\s+/g, "").replace("-", "");

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

    // Show "Not Found" message
    function showNotFoundMessage() {
        keyInput.textContent = "Not Found";
        setTimeout(() => {
            keyInput.textContent = "-";
        }, 1500);
    }
});
