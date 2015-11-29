Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$sourceFolder = $PSScriptRoot
$testSiteFolder = Join-Path $PSScriptRoot "testsite"
$pluginsFolder = Join-Path $testSiteFolder "_plugins"
$pretzelExe = Join-Path $PSScriptRoot "libs\Pretzel\Pretzel.exe"

function CreateEmptyFolder($path) {
    if (Test-Path $path) {
        Remove-Item $path -recurse -Force
        Start-Sleep -milliseconds 100
    }
    New-Item $path -type directory | Out-Null
}
function Assert($expected, $actual) {
    if ($expected -ne $actual) {
        Write-Error "`$expected was $expected, but `$actual was $actual"
    }
}

CreateEmptyFolder $testSiteFolder

New-Item $pluginsFolder -type directory | Out-Null

Push-Location $testSiteFolder
try {
    # create a default site and delete sitemap template
    & $pretzelExe create
    Remove-Item "sitemap.xml"

    # without the plugin there should be no sitemap.xml
    & $pretzelExe bake | Out-Null
    if (Test-Path "_site\sitemap.xml") {
        Write-Error "sitemap.xml was created without the Pretzel.Sitemap.csx plugin."
    }

    # the default _config.yml does not specify "url", so the plugin must fail with an error message
    Copy-Item ( Join-Path $sourceFolder "Pretzel.Sitemap.csx") $pluginsFolder
    $output = & $pretzelExe bake
    if (!($output -contains "You must specify ""url"" in _config.yml to you the Pretzel.Sitemap.csx plugin."))
    {
        Write-Error 'Expected error message about missing "url" parameter'
    }
    if (Test-Path "_site\sitemap.xml") {
        Write-Error "sitemap.xml was created without the Pretzel.Sitemap.csx plugin."
    }

    # add "url" to config
    Add-Content "_config.yml" "`n`nurl: http://example.org"

    & $pretzelExe bake

    if (!(Test-Path "_site\sitemap.xml")) {
        Write-Error "sitemap.xml was created without the Pretzel.Sitemap.csx plugin."
    }

    $sitemap = [xml](Get-Content "_site\sitemap.xml")

    Assert "http://example.org/$(Get-Date -Format 'yyyy\/MM\/dd')/myfirstpost.html" $sitemap.urlset.url[0].loc
    Assert 0.8 $sitemap.urlset.url[0].priority
    Assert "http://example.org/about.html" $sitemap.urlset.url[1].loc
    Assert 0.7 $sitemap.urlset.url[1].priority
    Assert "http://example.org/atom.xml" $sitemap.urlset.url[2].loc
    Assert 0.7 $sitemap.urlset.url[2].priority
    Assert "http://example.org/" $sitemap.urlset.url[3].loc
    Assert 1 $sitemap.urlset.url[3].priority
    Assert "http://example.org/rss.xml" $sitemap.urlset.url[4].loc
    Assert 0.7 $sitemap.urlset.url[4].priority

} finally {
    Pop-Location
}