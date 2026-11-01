(function () {
    'use strict';

    var grid = document.getElementById('htfCoursesGrid');
    if (!grid) return; // DB mein courses nahi hain

    var cards = Array.prototype.slice.call(grid.querySelectorAll('.htf-courses__card'));
    var pills = Array.prototype.slice.call(document.querySelectorAll('.htf-courses__filter'));
    var search = document.getElementById('htfCoursesSearch');
    var noMatch = document.getElementById('htfCoursesNoMatch');

    var activeFilter = 'all';

    function applyFilters() {
        var term = (search ? search.value : '').trim().toLowerCase();
        var shown = 0;

        cards.forEach(function (card) {
            var okCategory = activeFilter === 'all' || card.getAttribute('data-category') === activeFilter;
            var okSearch = !term || (card.getAttribute('data-search') || '').indexOf(term) !== -1;
            var visible = okCategory && okSearch;

            card.classList.toggle('is-hidden', !visible);
            if (!visible) card.classList.remove('is-open');
            if (visible) shown++;
        });

        if (noMatch) noMatch.hidden = shown !== 0;
    }

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

    if (search) search.addEventListener('input', applyFilters);

    // Touch devices: tap se card kholo/band karo (Enroll link par tap normal chalega)
    grid.addEventListener('click', function (e) {
        if (e.target.closest('a')) return;
        var card = e.target.closest('.htf-courses__card');
        if (!card) return;
        var wasOpen = card.classList.contains('is-open');
        cards.forEach(function (c) { c.classList.remove('is-open'); });
        if (!wasOpen) card.classList.add('is-open');
    });
})();