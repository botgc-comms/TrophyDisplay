document.addEventListener("DOMContentLoaded", () => {
    const panels = [
        document.getElementById("panel-1"),
        document.getElementById("panel-2"),
        document.getElementById("panel-3"),
    ];

    let isTransitioning = false;

    // Get the trophy relating to the specified slug
    async function getTrophy(slug) {
        const response = await fetch(`/trophy/details/${slug}`);

        if (response.ok) {
            const previous = response.headers.get("X-Previous-Slug");
            const next = response.headers.get("X-Next-Slug");
            const html = await response.text();

            return {
                slug, 
                html,
                previous, 
                next
            }
        }

        return null;
    }

    // Function to load a trophy into a specific panel
    async function loadTrophy(trophy, panel) {
        panel.dataset.slug = trophy.slug.toUpperCase(); 
        panel.dataset.previous = trophy.previous.toUpperCase();
        panel.dataset.next = trophy.next.toUpperCase();

        panel.innerHTML = trophy.html; 
    }

    // Function to handle transitions
    async function handleTransition(newSlug) {
        if (isTransitioning) return;

        const leftPanel = panels.find(panel => panel.dataset.position === "1");
        const middlePanel = panels.find(panel => panel.dataset.position === "2");
        const rightPanel = panels.find(panel => panel.dataset.position === "3");

        // The requested slug is already being displaued
        if (newSlug === middlePanel.dataset.slug) {
            console.log(`Slug ${newSlug} is already visible.`);
            return; 
        }

        // Determine what action needs to be taken
        let action = "load";
        if (leftPanel.dataset.slug === newSlug) action = "moveLeft";
        if (rightPanel.dataset.slug === newSlug) action = "moveRight";

        isTransitioning = true;

        if (action === "moveLeft") {
            rightPanel.style.display = "none"
            rightPanel.style.transform = "translateX(-100%)";
            rightPanel.dataset.position = "1"

            middlePanel.style.transform = "translateX(100%)";
            middlePanel.dataset.position = "3"

            leftPanel.style.transform = "translateX(0%)";
            leftPanel.dataset.position = "2"

            setTimeout(async () => {

                const data = await getTrophy(leftPanel.dataset.previous);
                if (data) {
                    await loadTrophy(data, rightPanel);
                }

                rightPanel.style.display = "inherit"
                isTransitioning = false;
            }, 500);


        } else if (action === "moveRight") {

            leftPanel.style.display = "none"
            leftPanel.style.transform = "translateX(100%)";
            leftPanel.dataset.position = "3"

            middlePanel.style.transform = "translateX(-100%)";
            middlePanel.dataset.position = "1"

            rightPanel.style.transform = "translateX(0%)";
            rightPanel.dataset.position = "2"

            setTimeout(async () => {

                const data = await getTrophy(rightPanel.dataset.next);
                if (data) {
                    await loadTrophy(data, leftPanel);
                }

                leftPanel.style.display = "inherit"
                isTransitioning = false;
            }, 500);
        } else if (action === "load") {

            const trophyData = await getTrophy(newSlug);

            if (trophyData) {
                await loadTrophy(trophyData, rightPanel);

                leftPanel.style.display = "none"
                leftPanel.style.transform = "translateX(100%)";
                leftPanel.dataset.position = "3"

                middlePanel.style.transform = "translateX(-100%)";
                middlePanel.dataset.position = "1"

                rightPanel.style.transform = "translateX(0%)";
                rightPanel.dataset.position = "2"

                setTimeout(async () => {

                    const payload = [
                        { slug: trophyData.previous, panel: middlePanel },
                        { slug: trophyData.next, panel: leftPanel }
                    ];

                    const promises = payload.map(async (p) => {
                        if (p.panel.dataset.slug !== p.slug) {
                            const data = await getTrophy(p.slug);
                            if (data) {
                                await loadTrophy(data, p.panel);
                            }
                        }
                    });

                    await Promise.all(promises);
                    leftPanel.style.display = "inherit"
                    isTransitioning = false;
                }, 500);

            }
        }
    }

    // Initial load
    async function setupInitialState(initialSlug) {

        const leftPanel = panels.find(panel => panel.dataset.position === "1") || panels[0];
        const middlePanel = panels.find(panel => panel.dataset.position === "2") || panels[1];
        const rightPanel = panels.find(panel => panel.dataset.position === "3") || panels[2];

        leftPanel.dataset.position = "1";
        middlePanel.dataset.position = "2";
        rightPanel.dataset.position = "3";

        var trophyData = await getTrophy(initialSlug);
        if (trophyData) {

            await loadTrophy(trophyData, middlePanel);

            const payload = [
                { slug: trophyData.previous, panel: leftPanel },
                { slug: trophyData.next, panel: rightPanel }
            ];

            const promises = payload.map(async (p) => {
                if (p.panel.dataset.slug !== p.slug) {
                    const data = await getTrophy(p.slug); 
                    if (data) {
                        await loadTrophy(data, p.panel); 
                    }
                }
            });

            await Promise.all(promises);
        }
    }


    // Handle hash changes
    function handleHashChange() {
        const newSlug = window.location.hash.substring(1) || "A01";
        handleTransition(newSlug.toUpperCase());
    }

    // Initialize
    const initialSlug = window.location.hash.substring(1) || "A01";
    setupInitialState(initialSlug);

    window.addEventListener("hashchange", handleHashChange);
});
