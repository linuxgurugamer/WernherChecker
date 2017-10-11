
set H=R:\KSP_1.3.1_dev
echo %H%

copy WernherChecker\bin\Debug\WernherChecker.dll GameData\WernherChecker\Plugins
xcopy /E /Y /I GameData\WernherChecker %H%\GameData\WernherChecker
