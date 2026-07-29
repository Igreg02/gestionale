// filterStorage.js — Persistenza filtri Dashboard via localStorage.
// Espone window.gestionaleFilters con API minima (load / save / clear)
// usata da FilterStateService per ricordare i filtri dell'utente tra
// refresh e navigazione. Solo i filtri "di sessione lunga" sono
// persistiti (date + selezioni dropdown); SearchQuery e stato pannello
// sono effimeri per design.
//
// Chiave di storage: 'gestionale.filters' — oggetto JSON con:
//   filterFrom   : string 'yyyy-MM-dd'
//   filterTo     : string 'yyyy-MM-dd'
//   employeeId   : string GUID o vuoto
//   projectId    : string GUID o vuoto
//   statusName   : string (nome stato) o vuoto
//
// Tutte le operazioni sono try/catched: localStorage può non essere
// disponibile (modalità privata, browser limitati, ambiente di test).
(function () {
    const KEY = "gestionale.filters";

    function read() {
        try {
            const raw = localStorage.getItem(KEY);
            if (!raw) return null;
            return JSON.parse(raw);
        } catch {
            // JSON corrotto o localStorage non disponibile: riparti pulito
            return null;
        }
    }

    function write(data) {
        try {
            localStorage.setItem(KEY, JSON.stringify(data));
        } catch {
            // localStorage pieno o non disponibile: ignora silenziosamente —
            // i filtri funzionano comunque nella sessione corrente.
        }
    }

    function clear() {
        try {
            localStorage.removeItem(KEY);
        } catch {
            // ignora
        }
    }

    window.gestionaleFilters = {
        load: read,
        save: write,
        clear: clear
    };
})();
