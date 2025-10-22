$dbName = "lessonflow"
$dbUser = "test"
$dbHost = "localhost"
$dbPort = 5432
$dbPassword = "test"

$schemaFile = "./schema.sql"

Write-Host "Dropping and recreating database"

$env:PGPASSWORD = $dbPassword
& 'C:\Program Files\PostgreSQL\17\bin\dropdb.exe' -h $dbHost -p $dbPort -U $dbUser --if-exists $dbName
# & 'C:\Program Files\PostgreSQL\17\bin\createdb.exe' -h $dbHost -p $dbPort -U $dbUser $dbName

# Write-Host "Applying schema"
# & 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -h $dbHost -p $dbPort -U $dbUser -d $dbName -f $schemaFile

Write-Host "Starting API..."
$apiProcess = Start-Process "dotnet" -ArgumentList "run" -PassThru

$baseUrl = "http://localhost:5283"
$healthUrl = "$baseUrl/health"

Write-Host "Waiting for API to be ready..."

$maxAttempts = 30
$attempt = 0

$ready = $false

while (-not $ready -and $attempt -lt $maxAttempts) {
    try {
        $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 3
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ API is ready!"
            $ready = $true
        } else {
            Write-Host "Waiting... (HTTP $($response.StatusCode))"
        }
    } catch {
        Write-Host "Waiting... API not ready yet ($($attempt + 1)/$maxAttempts)"
    }

    if (-not $ready) {
        Start-Sleep -Seconds 2
    }

    $attempt++
}

if (-not $ready) {
    Write-Host "❌ API failed to start in time."
    Stop-Process -Id $apiProcess.Id -Force
    exit 1
}

Write-Host "Running migrations"
dotnet ef database update

function Post-TermDates {
    param(
        [int]$Year,
        [array]$Terms
    )

    $body = @{
        calendarYear = $Year
        termDates = $Terms
    } | ConvertTo-Json -Depth 5

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/api/dev/services/term-dates" `
            -Method POST `
            -Headers @{ "Content-Type" = "application/json" } `
            -Body $body
        Write-Host "✅ Term dates for $Year posted successfully."
    } catch {
    }
}

Post-TermDates -Year 2025 -Terms @(
    @{ termNumber = 1; startDate = "2025-01-27"; endDate = "2025-04-11" },
    @{ termNumber = 2; startDate = "2025-04-28"; endDate = "2025-07-04" },
    @{ termNumber = 3; startDate = "2025-07-21"; endDate = "2025-09-26" },
    @{ termNumber = 4; startDate = "2025-10-13"; endDate = "2025-12-12" }
)

Post-TermDates -Year 2026 -Terms @(
    @{ termNumber = 1; startDate = "2026-01-26"; endDate = "2026-04-10" },
    @{ termNumber = 2; startDate = "2026-04-27"; endDate = "2026-07-03" },
    @{ termNumber = 3; startDate = "2026-07-20"; endDate = "2026-09-25" },
    @{ termNumber = 4; startDate = "2026-10-12"; endDate = "2026-12-11" }
)

$curriculumPath = "C:\\Users\\craig\\source\\repos\\LessonFlow\\src\\LessonFlow.Api\\CurriculumFiles"

Write-Host "📤 Sending curriculum parse request..."
try {
    Invoke-RestMethod -Uri "$baseUrl/api/dev/services/parse-curriculum?directory=$curriculumPath" -Method POST
    Write-Host "✅ Curriculum parsing triggered."
} catch {
    Write-Host "❌ Failed to trigger curriculum parse: $($_.Exception.Message)"
}

Write-Host "Stopping API..."
Stop-Process -Id $apiProcess.Id -Force
Write-Host "✅ All done."
