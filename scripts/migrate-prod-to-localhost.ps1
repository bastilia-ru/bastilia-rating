<#
.Description
Migrates DB from DEV PostgreSQL to localhost
#> 

[CmdletBinding()]
param(
    [Parameter(Position = 0, Mandatory = $true)]
    [String]$postgre_pass
)

. $PSScriptRoot\pgloader.ps1

$source = "pgsql://bastilia-rating@rc1b-1omkout6a9ifyold.mdb.yandexcloud.net:6432/BastiliaRating?sslmode=allow"
$dest = "pgsql://postgres_user:qwerty@host.docker.internal/BastiliaRating?sslmode=disable"

migrate $source $dest