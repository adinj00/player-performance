$ErrorActionPreference = "Stop"

# Official project-local skills installer for Windows PowerShell.
# Run from anywhere inside the repository.

$repoRoot = (& git rev-parse --show-toplevel).Trim()
if (-not $repoRoot) {
    throw "Nije pronađen Git repository root."
}

Set-Location $repoRoot
$env:DISABLE_TELEMETRY = "1"

Write-Host "Repository root: $repoRoot"
Write-Host "Branch: $((& git branch --show-current).Trim())"

$status = & git status --porcelain
if ($status) {
    Write-Warning "Working tree nije čist. Pregledaj git status prije commita."
}

New-Item -ItemType Directory -Force -Path ".agents\skills" | Out-Null

function Install-OfficialSkill {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [string[]]$SkillNames
    )

    Write-Host ""
    Write-Host "Installing official skill source: $Source"

    $arguments = @(
        "--yes",
        "skills@latest",
        "add",
        $Source
    )

    foreach ($skillName in $SkillNames) {
        $arguments += @("--skill", $skillName)
    }

    $arguments += @(
        "--agent", "codex",
        "--copy",
        "--yes"
    )

    & npx @arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Instalacija nije uspjela za: $Source"
    }
}

$dotnetSkillSources = @(
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet/skills/setup-local-sdk",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-aspnetcore/skills/configuring-opentelemetry-dotnet",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-aspnetcore/skills/convert-blazor-server-to-webapp",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-aspnetcore/skills/dotnet-webapi",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-aspnetcore/skills/minimal-api-file-upload",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-data/skills/optimizing-ef-core-queries",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-nuget/skills/convert-to-cpm",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/assertion-quality",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/code-testing-agent",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/code-testing-extensions",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/coverage-analysis",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/crap-score",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/detect-static-dependencies",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/filter-syntax",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/find-untested-sources",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/generate-testability-wrappers",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/grade-tests",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/migrate-static-to-wrapper",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/mtp-hot-reload",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/platform-detection",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/run-tests",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/test-analysis-extensions",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/test-anti-patterns",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/test-gap-analysis",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/test-smell-detection",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/test-tagging",
    "https://github.com/dotnet/skills/tree/main/plugins/dotnet-test/skills/writing-mstest-tests"
)

foreach ($source in $dotnetSkillSources) {
    Install-OfficialSkill -Source $source
}

Install-OfficialSkill `
    -Source "https://github.com/shadcn/ui" `
    -SkillNames @("shadcn")

Install-OfficialSkill `
    -Source "https://github.com/vercel-labs/agent-skills" `
    -SkillNames @(
        "vercel-react-best-practices",
        "vercel-composition-patterns",
        "web-design-guidelines"
    )

Install-OfficialSkill `
    -Source "https://github.com/anthropics/skills" `
    -SkillNames @("frontend-design")

Write-Host ""
Write-Host "Installed project skills:"
& npx --yes skills@latest list --agent codex

Write-Host ""
Write-Host "Detected SKILL.md files:"
Get-ChildItem -Path ".agents\skills" -Filter "SKILL.md" -Recurse -File |
    Sort-Object FullName |
    ForEach-Object {
        Resolve-Path -Relative $_.FullName
    }

Write-Host ""
Write-Host "Git ignore check for shadcn skill:"
& git check-ignore -v ".agents/skills/shadcn/SKILL.md"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Nije ignorisan ili fajl još nije pronađen kroz git check-ignore."
}

Write-Host ""
Write-Host "Current frontend-local skill files:"
if (Test-Path "frontend\.agents") {
    Get-ChildItem -Path "frontend\.agents" -Recurse -File |
        Sort-Object FullName |
        ForEach-Object {
            Resolve-Path -Relative $_.FullName
        }
}
else {
    Write-Host "frontend\.agents ne postoji."
}

Write-Host ""
Write-Host "Instalacija završena."
Write-Host "Nemoj još brisati frontend\.agents."
Write-Host "Prvo uporedi stari shadcn skill sa .agents\skills\shadcn."
