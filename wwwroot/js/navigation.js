document.addEventListener("DOMContentLoaded", () => {
    const panels = [
        document.getElementById("panel-1"),
        document.getElementById("panel-2"),
        document.getElementById("panel-3"),
    ];

    let isTransitioning = false;
    const TRANSITION_DURATION = 500; // milliseconds

    // Helper functions
    const setPanelAttributes = (panel, trophy) => {
        panel.dataset.slug = trophy.slug.toUpperCase();
        panel.dataset.previous = trophy.previous.toUpperCase();
        panel.dataset.next = trophy.next.toUpperCase();
        panel.innerHTML = trophy.html;
    };

    const transformPanel = (panel, translateX, position, display = "inherit") => {
        panel.style.transform = `translateX(${translateX}%)`;
        panel.dataset.position = position;
        panel.style.display = display;
    };

    const getTrophy = async (slug) => {
        const response = await fetch(`/trophy/details/${slug}`);
        if (response.ok) {
            return {
                slug,
                html: await response.text(),
                previous: response.headers.get("X-Previous-Slug"),
                next: response.headers.get("X-Next-Slug"),
            };
        }
        return null;
    };

    const loadTrophy = async (slug, panel) => {
        const trophy = await getTrophy(slug);
        if (trophy) {
            setPanelAttributes(panel, trophy);
        }
    };

    const handleTransition = async (newSlug) => {
        if (isTransitioning) return;

        const [leftPanel, middlePanel, rightPanel] = panels.sort(
            (a, b) => a.dataset.position - b.dataset.position
        );

        if (newSlug === middlePanel.dataset.slug) return;

        let action = "load";
        if (leftPanel.dataset.slug === newSlug) action = "moveLeft";
        if (rightPanel.dataset.slug === newSlug) action = "moveRight";

        isTransitioning = true;

        if (action === "moveLeft") {
            transformPanel(rightPanel, -100, "1", "none");
            transformPanel(middlePanel, 100, "3");
            transformPanel(leftPanel, 0, "2");

            setTimeout(async () => {
                await loadTrophy(leftPanel.dataset.previous, rightPanel);
                transformPanel(rightPanel, -100, "1");
                isTransitioning = false;
            }, TRANSITION_DURATION);
        } else if (action === "moveRight") {
            transformPanel(leftPanel, 100, "3", "none");
            transformPanel(middlePanel, -100, "1");
            transformPanel(rightPanel, 0, "2");

            setTimeout(async () => {
                await loadTrophy(rightPanel.dataset.next, leftPanel);
                transformPanel(leftPanel, 100, "3");
                isTransitioning = false;
            }, TRANSITION_DURATION);
        } else if (action === "load") {
            const trophyData = await getTrophy(newSlug);
            if (trophyData) {
                setPanelAttributes(rightPanel, trophyData);

                transformPanel(leftPanel, 100, "3", "none");
                transformPanel(middlePanel, -100, "1");
                transformPanel(rightPanel, 0, "2");

                setTimeout(async () => {
                    await Promise.all([
                        loadTrophy(trophyData.previous, middlePanel),
                        loadTrophy(trophyData.next, leftPanel),
                    ]);
                    transformPanel(leftPanel, 100, "3");
                    isTransitioning = false;
                }, TRANSITION_DURATION);
            }
        }
    };

    const setupInitialState = async (initialSlug) => {
        const [leftPanel, middlePanel, rightPanel] = panels;
        leftPanel.dataset.position = "1";
        middlePanel.dataset.position = "2";
        rightPanel.dataset.position = "3";

        const trophyData = await getTrophy(initialSlug);
        if (trophyData) {
            setPanelAttributes(middlePanel, trophyData);
            await Promise.all([
                loadTrophy(trophyData.previous, leftPanel),
                loadTrophy(trophyData.next, rightPanel),
            ]);
        }
    };

    const debounce = (func, delay) => {
        let timeout;
        return (...args) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), delay);
        };
    };

    const handleHashChange = debounce(() => {
        const newSlug = window.location.hash.substring(1) || "A01";
        handleTransition(newSlug.toUpperCase());
    }, 200);

    const moveNext = () => {
        const middlePanel = panels.find(panel => panel.dataset.position === "2");
        if (middlePanel) {
            const nextSlug = middlePanel.dataset.next;
            if (nextSlug) {
                window.location.hash = `#${nextSlug}`;
            }
        }
    };

    const movePrevious = () => {
        const middlePanel = panels.find(panel => panel.dataset.position === "2");
        if (middlePanel) {
            const previousSlug = middlePanel.dataset.previous;
            if (previousSlug) {
                window.location.hash = `#${previousSlug}`;
            }
        }
    };

    // Initialise
    const initialSlug = window.location.hash.substring(1) || "A01";
    setupInitialState(initialSlug);
    window.addEventListener("hashchange", handleHashChange);

    // Expose moveNext for external use
    window.moveNext = moveNext;
    window.movePrevious = movePrevious;
});
