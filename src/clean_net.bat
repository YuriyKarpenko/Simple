@echo off
REM dir:
REM /A          ����������� ������ � ���������� ����������.
REM ��������    D  ��������.                    R  �����, ��������� ������ ��� ������
REM             H  ������� �����                A  �����, ������� ��� �������������
REM             S  ��������� �����              I  ����� � ����������������� ����������
REM             L  ����� ��������� ���������    O  ���������� �����
REM             -  ������� "-" ����� �������� ��
REM /B          ����� ������ ���� ������.
REM /S          ����������� ������ �� ���������� �������� � ���� ��� ������������.

for /f %%a in ('dir /S /B /A:D "bin" "obj" "ProjectEvaluation" "dataSources"') do rd /s /q %%a & echo %%a

rem  .vs/ProjectEvaluation, .idea/dataSources, rust-tests/target

pause