
(function () {
    'use strict';

    var grid = document.getElementById('htfCoursesGrid');
    if (!grid) return;

    var cards = Array.prototype.slice.call(grid.querySelectorAll('.htf-courses__card'));
    var pills = Array.prototype.slice.call(document.querySelectorAll('.htf-courses__filter'));
    var search = document.getElementById('htfCoursesSearch');
    var noMatch = document.getElementById('htfCoursesNoMatch');

    var activeFilter = 'all';

    function applyFilters() {
        var term = (search ? search.value : '').trim().toLowerCase();
        var shown = 0;

        cards.forEach(function (card) {
            var okCategory = activeFilter === 'all' ||
                card.getAttribute('data-category') === activeFilter;

            var okSearch = !term ||
                (card.getAttribute('data-search') || '').indexOf(term) !== -1;

            var visible = okCategory && okSearch;

            card.classList.toggle('is-hidden', !visible);

            if (!visible) card.classList.remove('is-open');
            if (visible) shown++;
        });

        if (noMatch) noMatch.hidden = shown !== 0;
    }

    // Category filters
    pills.forEach(function (pill) {
        pill.addEventListener('click', function () {
            activeFilter = pill.getAttribute('data-filter');

            pills.forEach(function (p) {
                var on = p === pill;
                p.classList.toggle('is-active', on);
                p.setAttribute('aria-pressed', on ? 'true' : 'false');
            });

            applyFilters();
        });
    });

    // Course search
    if (search) {
        search.addEventListener('input', applyFilters);
    }

    // Course card -> Detail Page
    // Enroll Now -> Login Page (handled by its own href)
    grid.addEventListener('click', function (e) {

        if (e.target.closest('a, button')) return;

        var card = e.target.closest('.htf-courses__card');
        if (!card || card.classList.contains('is-hidden')) return;

        var url = card.getAttribute('data-detail-url');

        if (url) {
            window.location.href = url;
        }
    });

    // Keyboard accessibility
    grid.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' && e.key !== ' ') return;

        var card = e.target.closest('.htf-courses__card');
        if (!card || e.target !== card ||
            card.classList.contains('is-hidden')) return;

        e.preventDefault();

        var url = card.getAttribute('data-detail-url');

        if (url) {
            window.location.href = url;
        }
    });

})();
