# curl

## Auth

### Login (Admin)

```bash
$TOKEN = (curl.exe -s -X POST http://localhost:5182/api/auth/login -H "Content-Type: application/json" -d '{\"userName\": \"admin\", \"password\": \"Admin123!\"}' | ConvertFrom-Json).token
```

### Register

``` bash
curl.exe -s -X POST http://localhost:5182/api/auth/register -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" -d '{\"userName\": \"mrossi\", \"password\": \"Password123!\", \"firstName\": \"Mario\", \"lastName\": \"Rossi\"}' -i
```

### Login (User)

```bash
$TOKEN = (curl.exe -s -X POST http://localhost:5182/api/auth/login -H "Content-Type: application/json" -d '{\"userName\": \"mrossi\", \"password\": \"Password123!\"}' | ConvertFrom-Json).token
```

### Logout

```bash
curl.exe -X POST http://localhost:5182/api/auth/logout -H "Authorization: Bearer $TOKEN" -i
```

## Emoloyee

### Get

```bash
curl.exe -X GET http://localhost:5182/api/employee/ -H "Authorization: Bearer $TOKEN" -i
```

### Get{ID}

```bash
curl.exe -X GET http://localhost:5182/api/employee/{id} -H "Authorization: Bearer $TOKEN" -i
```

### PUT

```bash
curl.exe -X PUT http://localhost:5182/api/employee/977dad30-4e55-4128-9521-103debe97042 -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"username\": \"string\",\"firstName\": \"string\",\"lastName\": \"string\"}' -i
```

### DELETE

```bash
curl.exe -X DELETE http://localhost:5182/api/employee/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json"
```

## Company

### Get

```bash
curl.exe -X GET http://localhost:5182/api/Company/ -H "Authorization: Bearer $TOKEN" -i
```

### Get{ID}

```bash
curl.exe -X GET http://localhost:5182/api/Company/{11c1ca68-9425-4f2b-8f42-5473fa89ee7d} -H "Authorization: Bearer $TOKEN" -i
```

### POST

```bash
curl.exe -X POST http://localhost:5182/api/Company -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"Nome Azienda\", \"email\": \"info@azienda.com\"}' -i
```

### PUT

```bash
curl.exe -X PUT http://localhost:5182/api/Company/11c1ca68-9425-4f2b-8f42-5473fa89ee7d -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"Nome Azienda\", \"email\": \"info@azienda.com\"}' -i
```

### DELETE

```bash
curl.exe -X DELETE http://localhost:5182/api/Company/7f441c01-65dd-4151-80c0-1e38375c6d50 -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json"
```

## Type


### Get

```bash
curl.exe -X GET http://localhost:5182/api/type/ -H "Authorization: Bearer $TOKEN" -i
```

### Get{ID}

```bash
curl.exe -X GET http://localhost:5182/api/type/{id} -H "Authorization: Bearer $TOKEN" -i
```

### POST

```bash
curl.exe -X POST http://localhost:5182/api/type -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"Nome tipo\"}' -i
```

### PUT

```bash
curl.exe -X PUT http://localhost:5182/api/type/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"Nome Azienda\"}' -i
```

### DELETE

```bash
curl.exe -X DELETE http://localhost:5182/api/type/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json"
```

## Status

### Get

```bash
curl.exe -X GET http://localhost:5182/api/status/ -H "Authorization: Bearer $TOKEN" -i
```

### Get{ID}

```bash
curl.exe -X GET http://localhost:5182/api/status/{id} -H "Authorization: Bearer $TOKEN" -i
```

### POST

```bash
curl.exe -X POST http://localhost:5182/api/status -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"Nome tipo\"}' -i
```

### PUT

```bash
curl.exe -X PUT http://localhost:5182/api/status/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"Nome Azienda\"}' -i
```

### DELETE

```bash
curl.exe -X DELETE http://localhost:5182/api/status/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json"
```

## Project

### Get

```bash
curl.exe -X GET http://localhost:5182/api/project/ -H "Authorization: Bearer $TOKEN" -i
```

### Get{ID}

```bash
curl.exe -X GET http://localhost:5182/api/project/{id} -H "Authorization: Bearer $TOKEN" -i
```

### POST

```bash
 curl.exe -X POST http://localhost:5182/api/project -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"dioporco\",\"idCompany\":\"436329e0-d7e7-400c-8ce3-e616849909f8\" }' -i
```

### PUT

```bash
curl.exe -X PUT http://localhost:5182/api/project/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"name\": \"CENSURA\", \"idCompany\" : \"436329e0-d7e7-400c-8ce3-e616849909f8\"}' -i
```

### DELETE

```bash
curl.exe -X DELETE http://localhost:5182/api/project/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json"
```

## Worklog

> Endpoint unico `api/worklog`: il comportamento cambia in base al ruolo del `$TOKEN` usato.
> - Con token **Admin**: `Get` accetta i filtri `employeeId`, `projectId`, `dateFrom`, `dateTo`, `statusName` e opera su tutti i dipendenti; in `POST`/`PUT` `idEmployee` viene usato per davvero.
> - Con token **User**: `Get` accetta solo `dateFrom`/`dateTo` e restituisce/modifica solo i worklog del dipendente autenticato. Il body di `POST` richiede comunque un `idEmployee` valido (per via della validazione), ma il server lo **ignora** e usa sempre l'Oid del token.

### Get (Admin, con filtri)

```bash
curl.exe -X GET "http://localhost:5182/api/worklog?employeeId={id}&projectId={id}&dateFrom=2026-07-01&dateTo=2026-07-31&statusName=Approvato" -H "Authorization: Bearer $TOKEN" -i
```

### Get (User, solo proprie)

```bash
curl.exe -X GET "http://localhost:5182/api/worklog?dateFrom=2026-07-01&dateTo=2026-07-31" -H "Authorization: Bearer $TOKEN" -i
```

### Get{ID}

```bash
curl.exe -X GET http://localhost:5182/api/worklog/{id} -H "Authorization: Bearer $TOKEN" -i
```

### POST (Admin)

```bash
curl.exe -X POST http://localhost:5182/api/worklog -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"description\": \"SONO MARIO E SONO GAY\", \"hoursCounter\": 8, \"date\": \"2026-07-14\", \"idProject\": \"d0a3402a-a7fd-4a8c-aef5-26c3850a348e\", \"idEmployee\": \"3685f964-0a41-4a9a-883f-30317b961786\", \"idType\": \"5370d468-847a-4153-b0ce-fc2fc71c34c3\", \"idStatus\": \"c32e3dcc-777e-447c-96c9-a222230e0c58\"}' -i
```

### POST (User)

```bash
curl.exe -X POST http://localhost:5182/api/worklog -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"description\": \"Sviluppo API worklog\", \"hoursCounter\": 8, \"date\": \"2026-07-14\", \"idProject\": \"d0a3402a-a7fd-4a8c-aef5-26c3850a348e\", \"idType\": \"5370d468-847a-4153-b0ce-fc2fc71c34c3\", \"idStatus\": \"21fd64e2-7233-4186-baa3-8fc121f7d6ee\"}' -i
```

### PUT

```bash
curl.exe -X PUT http://localhost:5182/api/worklog/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json" -d '{\"description\": \"Aggiornamento worklog\", \"hoursCounter\": 6, \"date\": \"2026-07-14\", \"idProject\": \"436329e0-d7e7-400c-8ce3-e616849909f8\", \"idType\": \"{idType}\", \"idStatus\": \"{idStatus}\"}' -i
```

### DELETE

```bash
curl.exe -X DELETE http://localhost:5182/api/worklog/{id} -H "Authorization: Bearer $TOKEN" -H "accept: application/json" -H "Content-Type: application/json"
```

## Report Data

### GET Report Progetto (Admin)

```bash
curl.exe -X GET "http://localhost:5182/api/report-data/project/{projectId}?from=2026-01-01&to=2026-01-31" -H "Authorization: Bearer $TOKEN" -i
```

### GET Report Dipendente (Admin, id esplicito)

```bash
curl.exe -X GET "http://localhost:5182/api/report-data/employee/{employeeId}?from=2026-01-01&to=2026-01-31" -H "Authorization: Bearer $TOKEN" -i
```

### GET Report Dipendente (User, proprio report)

```bash
curl.exe -X GET "http://localhost:5182/api/report-data/employee?from=2026-01-01&to=2026-01-31" -H "Authorization: Bearer $TOKEN" -i
```

.