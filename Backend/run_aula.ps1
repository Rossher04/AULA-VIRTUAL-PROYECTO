$env:AULA_DB_ENGINE = 'postgres'
$env:AULA_POSTGRES_DB = 'aula_virtual_academica'
$env:AULA_POSTGRES_USER = 'postgres'
$env:AULA_POSTGRES_HOST = 'localhost'
$env:AULA_POSTGRES_PORT = '5432'

# Secretos (password, SECRET_KEY) viven en local.env.ps1, que NO se sube al repo.
# Copia local.env.ps1.example a local.env.ps1 y pon ahi tus valores reales.
$localEnv = "$PSScriptRoot\local.env.ps1"
if (Test-Path $localEnv) {
    . $localEnv
} else {
    Write-Error "Falta $localEnv. Copia local.env.ps1.example a local.env.ps1 y completa tus credenciales."
    exit 1
}

Set-Location "$PSScriptRoot\aula"
& "$PSScriptRoot\.venv\Scripts\python.exe" manage.py runserver 0.0.0.0:8002
