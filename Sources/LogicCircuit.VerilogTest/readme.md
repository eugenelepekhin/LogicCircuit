# LogicCircuit.VerilogTest
The project is intended to test export of circuits to Verilog HDL.

For simple way of testing follow these steps:
- Open Developer Command Prompt for VS.
- Navigate to LogicCircuit solution folder.
- For testing
	- one circuit in LogicCircuit.UnitTest\Properties\VerilogTests.CircuitProject run:
	```
	msbuild -t:RunVerilogTests LogicCircuit.VerilogTest -p:Circuit=<Circuit name>
	```
	- one circuit in any other file run:
	```
	msbuild -t:RunVerilogTests LogicCircuit.VerilogTest -p:CircuitProject=<file> -p:Circuit=<Circuit name>
	```
	- all circuit in category
	```
	msbuild -t:RunVerilogTests LogicCircuit.VerilogTest -p:Category=<Category name>
	```

All the available options to run it from msbuild:
- Root - Folder where all the exports go.
- CircuitProject - file to run tests against.
- Verilator - path to root folder of the Verilator installation.
- Circuit - Semicolon separated list of individual circuit names patterns.
- Category - Semicolon separated list of patterns for names of categories.

Useful links:
Verilator Windows:
https://github.com/withlimon/verilator-windows

Good tutorial: https://www.chipverify.com/

Another one: https://www.asic-world.com/

And another one: http://www.emmelmann.org/Pages/Library_TutorialsWS.html

Online test bench. Run generated code there: https://www.edaplayground.com/
