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
