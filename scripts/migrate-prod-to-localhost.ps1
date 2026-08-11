<#
.Description
Migrates DB from DEV PostgreSQL to localhost
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $false)]
    [String]$postgre_pass
)

. $PSScriptRoot\pgloader.ps1

$pgHost = "rc1b-1omkout6a9ifyold.mdb.yandexcloud.net"
$pgPort = 6432
$pgUser = "bastilia-rating"
$pgpassPath = "$env:APPDATA\postgresql\pgpass.conf"
$pgpassEntry = "${pgHost}:${pgPort}:*:${pgUser}"

$hasPgpassEntry = (Test-Path $pgpassPath -PathType Leaf) -and
    (Select-String -Path $pgpassPath -Pattern ([regex]::Escape($pgpassEntry)) -Quiet)

if (-not $hasPgpassEntry) {
    if (-not $postgre_pass) {
        $securePass = Read-Host -Prompt "pgpass.conf для $pgUser@$pgHost не найден, введите пароль" -AsSecureString
        $postgre_pass = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
            [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePass))
    }

    if (Test-Path $pgpassPath -PathType Container) {
        Remove-Item $pgpassPath -Force -Recurse
    }

    $pgpassDir = Split-Path $pgpassPath -Parent
    if (-not (Test-Path $pgpassDir)) {
        New-Item -ItemType Directory -Path $pgpassDir -Force | Out-Null
    }

    $escapedPass = $postgre_pass -replace '\\', '\\\\' -replace ':', '\:'
    Add-Content -Path $pgpassPath -Value "${pgpassEntry}:${escapedPass}"
}

$source = "pgsql://$pgUser@${pgHost}:${pgPort}/BastiliaRating?sslmode=allow"
$dest = "pgsql://postgres_user:qwerty@host.docker.internal:6432/BastiliaRating?sslmode=disable"

migrate $source $dest
