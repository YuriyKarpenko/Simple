@echo off
for /f %%a in ('dir /s /b /a:D "bin" "obj" "ProjectEvaluation" "dataSources" "target"') do rd /s /q %%a & echo %%a

rem  .vs/ProjectEvaluation, .idea/dataSources, rust-tests/target

pause