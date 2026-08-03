# Audit del Codice — Gestionale (API + DATAACCESS + DOMAIN + Blazor Client)

## Contesto

Richiesta una revisione completa del codebase: meglio continuare a migliorare il codice esistente o passare a nuove funzionalità? Due analisi parallele hanno coperto backend (API/DATAACCESS/DOMAIN) e frontend (Blazor Client). Risultati sotto. I commit recenti mostrano un refactor già in corso (astrazione CRUD generica `XpoCrudServiceBase`), quindi questo audit arriva a metà del refactor — alcuni punti sono "finire ciò che è iniziato".

Non esiste nessun progetto di test nella soluzione (né backend né frontend).

## Risultati — Backend (API / DATAACCESS / DOMAIN)

**Sicurezza / correttezza (da sistemare per primi, poco sforzo):**
- `API/appsettings.json:18` — segreto JWT placeholder committato. Va spostato su env var/user-secrets prima di qualsiasi deploy reale.
- `DATAACCESS/Datacontext/DataSeeder.cs:45` — admin seedato con password hardcoded `Admin123!`, nessun cambio forzato.
- `DATAACCESS/Services/TokenBlacklistService.cs:38,59-81` — task fire-and-forget con `catch{}` vuoto, ingoia silenziosamente i fallimenti di pruning.
- `API/helpers/XpoSerilogSink.cs:70-77` — stesso pattern di swallow silenzioso per i fallimenti del sink di logging.
- `API/Controllers/AuthController.cs:63-81` — `Logout` non fa nulla (ritorna comunque 204) quando il JWT non ha `jti`, il token non viene mai revocato.
- `API/Controllers/AuthController.cs` (`Login`) — nessun rate limiting/lockout, attaccabile con brute-force.
- `MappingProfile.cs:155-171` / `ProjectService.cs:44-47` — FK non valida lancia `InvalidOperationException` mappata a HTTP 409, dovrebbe essere 400/422 (è validazione, non conflitto).

**Architettura (finire il refactor):**
- `DeleteGuard.cs` codice morto — mai chiamato, stessa logica duplicata inline in `XpoCrudServiceBase.cs:102-108`.
- `WorkLogServices.cs`, `LogService.cs`, `ReportService.cs` mai migrati a `XpoCrudServiceBase` — CRUD ancora scritto a mano.
- `WorkLogAdminService`/`WorkLogUserService` duplicano CRUD quasi identico, differiscono solo per il controllo di ownership.
- `XpoCrudServiceBase.cs:44-61` — `GetAllAsync`/`GetByIdAsync` incapsulano chiamate XPO sincrone in `Task.FromResult`, nessun I/O asincrono reale (astrazione perdente su tutti e 5 i consumer).
- `WorklogController.cs:112-120,148-156` — mapping DTO Admin→User duplicato tra Create e Update.

**Manutenibilità:**
- `AuthService.cs:39-70` — `LoginAsync` ritorna una tupla a 7 elementi, dovrebbe essere un record.
- `ProjectController.cs:53-93` — non ha ricevuto il passaggio di formattazione/refactor fatto altrove.
- Stringhe di errore in italiano hardcoded sparse per servizio, non centralizzate.
- `EmployeeService` ha un controllo di unicità amichevole, `Company` no — esperienza inconsistente sui nomi duplicati.
- `Program.cs:22-23` — connection string SQLite duplicata come fallback inline rispetto ad appsettings.json.

**Nota positiva:** la nuova astrazione `XpoCrudServiceBase` è applicata correttamente su 5 servizi. Nessuna SQL injection, nessun CORS misconfigurato, nessun uso scorretto di `.Result`/`.Wait()`/`async void` nel backend.

## Risultati — Frontend (Blazor Client)

**Sicurezza:**
- `Services/TokenStorageService.cs:26,31` — JWT e claim in `localStorage`, leggibili da qualsiasi XSS. Valutare storage in memoria + refresh silenzioso, o almeno `sessionStorage`.
- `wwwroot/index.html:12` — link CDN bootstrap-icons senza SRI (`integrity`/`crossorigin`), inconsistente col CSS Bootstrap sopra.
- Nessun meta tag CSP presente.
- `ReportDownloadModal.razor:106-108` — `_fileName` non sanificato passato a `a.download`.
- I controlli di ruolo lato client sono solo UI — confermare che il backend rivalidi i ruoli per endpoint in modo indipendente (lo fa, secondo la review backend, ma vale la pena tenerlo a mente).

**Correttezza:**
- `Dashboard.razor.cs:66-72` — `GetCurrentUserId()` blocca con `.GetAwaiter().GetResult()` invece di fare await.
- `MainLayout.razor:340-347` — `System.Timers.Timer.Elapsed` chiama `InvokeAsync` senza guardia su componente già disposed, race con `Dispose()`.
- `ReportDataApiClient.cs:24-28` — manca try/catch attorno alla chiamata HTTP, inconsistente col pattern di `WorkLogApiClient`.
- `Dashboard.razor.cs:113,154` — handler `async void` su eventi di servizio, fragile.
- `FilterStateService.cs:161-186` — `LoadLookupsAsync` senza `CancellationToken`, non può annullare chiamate sovrapposte (il metodo gemello `ReloadLookupsAsync` lo fa già correttamente).
- I fallimenti di reload delle lookup finiscono solo in `Console.Error.WriteLine`, nessun feedback visibile all'utente.

**Architettura:**
- `CompanyApiClient/EmployeeApiClient/ProjectApiClient/StatusApiClient/TypeApiClient` — 5 wrapper CRUD quasi identici, stessa forma della duplicazione pre-refactor nel backend. Candidato naturale per un `CrudApiClient<TResponse,TCreate,TUpdate>` generico.
- `Admin/Services/{Companies,Statuses,Types}/*.razor.cs` — stesso scheletro apri/chiudi-modale/salva/elimina/reload copiato per ogni entità.
- Due gerarchie di DTO parallele per le stesse entità (es. `EmployeeResponse` vs `EmployeeResponseDto`).
- Blocchi UI di loading/errore copiati a mano su tutte le pagine Admin.
- 4 pattern di state management inconsistenti in uso (servizi pub/sub, cascading auth state, prop-drilling, campi locali), nessuna convenzione documentata.

**Manutenibilità:**
- Stringhe di route API hardcoded in 9 file, nessuna classe di costanti `ApiRoutes` (nonostante `Constants/RoleNames` esista già come pattern).
- Route delle pagine duplicate tra `Sidebar.razor` e la direttiva `@page` di ogni pagina.
- `RoleNames.cs` è uno specchio mantenuto a mano di un enum backend, nessun controllo di sincronizzazione a compile-time.

**Nota positiva:** nessun segreto hardcoded, nessun vettore XSS da `MarkupString`/HTML grezzo, protezione open-redirect in `Login.razor` corretta, igiene `IDisposable` solida altrove.

## Raccomandazione

Il codebase è in condizioni discrete, a metà refactor, non è un disastro. Prima delle nuove funzionalità:

1. **Fix di sicurezza veloci (ore, da fare per primi in ogni caso):** segreto JWT fuori da appsettings, password admin seedata, buco `jti` nel Logout, rate limiting sul login, SRI sul CDN, header CSP, sanificazione filename.
2. **Finire il refactor CRUD (entrambi i livelli)** — replicare il pattern backend `XpoCrudServiceBase` in un `CrudApiClient<T>` frontend + base condivisa per le pagine Admin. Stessa forma di lavoro già validata una volta (bb061b0/c7493d8), basso rischio, ripaga il blocco di duplicazione più grande su entrambi i lati.
3. **Poi le nuove funzionalità** — la base sarà più coerente su cui costruire, e la deriva tra route/DTO non si accumulerà.

Da saltare: retrofit di una suite di test completa (decisione di scope a parte), tupla→record e altri nitpick di stile puro (da inglobare nel passaggio di refactor sopra, non lavoro a sé stante).

## Decisione

Revisione del 2026-07-31: solo report, nessuna azione intrapresa in quel momento. Questo documento resta come riferimento per la prioritizzazione quando il lavoro riprenderà.
