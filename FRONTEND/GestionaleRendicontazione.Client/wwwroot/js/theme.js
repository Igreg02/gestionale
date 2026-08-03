// theme.js — Toggle dark/light theme with localStorage persistence
// + "reveal" animation (cerchio che si espande dal punto del click).
//
// Espone window.gestionaleTheme con API usata dal componente Blazor
// MainLayout. Si auto-esegue al boot per evitare FOUC: applica la classe
// .dark al <body> PRIMA del primo render di Blazor.
//
// Riferimento: stessa logica del bottone dark mode di docbridge
// (vedi docbridge/app.js: themeToggle click handler), adattata ai token
// del gestionale e integrata con il binding Blazor.
(function () {
    const KEY = "gestionale-dark-mode";

    function apply(dark) {
        document.body.classList.toggle("dark", !!dark);
    }

    function read() {
        try { return localStorage.getItem(KEY) === "1"; }
        catch { return false; }
    }

    function write(dark) {
        try {
            if (dark) localStorage.setItem(KEY, "1");
            else localStorage.removeItem(KEY);
        } catch {
            // localStorage pieno o non disponibile (modalità privata): ignora
            // silenziosamente — il toggle funziona comunque nella sessione.
        }
    }

    // Anima la transizione dark/light espandendo un cerchio dal punto in cui
    // l'utente ha cliccato (passato da Blazor come x/y). Se x/y non sono
    // passati (o sono 0,0, es. toggle da tastiera) cade al centro dello
    // schermo.
    // Raddoppiati rispetto ai valori originali (500ms / 0.6s), poi raddoppiati
    // ancora: durata 2s per un reveal "cinematografico" e raggio 4x la
    // diagonale (il cerchio è grande il quadruplo dell'originale).
    const ANIM_DURATION_MS = 1000;
    const ANIM_FALLBACK_TIMEOUT_MS = 1000;

    function animateThemeSwitch(dark, x, y) {
        const cx = (typeof x === "number" && isFinite(x) && x > 0) ? x : window.innerWidth / 2;
        const cy = (typeof y === "number" && isFinite(y) && y > 0) ? y : window.innerHeight / 2;

        // 1) Path preferito: View Transitions API
        if (document.startViewTransition) {
            // Raggio 4x la diagonale: il cerchio parte piccolo al click
            // point e cresce fino a coprire la viewport e oltre, con un
            // oversize marcato che rende l'effetto più "drammatico".
            const radius = 4 * Math.hypot(window.innerWidth, window.innerHeight);
            const transition = document.startViewTransition(() => {
                apply(dark);
                write(dark);
            });
            transition.ready.then(() => {
                document.documentElement.animate(
                    {
                        clipPath: [
                            `circle(0px at ${cx}px ${cy}px)`,
                            `circle(${radius}px at ${cx}px ${cy}px)`
                        ]
                    },
                    {
                        duration: ANIM_DURATION_MS,
                        easing: "cubic-bezier(0.4, 0, 0.2, 1)",
                        pseudoElement: "::view-transition-new(root)"
                    }
                );
            });
            return;
        }

        // 2) Fallback: cerchio via body::before (dimensione e durata
        // gestite dal CSS; qui aggiorniamo solo le coordinate).
        document.body.style.setProperty("--clip-x", `${cx}px`);
        document.body.style.setProperty("--clip-y", `${cy}px`);
        document.body.classList.add("animating-theme");
        // Esegui il cambio classe nel frame successivo, così il browser
        // vede il "prima" (cerchio a scale 0) e può animare fino al "dopo".
        requestAnimationFrame(() => {
            apply(dark);
            write(dark);
            // Togli la classe appena la transition CSS è terminata.
            setTimeout(() => {
                document.body.classList.remove("animating-theme");
            }, ANIM_FALLBACK_TIMEOUT_MS);
        });
    }

    window.gestionaleTheme = {
        get() { return read(); },

        set(dark, x, y) {
            animateThemeSwitch(dark, x, y);
        }
    };

    // Auto-applica lo stato salvato appena lo script viene caricato.
    // Viene caricato in index.html PRIMA di blazor.webassembly.js, quindi
    // la classe è già presente quando Blazor esegue il primo render.
    apply(read());
})();
