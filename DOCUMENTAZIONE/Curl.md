# Auth

## Login

```bash
$TOKEN = (curl.exe -s -X POST http://localhost:5182/api/auth/login -H "Content-Type: application/json" -d '{\"userName\": \"admin\", \"password\": \"Admin123!\"}' | ConvertFrom-Json).token
```

## Logout

```bash
curl.exe -X POST http://localhost:5182/api/auth/logout -H "Authorization: Bearer $TOKEN" -i
```
