$env:LOGIN_DB_ENGINE = 'postgres'
$env:LOGIN_POSTGRES_DB = 'login_db'
$env:LOGIN_POSTGRES_USER = 'postgres'
$env:LOGIN_POSTGRES_HOST = 'localhost'
$env:LOGIN_POSTGRES_PORT = '5432'

# Secretos (password, SECRET_KEY) viven en local.env.ps1, que NO se sube al repo.
# Copia local.env.ps1.example a local.env.ps1 y pon ahi tus valores reales.
$localEnv = "$PSScriptRoot\local.env.ps1"
if (Test-Path $localEnv) {
    . $localEnv
} else {
    Write-Error "Falta $localEnv. Copia local.env.ps1.example a local.env.ps1 y completa tus credenciales."
    exit 1
}

Set-Location "$PSScriptRoot\login"
& "$PSScriptRoot\.venv\Scripts\python.exe" manage.py runserver 0.0.0.0:8001
