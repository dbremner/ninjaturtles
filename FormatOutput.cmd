@echo off
copy /y NinjaTurtles.Tests\bin\Debug\SampleReport.xml .
msxsl SampleReport.xml ReportXslt.xslt -o SampleOutput.html
del NinjaTurtles.Tests\bin\Debug\SampleReport.xml