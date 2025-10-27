param(
  [Parameter(Mandatory=$true)][string]$Ref
)

# Extract tag name from ref (e.g., refs/tags/v1.2.3 → v1.2.3)
$tagName = ($Ref -split '/')[-1]

# Remove leading 'v' if present
$versionPrefix = if ($tagName.StartsWith('v')) { $tagName.Substring(1) } else { $tagName }

# Strip any prerelease suffix from tag name
if ($versionPrefix -match '-') {
  $versionPrefix = $versionPrefix.Split('-')[0]
}

# Resolve commit SHA from tag ref
$Sha = git rev-parse $Ref

# Find remote branches that contain the tag commit
$remoteBranches = @(git for-each-ref --format='%(refname:short)' --contains $Sha refs/remotes)

# Normalize branch names
$branches = $remoteBranches | ForEach-Object {
  $b = $_.Trim()
  if ($b -match '^origin/') { $b = $b.Substring(7) }
  if ($b -match '^remotes/') { $b = $b -replace '^remotes/[^/]+/','' }
  $b
} | Where-Object { $_ -ne '' } | Select-Object -Unique

# Prefer main, then develop
$preferred = @('main','develop')
$selectedBranch = $preferred | Where-Object { $branches -contains $_ } | Select-Object -First 1

if (-not $selectedBranch) {
  throw "Tag commit is present on branches: $($branches -join ', '). Only 'main' or 'develop' are supported for release creation"
}

$branch = $selectedBranch
Write-Host "Branch is $branch."

# Derive suffix
switch ($branch) {
  'main'    { $versionSuffix = '' }
  'develop' { $versionSuffix = 'beta' }
  default   { throw "Branch $branch is not supported for release creation" }
}

Write-Host "Computed VersionPrefix: $versionPrefix"
Write-Host "Computed VersionSuffix: $versionSuffix"

# Return structured result
return @{
  VersionPrefix = $versionPrefix
  VersionSuffix = $versionSuffix
  Branch        = $branch
}
