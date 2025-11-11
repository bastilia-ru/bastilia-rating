<#
.Description
Migrates DB from SOURCE to DEST
#> 

function migrate([String]$source_conn_string, [String]$dest_conn_string)
{

@"
load database
    from $source_conn_string
    into $dest_conn_string

    WITH include drop, create tables, no truncate, create indexes, reset sequences, foreign keys;

"@ >migrate.load
    
    pgloader --no-ssl-cert-verification --on-error-stop --verbose /tmp/cmd/migrate.load
    Remove-Item .\migrate.load
}

function pgloader {
    $cur_dir = (Get-Item .).FullName
    Write-Output $mount_arg
    docker run --rm --name pgloader --mount "type=bind,source=$cur_dir,target=/tmp/cmd" -v "${env:APPDATA}\postgresql\pgpass.conf:/tmp/.pgpass:ro" -e PGPASSFILE=/tmp/.pgpass dimitri/pgloader:v3.6.7 pgloader $args
}
