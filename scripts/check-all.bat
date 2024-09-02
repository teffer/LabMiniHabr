@echo off
call "%~dp0_internal/run-bash.bat" "%~dp0%~n0" %* || if /i %0 == "%~0" pause
exit /b %ERRORLEVEL%
