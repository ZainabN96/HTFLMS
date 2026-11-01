/* =========================================================
   ABOUT PAGE  –  wwwroot/js/about.js
     ========================================================= */
(function () {
    'use strict';

    var reduce = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    /* ---------- 1. Scroll reveal (.reveal, timeline items, card groups) ---------- */
    var revealTargets = [].slice.call(document.querySelectorAll('.about-page .reveal, .about-page .ab-tl__item'));

    // data-stagger groups: har child ko index (--s) do aur alag-alag observe karo
    document.querySelectorAll('.about-page [data-stagger]').forEach(function (group) {
        [].slice.call(group.children).forEach(function (child, i) {
            child.style.setProperty('--s', i);
            revealTargets.push(child);
        });
    });

    if (reduce || !('IntersectionObserver' in window)) {
        revealTargets.forEach(function (el) { el.classList.add('is-in'); });
    } else {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (e.isIntersecting) {
                    e.target.classList.add('is-in');
                    io.unobserve(e.target);
                }
            });
        }, { threshold: 0.15, rootMargin: '0px 0px -6% 0px' });
        revealTargets.forEach(function (el) { io.observe(el); });
    }

    /* ---------- 2. Timeline rail progress (scroll ke sath line draw hoti hai) ---------- */
    var tl = document.getElementById('abTimeline');
    if (tl && !reduce) {
        var ticking = false;

        var update = function () {
            var r = tl.getBoundingClientRect();
            var vh = window.innerHeight || document.documentElement.clientHeight;
            // line tab bharna shuru jab timeline screen ke ~75% tak aaye, end pe full
            var p = (vh * 0.75 - r.top) / r.height;
            p = Math.max(0, Math.min(1, p));
            tl.style.setProperty('--progress', p.toFixed(3));
            ticking = false;
        };

        var onScroll = function () {
            if (!ticking) { ticking = true; requestAnimationFrame(update); }
        };

        window.addEventListener('scroll', onScroll, { passive: true });
        window.addEventListener('resize', onScroll);
        update();
    } else if (tl) {
        tl.style.setProperty('--progress', 1);
    }

    /* ---------- 3. Year count-up (halka sa, sirf pehli baar) ---------- */
    if (!reduce && 'IntersectionObserver' in window) {
        var yearObserver = new IntersectionObserver(function (entries) {
            entries.forEach(function (e) {
                if (!e.isIntersecting) return;
                var el = e.target;
                yearObserver.unobserve(el);
                var end = parseInt(el.getAttribute('data-year'), 10);
                var start = end - 18;
                var t0 = null, dur = 900;
                (function step(ts) {
                    if (!t0) t0 = ts;
                    var k = Math.min((ts - t0) / dur, 1);
                    var eased = 1 - Math.pow(1 - k, 3);
                    el.textContent = Math.round(start + (end - start) * eased);
                    if (k < 1) requestAnimationFrame(step);
                })(performance.now());
            });
        }, { threshold: 0.6 });

        document.querySelectorAll('.about-page .ab-tl__year').forEach(function (el) {
            el.setAttribute('data-year', el.textContent.trim());
            yearObserver.observe(el);
        });
    }

    /* ---------- 4. Board cards: touch devices pe tap se glass detail ---------- */
    var canHover = window.matchMedia && window.matchMedia('(hover: hover)').matches;
    if (!canHover) {
        var cards = [].slice.call(document.querySelectorAll('.about-page .ab-bcard'));
        cards.forEach(function (card) {
            card.addEventListener('click', function (ev) {
                if (ev.target.closest('a')) return; // LinkedIn link normal chale
                var open = card.classList.contains('is-open');
                cards.forEach(function (c) { c.classList.remove('is-open'); });
                if (!open) card.classList.add('is-open');
            });
        });
    }
})();
