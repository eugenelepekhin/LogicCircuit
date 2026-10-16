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
- Circuit - name of circuit to test.
- Category - name of category. All circuits of the category will be tested.

