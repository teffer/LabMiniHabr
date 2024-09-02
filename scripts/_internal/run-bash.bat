@echo off
setlocal
set "paths[0]=%BASH_EXE_PATH%"
set "paths[1]=C:\portable-tortoise-git\PortableGit\bin\bash.exe"
set "paths[2]=C:\Program Files\Git\bin\bash.exe"
for /F "tokens=1* delims==" %%I in ('set paths[ 2^>nul') do (
	if exist "%%J" (
		"%%J" -- %*
		exit /b
	)
)
echo bash.exe not found, install Git for Windows to 'C:\Program Files\Git', or to another folder but then set BASH_EXE_PATH variable 1>&2
exit /b 1
endlocal
