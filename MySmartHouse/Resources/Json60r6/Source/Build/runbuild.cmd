cls
powershell -Command "& { [Console]::WindowWidth = 150; [Console]::WindowHeight = 50; Start-Transcript runbuild.txt; Import-Module ..\Tools\PSake\psake.psm1; Invoke-psake ..\Build\build.ps1 %*; Stop-Transcript; }"

pause