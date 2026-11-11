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
- VerilatorWarnings - Semicolon separated list of Verilator warnings to turn off.
- Circuit - Semicolon separated list of individual circuit names patterns.
- Category - Semicolon separated list of patterns for names of categories.
- TestBench - Path to test bench file for single circuit.


To build verilator on Windows, follow these steps:
- Install Python 3
- Download Flex and Bison from https://github.com/lexxmark/winflexbison/releases to a folder, e.g. C:\Tools\win_flex_bison-2.5.25
- git clone https://github.com/verilator/verilator
- git checkout stable
- to build run: msbuild LogicCircuit.VerilogTest -t:BuildVerilator


Useful links:
Good tutorial: https://www.chipverify.com/

Another one: https://www.asic-world.com/

And another one: http://www.emmelmann.org/Pages/Library_TutorialsWS.html

Online test bench. Run generated code there: https://www.edaplayground.com/
