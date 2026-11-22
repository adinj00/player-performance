# Player Performance

## Windows checkout for Claude skills

Claude discovers project skills through directory symlinks in `.claude/skills/`. Windows must allow Git to create symbolic links. Enable [Windows Developer Mode](https://learn.microsoft.com/en-us/windows/advanced-settings/developer-mode) or clone from a PowerShell session with the **Create symbolic links** privilege.

```powershell
git clone -c core.symlinks=true https://github.com/adinj00/player-performance.git
Set-Location player-performance
```

Verify that every tracked Claude skill is a symbolic link with a reachable `SKILL.md`.

```powershell
$skills = git ls-files -- .claude/skills
if ($LASTEXITCODE -ne 0) { throw 'Could not list tracked Claude skills' }
foreach ($skill in $skills) {
    $link = Get-Item -LiteralPath $skill -Force -ErrorAction Stop
    if ($link.LinkType -ne 'SymbolicLink' -or
        !(Test-Path -LiteralPath (Join-Path $link.FullName 'SKILL.md') -PathType Leaf)) {
        throw "Invalid Claude skill link: $skill"
    }
}
Get-Content -LiteralPath .claude/skills/check/SKILL.md -TotalCount 3 -ErrorAction Stop
```

The verification should finish without errors and show `name: check`. If the links were checked out as plain files, use the clone command above in a fresh directory.
