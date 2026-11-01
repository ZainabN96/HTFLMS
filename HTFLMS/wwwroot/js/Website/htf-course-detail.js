(function () {
    'use strict';

    var root = document.querySelector('.htf-cd');
    if (!root) return;

    /* ---------- Requirements ) ---------- */
    var need = root.querySelector('.htf-cd__need');
    var needBtn = root.querySelector('.htf-cd__need-head');
    if (need && needBtn) {
        needBtn.addEventListener('click', function () {
            var open = need.classList.toggle('is-open');
            needBtn.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    }

    /* ---------- Sticky tabs: oval indicator + scroll spy ---------- */
    var tabs = root.querySelector('.htf-cd__tabs');
    if (tabs) {
        var links = [].slice.call(tabs.querySelectorAll('a'));
        var secs = links.map(function (a) {
            return document.querySelector(a.getAttribute('href'));
        });
        var cur = -1;

        var place = function () {
            var a = links[cur < 0 ? 0 : cur];
            if (!a) return;
            tabs.style.setProperty('--x', a.offsetLeft + 'px');
            tabs.style.setProperty('--w', a.offsetWidth + 'px');
        };

        var spy = function () {
            var nav = parseInt(getComputedStyle(root).getPropertyValue('--cd-nav'), 10) || 80;
            var y = window.scrollY + nav + 120;
            var n = 0;
            secs.forEach(function (s, i) {
                if (s && s.getBoundingClientRect().top + window.scrollY <= y) n = i;
            });
            if (window.innerHeight + window.scrollY >= document.documentElement.scrollHeight - 4) {
                n = secs.length - 1;
            }
            if (n !== cur) {
                cur = n;
                links.forEach(function (a, i) {
                    a.classList.toggle('is-active', i === n);
                });
                place();
            }
        };

        window.addEventListener('scroll', spy, { passive: true });
        window.addEventListener('resize', place);
        if (document.fonts && document.fonts.ready) document.fonts.ready.then(place);
        place();
        spy();
    }

    /* ---------- Scroll reveal ---------- */
    var items = [].slice.call(root.querySelectorAll('.htf-cd__rv'));
    if (!items.length) return;

    if (!('IntersectionObserver' in window)) {
        items.forEach(function (el) { el.classList.add('is-in'); });
        return;
    }

    var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (e) {
            if (e.isIntersecting) {
                e.target.classList.add('is-in');
                io.unobserve(e.target);
            }
        });
    }, { threshold: 0.12, rootMargin: '0px 0px -6% 0px' });

    items.forEach(function (el) { io.observe(el); });
})();