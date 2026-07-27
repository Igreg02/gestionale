// theme.js — Toggle dark/light theme with localStorage persistence.
// Espone window.gestionaleTheme con API minima (get / set) usata dal
// componente Blazor MainLayout. Si auto-esegue al boot per evitare FOUC:
// applica la classe .dark al <body> PRIMA del primo render di Blazor.
//
// Riferimento: stessa logica del bottone dark mode di docbridge
// (vedi docbridge/app.js: toggleThemeClass), adattata ai token del gestionale.
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

    window.gestionaleTheme = {
        get() { return read(); },
        set(dark) {
            write(!!dark);
            apply(dark);
        }
    };

    // Auto-applica lo stato salvato appena lo script viene caricato.
    // Viene caricato in index.html PRIMA di blazor.webassembly.js, quindi
    // la classe è già presente quando Blazor esegue il primo render.
    apply(read());
})();