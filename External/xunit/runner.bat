@echo OFF

mkdir tests
for /l %%i in (1,1,500) do (
	echo =============================
	echo =============================
	echo Running test %%i of 500
	echo =============================
	echo =============================

	REM xunit.console.clr4.exe ../../Forge.UtilitiesTests/bin/Release/Forge.Utilities.Tests.dll /html tests/run%%i.html
	xunit.console.clr4.exe ../../Forge.EntitiesTests/bin/Release/Forge.Entities.Tests.dll /html tests/run%%i.html
	if not errorlevel 1 (
		del "tests\run%%i.html"
	) else (
		echo Repeat number %%i errored
        )
)