﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 17.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace LogicCircuit
{
    using System.Linq;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    
    #line 1 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class N2THdl : HdlTransformation
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            this.Write("CHIP ");
            
            #line 3 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Name));
            
            #line default
            #line hidden
            this.Write(" {\r\n");
            
            #line 4 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
if(this.HasInputPins) {
            
            #line default
            #line hidden
            this.Write("\tIN ");
            
            #line 5 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(PinsText(this.InputPins)));
            
            #line default
            #line hidden
            this.Write(";\r\n");
            
            #line 6 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
}
            
            #line default
            #line hidden
            
            #line 7 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
if(this.HasOutputPins) {
            
            #line default
            #line hidden
            this.Write("\tOUT ");
            
            #line 8 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(PinsText(this.OutputPins)));
            
            #line default
            #line hidden
            this.Write(";\r\n");
            
            #line 9 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
}
            
            #line default
            #line hidden
            this.Write("PARTS:\r\n");
            
            #line 11 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
foreach(HdlSymbol symbol in this.Parts) {
	bool comma = false;

            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 14 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
if(this.CommentPoints) {
            
            #line default
            #line hidden
            this.Write("\t// ");
            
            #line 15 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(symbol.CircuitSymbol.Circuit.Name));
            
            #line default
            #line hidden
            
            #line 15 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(symbol.CircuitSymbol.Point));
            
            #line default
            #line hidden
            this.Write("\r\n\t");
            
            #line 16 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
}
            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 17 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(symbol.Name));
            
            #line default
            #line hidden
            this.Write("(");
            
            #line 17 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
foreach(HdlConnection connection in symbol.HdlConnections().Where(c => c.GenerateOutput(symbol))) {
            
            #line default
            #line hidden
            
            #line 17 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(comma ? ", " : ""));
            
            #line default
            #line hidden
            
            #line 17 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(connection.SymbolJamName(symbol)));
            
            #line default
            #line hidden
            this.Write("=");
            
            #line 17 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(connection.PinName(symbol)));
            
            #line default
            #line hidden
            
            #line 17 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
comma = true;
            
            #line default
            #line hidden
            
            #line 17 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
}
            
            #line default
            #line hidden
            this.Write(");\r\n");
            
            #line 18 "C:\Projects\LogicCircuit\LogicCircuit\master.hdl\Sources\LogicCircuit\HDL\N2THdl.tt"
}
            
            #line default
            #line hidden
            this.Write("}\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
