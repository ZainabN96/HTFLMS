/* =========================================================
   HCC Technology Foundation — interactions & animations
   Requires: GSAP 3 + ScrollTrigger (loaded in index.html)
   ========================================================= */
(() => {
    "use strict";

    const $ = (s, c = document) => c.querySelector(s);
    const $$ = (s, c = document) => Array.from(c.querySelectorAll(s));

    const reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const hasGSAP = typeof window.gsap !== "undefined" && typeof window.ScrollTrigger !== "undefined";
    if (hasGSAP) gsap.registerPlugin(ScrollTrigger);

    /* ---------------------------------------------------------
       Mobile menu
       --------------------------------------------------------- */
    (function menu() {
        const burger = $("#burger"), nav = $("#nav");
        if (!burger || !nav) return;
        const set = (open) => {
            nav.classList.toggle("is-open", open);
            burger.setAttribute("aria-expanded", String(open));
            document.body.style.overflow = open ? "hidden" : "";
        };
        burger.addEventListener("click", () => set(!nav.classList.contains("is-open")));
        $$("a", nav).forEach((a) => a.addEventListener("click", () => set(false)));
    })();

    /* ---------------------------------------------------------
       Partners marquee — clone the set for a seamless loop
       (works with or without GSAP)
       --------------------------------------------------------- */
    (function marquee() {
        const track = $("#partnersTrack");
        if (!track) return;
        const set = track.firstElementChild;
        const clone = set.cloneNode(true);
        clone.setAttribute("aria-hidden", "true");
        $$("img", clone).forEach((i) => (i.alt = ""));
        track.appendChild(clone);
    })();

    /* ---------------------------------------------------------
       Bridge strip — duplicate the set for a seamless loop
       --------------------------------------------------------- */
    (function bridgeClone() {
        const track = $("#bridgeTrack");
        if (!track) return;
        track.appendChild(track.firstElementChild.cloneNode(true));
    })();
    /* =========================================================
       SUCCESS STORIES — looping carousel + glass spotlight
       • cards loop endlessly, one step at a time
       • the card that reaches the centre gets the glass effect:
         photo floats in, frosted panel morphs open, story appears
       • works without GSAP (plain CSS animations)
       ========================================================= */
    (function stories() {
        const section = $(".stories");
        const track = $("#storiesTrack");
        if (!section || !track) return;

        const cards = $$(".s-card", track);
        const N = cards.length;
        if (N < 5) return;

        const SLOT = 344;       // card width 320 + gap 24
        const STAY = 4800;      // ms each card stays in the centre
        const SVG = "http://www.w3.org/2000/svg";

        const el = (tag, cls, text) => {
            const e = document.createElement(tag);
            if (cls) e.className = cls;
            if (text) e.textContent = text;
            return e;
        };

        /* ---- build the glass panel for every card from its own content */
        cards.forEach((card) => {
            const name = $(".s-card__name", card).textContent.trim();
            const role = $(".s-card__role", card).textContent.trim();
            const quote = $(".s-card__quote", card).textContent.trim();
            const avatar = $(".s-card__avatar", card).getAttribute("src");

            const glass = el("div", "s-glass");
            glass.setAttribute("aria-hidden", "true");
            const inner = el("div", "s-glass__in");

            const q = el("p", "s-glass__quote");
            const words = quote.split(/\s+/);
            words.forEach((w, i) => {
                const s = el("span", "w", w + (i < words.length - 1 ? " " : ""));
                s.style.setProperty("--i", i);
                q.appendChild(s);
            });

            const foot = el("div", "s-glass__foot");
            const img = el("img", "s-glass__avatar");
            img.src = avatar; img.alt = "";
            const who = el("div", "s-glass__who");
            who.appendChild(el("b", "", name));
            who.appendChild(el("span", "", role));

            const heart = document.createElementNS(SVG, "svg");
            heart.setAttribute("class", "s-glass__heart");
            heart.setAttribute("viewBox", "0 0 24 24");
            const path = document.createElementNS(SVG, "path");
            path.setAttribute("fill", "currentColor");
            path.setAttribute("d", "M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z");
            heart.appendChild(path);

            foot.append(img, who, heart);
            inner.append(q, foot);
            glass.appendChild(inner);
            card.appendChild(glass);
        });

        section.classList.add("is-loop");

        /* ---- positions: every card sits at (distance from centre) * SLOT */
        let pos = 0;
        const prev = new Array(N).fill(null);

        const layout = (first) => {
            cards.forEach((c, i) => {
                let d = (((i - pos) % N) + N) % N;
                if (d > N / 2) d -= N;
                // a card jumping from one far side to the other does it off-screen, without animating
                const wrapped = prev[i] !== null && Math.abs(d - prev[i]) > N / 2;
                c.classList.toggle("is-jump", wrapped || !!first);
                c.style.transform = "translateX(" + d * SLOT + "px)";
                prev[i] = d;
            });
        };
        const setActive = () => cards.forEach((c, i) => c.classList.toggle("is-active", i === pos));

        layout(true);

        /* ---- autoplay (only while the section is on screen) */
        let timer = null, started = false;

        const next = () => {
            cards.forEach((c) => c.classList.remove("is-active"));
            pos = (pos + 1) % N;
            layout(false);
            setTimeout(setActive, 450); // glass starts as the card reaches the centre
        };
        const run = () => {
            if (timer) return;
            if (!started) { started = true; setTimeout(setActive, 300); }
            else setActive();
            timer = setInterval(next, STAY);
        };
        const halt = () => { clearInterval(timer); timer = null; };

        if ("IntersectionObserver" in window) {
            new IntersectionObserver((entries) => {
                entries[0].isIntersecting ? run() : halt();
            }, { threshold: 0.35 }).observe(section);
        } else {
            run();
        }
    })();

    /* ---------------------------------------------------------
       Word-swap measuring helper (also used for reduced motion)
       --------------------------------------------------------- */
    const swapWrap = $("#swapWord");
    const swapItems = swapWrap ? $$(".swap__item", swapWrap) : [];
    const swapMask = swapWrap ? swapWrap.parentElement : null;

    function measureSwap() {
        if (!swapMask) return;
        let max = 0;
        swapItems.forEach((el) => { max = Math.max(max, el.scrollWidth, el.getBoundingClientRect().width); });
        swapWrap.style.width = Math.ceil(max) + "px";
    }

    if (!hasGSAP || reduce) {
        measureSwap();
        if (document.fonts && document.fonts.ready) document.fonts.ready.then(measureSwap);
        window.addEventListener("resize", measureSwap);
        return; // everything below needs GSAP; page stays fully readable without it
    }

    /* =========================================================
       1. HERO — entrance
       ========================================================= */
    (function hero() {
        const lines = $$('[data-hero="line"]');
        const grow = $('[data-hero="grow"]');
        const photos = $$('[data-hero="photo"]');
        const rot = [3, -6, -3];

        gsap.set(lines, { yPercent: 150 });
        gsap.set(grow, { yPercent: 150, rotate: 4 });
        gsap.set($('[data-hero="eyebrow"]'), { opacity: 0, y: 18 });
        gsap.set($('[data-hero="lead"]'), { opacity: 0, y: 24 });
        photos.forEach((p, i) => gsap.set(p, { rotation: rot[i], opacity: 0, y: 90, scale: .88 }));
        gsap.set(".header__logo, .nav a, .header__cta", { opacity: 0, y: -14 });

        const tl = gsap.timeline({ defaults: { ease: "expo.out" }, delay: .15 });
        tl.to(".header__logo, .nav a, .header__cta", { opacity: 1, y: 0, duration: 1, stagger: .08 }, 0)
            .to('[data-hero="eyebrow"]', { opacity: 1, y: 0, duration: 1 }, .15)
            .to(lines, { yPercent: 0, duration: 1.4, stagger: .12 }, .25)
            .to(grow, { yPercent: 0, rotate: 0, duration: 1.5 }, .6)
            .to('[data-hero="lead"]', { opacity: 1, y: 0, duration: 1.1 }, .95)
            .to(photos, { opacity: 1, y: 0, scale: 1, duration: 1.4, stagger: .16, ease: "back.out(1.15)" }, .55);

        // gentle parallax on the photo cluster while leaving the hero
        const depth = [-70, -30, -110];
        photos.forEach((p, i) => {
            gsap.to(p, {
                yPercent: depth[i] / 3, ease: "none",
                scrollTrigger: { trigger: ".hero", start: "top top", end: "bottom top", scrub: true }
            });
        });
    })();

    /* =========================================================
       Generic reveal
       ========================================================= */
    $$("[data-reveal]").forEach((el) => {
        gsap.from(el, {
            y: 42, opacity: 0, duration: 1.1, ease: "expo.out",
            scrollTrigger: { trigger: el, start: "top 88%", once: true }
        });
    });

    /* =========================================================
       2. STATEMENT — rolling word swap
          (reference: "We make things that matter to ___")
          old word slides up & out of a mask, new word slides up in
       ========================================================= */
    (function wordSwap() {
        if (!swapMask || swapItems.length < 2) return;

        const lines = $$(".swap__line");
        gsap.set(swapItems, { yPercent: 150 });
        gsap.set(swapItems[0], { yPercent: 0 });
        gsap.set(lines, { yPercent: 112 });
        measureSwap();

        let idx = 0, inView = false, timer = null, started = false;
        const HOLD = 2.0;

        const next = () => {
            if (!inView) { timer = null; return; }
            const n = (idx + 1) % swapItems.length;
            const out = swapItems[idx], inn = swapItems[n];
            gsap.timeline({
                onComplete: () => { idx = n; timer = gsap.delayedCall(HOLD, next); }
            })
                .to(out, { yPercent: -150, duration: .42, ease: "power3.in" }, 0)
                .fromTo(inn, { yPercent: 150 }, { yPercent: 0, duration: .55, ease: "power3.out" }, .2);
        };

        ScrollTrigger.create({
            trigger: ".statement",
            start: "top 78%",
            end: "bottom 10%",
            onToggle: (self) => {
                inView = self.isActive;
                if (inView && !timer && started) timer = gsap.delayedCall(.2, next);
            },
            onEnter: () => {
                if (started) return;
                started = true;
                gsap.to(lines, { yPercent: 0, duration: 1.3, ease: "expo.out", stagger: .1 });
                timer = gsap.delayedCall(2.4, next);
            }
        });

        if (document.fonts && document.fonts.ready) document.fonts.ready.then(() => { measureSwap(); ScrollTrigger.refresh(); });
        window.addEventListener("resize", measureSwap);
    })();

    /* =========================================================
       3. STATS — "liquid fill" odometer
          every glyph starts as a thin outline; its reel spins to the
          value while the colour fills the glyph from the bottom up.
          Columns finish left → right, the "+" fills last.
          (reference: outlined digits filling like a tank)
       ========================================================= */
    (function stats() {
        const grid = $("#statsGrid");
        const nums = $$(".stat__num");
        if (!grid || !nums.length) return;

        const REEL = 30;            // 3 x (0-9): two full spins + landing
        const reels = [];

        const makeStrip = () => {
            const strip = document.createElement("span"); strip.className = "strip";
            for (let k = 0; k < REEL; k++) {
                const s = document.createElement("span"); s.textContent = k % 10; strip.appendChild(s);
            }
            return strip;
        };

        nums.forEach((el) => {
            const value = el.dataset.value, suffix = el.dataset.suffix || "";
            el.textContent = "";
            const digits = [];

            [...value].forEach((ch) => {
                const d = +ch;
                const wrap = document.createElement("span"); wrap.className = "digit";
                const lo = document.createElement("span"); lo.className = "layer layer--o";
                const lf = document.createElement("span"); lf.className = "layer layer--f";
                const so = makeStrip(), sf = makeStrip();
                lo.appendChild(so); lf.appendChild(sf);
                wrap.append(lo, lf); el.appendChild(wrap);
                digits.push({ wrap, so, sf, fill: lf, d });
            });

            const suf = document.createElement("span"); suf.className = "suffix";
            const so = document.createElement("span"); so.className = "suffix__o"; so.textContent = suffix;
            const sf = document.createElement("span"); sf.className = "suffix__f"; sf.textContent = suffix;
            suf.append(so, sf); el.appendChild(suf);
            reels.push({ el, digits, sf });
        });

        // lock each column to the width of its final digit (so "1" doesn't leave a gap)
        const fit = () => {
            reels.forEach(({ el, digits }) => {
                digits.forEach(({ wrap, d }) => {
                    const probe = document.createElement("span");
                    probe.textContent = d;
                    probe.style.cssText = "position:absolute;visibility:hidden;white-space:pre;";
                    el.appendChild(probe);
                    wrap.style.width = probe.getBoundingClientRect().width + "px";
                    probe.remove();
                });
            });
        };
        fit();
        if (document.fonts && document.fonts.ready) document.fonts.ready.then(fit);
        window.addEventListener("resize", fit);

        ScrollTrigger.create({
            trigger: grid, start: "top 80%", once: true,
            onEnter: () => {
                grid.classList.add("is-in");
                reels.forEach(({ digits, sf }, col) => {
                    const base = col * .2;
                    digits.forEach(({ so, sf: strip, fill, d }, i) => {
                        const delay = base + i * .14, dur = 2.4 + i * .35;
                        // reels spin to the digit
                        gsap.to([so, strip], { yPercent: -((20 + d) / REEL) * 100, duration: dur, delay, ease: "power3.out" });
                        // colour rises from the bottom like liquid
                        gsap.to(fill, { clipPath: "inset(0% 0 0 0)", duration: dur * .92, delay, ease: "power2.inOut" });
                    });
                    gsap.to(sf, {
                        clipPath: "inset(0% 0 0 0)", duration: 1, ease: "power2.out",
                        delay: base + .6 + digits.length * .5
                    });
                });
            }
        });
    })();

    /* =========================================================
       4. GALLERY — scroll-driven horizontal cards
          (reference: cards rise from below at different heights,
          drift sideways with the scroll and settle in a row)
       ========================================================= */
    (function gallery() {
        const section = $(".gallery");
        const track = $("#galleryTrack");
        const viewport = $(".gallery__viewport");
        if (!section || !track) return;
        const cards = $$(".g-card", track);

        // heading
        gsap.set([".gallery__l1", ".gallery__l2"], { yPercent: 110 });
        gsap.set(".gallery__open", { opacity: 0, y: 22 });
        gsap.timeline({
            scrollTrigger: { trigger: ".gallery__head", start: "top 82%", once: true },
            defaults: { ease: "expo.out" }
        })
            .to(".gallery__l1", { yPercent: 0, duration: 1.4 })
            .to(".gallery__l2", { yPercent: 0, duration: 1.5 }, .14)
            .to(".gallery__open", { opacity: 1, y: 0, duration: 1 }, .45);

        // horizontal drift tied to scroll
        const dist = () => Math.max(0, track.scrollWidth - window.innerWidth);
        gsap.to(track, {
            x: () => -dist(), ease: "none",
            scrollTrigger: {
                trigger: viewport, start: "top 92%", end: "bottom 72%",
                scrub: .9, invalidateOnRefresh: true
            }
        });

        // each card: rises from a different depth, tilts and scales in (staggered)
        cards.forEach((card, i) => {
            const lowStart = i % 2 === 0 ? 260 : 380;
            gsap.fromTo(card,
                { y: lowStart + i * 46, scale: .86, rotate: i % 2 === 0 ? -3 : 3, opacity: 0, transformOrigin: "50% 100%" },
                {
                    y: 0, scale: 1, rotate: 0, opacity: 1, ease: "power3.out",
                    scrollTrigger: {
                        trigger: viewport,
                        start: `top ${96 - i * 5}%`,
                        end: `top ${40 - i * 4}%`,
                        scrub: .9
                    }
                });
        });

        // image parallax inside each card — slides against the drift direction
        cards.forEach((card) => {
            const img = $("img", card);
            gsap.set(img, { scale: 1.22 });
            gsap.fromTo(img, { xPercent: 7 }, {
                xPercent: -7, ease: "none",
                scrollTrigger: { trigger: viewport, start: "top 92%", end: "bottom 72%", scrub: .9 }
            });
        });

        window.addEventListener("load", () => ScrollTrigger.refresh());
    })();

    /* =========================================================
       BRIDGE STRIP — mission text marquee
       drifts to the right (opposite to the partner logos) and
       reacts to scroll speed: faster scroll = faster strip
       ========================================================= */
    (function bridge() {
        const track = $("#bridgeTrack");
        if (!track) return;
        track.style.animation = "none";
        const tw = gsap.fromTo(track, { xPercent: -50 }, { xPercent: 0, duration: 42, ease: "none", repeat: -1 });
        ScrollTrigger.create({
            onUpdate: (self) => {
                const t = 1 + gsap.utils.clamp(-6, 6, self.getVelocity() / 420);
                gsap.to(tw, { timeScale: t, duration: .25, overwrite: true });
                gsap.to(tw, { timeScale: 1, duration: 1.3, delay: .25, overwrite: false });
            }
        });
        gsap.from(".bridge", {
            clipPath: "inset(0 50% 0 50%)", duration: 1.4, ease: "expo.out",
            scrollTrigger: { trigger: ".bridge", start: "top 92%", once: true }
        });
    })();

    /* =========================================================
       5. CTA — "Ready to start?"
          • the pink block's top corners flatten as it scrolls in
          • headline letters flip up out of a mask, "start" waves
          • "?" wobbles, soft blobs drift with the pointer
          • button is magnetic, fills with a ripple from the cursor
          • pulse rings radiate from the button
       ========================================================= */
    (function cta() {
        const shape = $("#ctaShape");
        if (!shape) return;

        // ---- split text into masked characters
        const split = (el, startIndex = 0) => {
            const text = el.textContent;
            el.textContent = "";
            el.setAttribute("aria-hidden", "true");
            let i = startIndex;
            [...text].forEach((ch) => {
                if (ch === " ") {
                    const sp = document.createElement("span");
                    sp.className = "char--space"; sp.style.display = "inline-block"; sp.innerHTML = "&nbsp;";
                    el.appendChild(sp); return;
                }
                const m = document.createElement("span"); m.className = "char-mask";
                const c = document.createElement("span"); c.className = "char"; c.textContent = ch;
                c.style.setProperty("--i", i++);
                m.appendChild(c); el.appendChild(m);
            });
            return $$(".char", el);
        };
        const c1 = split($("[data-split]"));
        const c2 = split($("[data-split-italic]"));
        const q = $(".cta__q");
        const btnWrap = $(".cta__btnwrap");

        // ---- scroll-linked shape: big rounded top → flat, side margins close
        gsap.fromTo(shape,
            { borderTopLeftRadius: "20vw", borderTopRightRadius: "20vw", marginLeft: "6vw", marginRight: "6vw" },
            {
                borderTopLeftRadius: "0vw", borderTopRightRadius: "0vw", marginLeft: "0vw", marginRight: "0vw",
                ease: "none",
                scrollTrigger: { trigger: shape, start: "top 96%", end: "top 8%", scrub: .8 }
            });

        // ---- entrance
        gsap.set(c1, { yPercent: 125, rotate: 9 });
        gsap.set(c2, { yPercent: 125, rotate: -9 });
        gsap.set(q, { scale: 0, rotate: -35 });
        gsap.set(btnWrap, { opacity: 0, y: 46 });

        gsap.timeline({
            scrollTrigger: { trigger: shape, start: "top 38%", once: true },
            defaults: { ease: "expo.out" }
        })
            .to(c1, { yPercent: 0, rotate: 0, duration: 1.35, stagger: .045 }, 0)
            .to(c2, { yPercent: 0, rotate: 0, duration: 1.5, stagger: .07 }, .35)
            .to(q, { scale: 1, rotate: 0, duration: 1.1, ease: "elastic.out(1, .5)" }, .95)
            .to(btnWrap, { opacity: 1, y: 0, duration: 1.2 }, 1.05)
            .add(() => {
                gsap.to(q, { rotate: 12, duration: 1.2, ease: "sine.inOut", yoyo: true, repeat: -1 });
            }, 1.9);

        // ---- blobs follow the pointer (depths differ → parallax)
        const blobs = $$(".blob", shape);
        const depths = [-60, 80, -40];
        shape.addEventListener("pointermove", (e) => {
            const r = shape.getBoundingClientRect();
            const nx = (e.clientX - r.left) / r.width - .5;
            const ny = (e.clientY - r.top) / r.height - .5;
            blobs.forEach((b, i) => gsap.to(b, { x: nx * depths[i], y: ny * depths[i], duration: 1.6, ease: "power3.out", overwrite: "auto" }));
        });

        // ---- magnetic button + ripple fill origin
        const btn = $("#ctaBtn");
        if (btn && window.matchMedia("(hover: hover)").matches) {
            const qx = gsap.quickTo(btn, "x", { duration: .6, ease: "power3.out" });
            const qy = gsap.quickTo(btn, "y", { duration: .6, ease: "power3.out" });
            const label = $(".cta__btn-label", btn);
            const lx = gsap.quickTo(label, "x", { duration: .5, ease: "power3.out" });
            const ly = gsap.quickTo(label, "y", { duration: .5, ease: "power3.out" });

            btnWrap.addEventListener("pointermove", (e) => {
                const r = btn.getBoundingClientRect();
                const cx = r.left + r.width / 2, cy = r.top + r.height / 2;
                const dx = e.clientX - cx, dy = e.clientY - cy;
                const dist = Math.hypot(dx, dy);
                if (dist < Math.max(r.width, 220)) {
                    qx(dx * .32); qy(dy * .4); lx(dx * .08); ly(dy * .12);
                } else { qx(0); qy(0); lx(0); ly(0); }
                btn.style.setProperty("--mx", e.clientX - r.left + "px");
                btn.style.setProperty("--my", e.clientY - r.top + "px");
            });
            btnWrap.addEventListener("pointerleave", () => {
                gsap.to(btn, { x: 0, y: 0, duration: 1.1, ease: "elastic.out(1, .4)" });
                gsap.to(label, { x: 0, y: 0, duration: .8, ease: "power3.out" });
            });
            btn.addEventListener("pointerenter", (e) => {
                const r = btn.getBoundingClientRect();
                btn.style.setProperty("--mx", e.clientX - r.left + "px");
                btn.style.setProperty("--my", e.clientY - r.top + "px");
            });
            btn.addEventListener("pointerleave", (e) => {
                const r = btn.getBoundingClientRect();
                btn.style.setProperty("--mx", e.clientX - r.left + "px");
                btn.style.setProperty("--my", e.clientY - r.top + "px");
            });
        }
    })();

    /* ---------------------------------------------------------
       Refresh triggers once fonts / images settle
       --------------------------------------------------------- */
    if (document.fonts && document.fonts.ready) document.fonts.ready.then(() => ScrollTrigger.refresh());
    window.addEventListener("load", () => ScrollTrigger.refresh());
})();
