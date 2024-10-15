@echo off
REM dir:
REM /A          Отображение файлов с указанными атрибутами.
REM атрибуты    D  Каталоги.                    R  Файлы, доступные только для чтения
REM             H  Скрытые файлы                A  Файлы, готовые для архивирования
REM             S  Системные файлы              I  Файлы с неиндексированным содержимым
REM             L  Точки повторной обработки    O  Автономные файлы
REM             -  Префикс "-" имеет значение НЕ
REM /B          Вывод только имен файлов.
REM /S          Отображение файлов из указанного каталога и всех его подкаталогов.

for /f %%a in ('dir /S /B /A:D "bin" "obj" "ProjectEvaluation" "dataSources"') do rd /s /q %%a & echo %%a

rem  .vs/ProjectEvaluation, .idea/dataSources, rust-tests/target

pause