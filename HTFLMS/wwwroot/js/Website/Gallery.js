
(function () {
    "use strict";

    function initializeGallery() {
        const page = document.getElementById("galleryPage");

        if (!page) return;

        const grid = page.querySelector("#galleryGrid");
        const buttons = page.querySelectorAll(
            ".htf-gallery__filter"
        );

        if (!grid) return;

        const cards = Array.from(
            grid.querySelectorAll(".htf-gallery__card")
        );

        const reducedMotion = window.matchMedia(
            "(prefers-reduced-motion: reduce)"
        ).matches;

        let observer = null;

        // Scroll reveal animation
        if ("IntersectionObserver" in window && !reducedMotion) {

            observer = new IntersectionObserver(
                function (entries) {
                    entries.forEach(function (entry) {
                        if (!entry.isIntersecting) return;

                        entry.target.classList.remove(
                            "htf-gallery--pending"
                        );

                        entry.target.classList.add(
                            "htf-gallery--visible"
                        );

                        observer.unobserve(entry.target);
                    });
                },
                {
                    threshold: 0.08,
                    rootMargin: "0px 0px -30px 0px"
                }
            );

            cards.forEach(function (card) {
                card.classList.add("htf-gallery--pending");
                observer.observe(card);
            });
        }

        // Filter gallery
        function filterGallery(category) {

            buttons.forEach(function (button) {
                const active =
                    button.dataset.filter === category;

                button.classList.toggle(
                    "is-active",
                    active
                );

                button.setAttribute(
                    "aria-pressed",
                    String(active)
                );
            });

            cards.forEach(function (card) {

                const cardCategory =
                    card.dataset.category || "all";

                const shouldShow =
                    category === "all" ||
                    cardCategory === category;

                card.classList.toggle(
                    "is-hidden",
                    !shouldShow
                );

                if (shouldShow) {
                    if (observer) {
                        observer.unobserve(card);

                        card.classList.remove(
                            "htf-gallery--visible"
                        );

                        card.classList.add(
                            "htf-gallery--pending"
                        );

                        observer.observe(card);
                    } else {
                        card.classList.add(
                            "htf-gallery--visible"
                        );
                    }
                }
            });
        }

        buttons.forEach(function (button) {
            button.addEventListener("click", function () {
                filterGallery(this.dataset.filter);
            });
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initializeGallery
        );
    } else {
        initializeGallery();
    }
})();
