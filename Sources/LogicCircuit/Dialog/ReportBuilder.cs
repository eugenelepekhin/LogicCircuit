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
    
    #line 1 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "17.0.0.0")]
    public partial class ReportBuilder : T4Transformation
    {
#line hidden
        /// <summary>
        /// Create the template output
        /// </summary>
        public override string TransformText()
        {
            
            #line 3 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
this.ToStringHelper.EscapeXmlText = true;
            
            #line default
            #line hidden
            this.Write("<FlowDocument xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">\r" +
                    "\n\t<Paragraph FontSize=\"20\" FontWeight=\"Bold\">");
            
            #line 5 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.CommandCircuitProject));
            
            #line default
            #line hidden
            this.Write(" \"");
            
            #line 5 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Project.Name));
            
            #line default
            #line hidden
            this.Write("\"</Paragraph>\r\n\t<Paragraph><Bold>");
            
            #line 6 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.TitleProjectDescription));
            
            #line default
            #line hidden
            this.Write("</Bold> ");
            
            #line 6 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Project.Note));
            
            #line default
            #line hidden
            this.Write("</Paragraph>\r\n\t<Paragraph><Bold>");
            
            #line 7 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.TitleSummary));
            
            #line default
            #line hidden
            this.Write("</Bold>\r\n\t\t");
            
            #line 8 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.ProjectSummary(
			this.Project.CircuitProject.LogicalCircuitSet.Count(),
			this.CategoryCount,
			this.Project.CircuitProject.CircuitSymbolSet.Count(),
			this.Project.CircuitProject.WireSet.Count()
		)));
            
            #line default
            #line hidden
            this.Write("\r\n\t</Paragraph>\r\n\t<Paragraph FontSize=\"20\" FontWeight=\"Bold\">");
            
            #line 15 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.ReportFunctions(this.Root.Name)));
            
            #line default
            #line hidden
            this.Write("</Paragraph>\r\n");
            
            #line 16 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
if(this.BuildMapException == null) {
            
            #line default
            #line hidden
            this.Write("\t<Table CellSpacing=\"5\">\r\n\t\t<Table.Columns>\r\n\t\t\t<TableColumn/>\r\n\t\t\t<TableColumn/>" +
                    "\r\n\t\t</Table.Columns>\r\n\t\t<TableRowGroup>\r\n\t\t\t<TableRow Background=\"Gray\">\r\n\t\t\t\t<T" +
                    "ableCell><Paragraph FontSize=\"15\" FontWeight=\"Bold\">");
            
            #line 24 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.TitleFunction));
            
            #line default
            #line hidden
            this.Write("</Paragraph></TableCell>\r\n\t\t\t\t<TableCell><Paragraph FontSize=\"15\" FontWeight=\"Bol" +
                    "d\">");
            
            #line 25 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.TitleCount));
            
            #line default
            #line hidden
            this.Write("</Paragraph></TableCell>\r\n\t\t\t</TableRow>\r\n");
            
            #line 27 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
	for(int i = 0; i < this.Functions.Count; i++) {
            
            #line default
            #line hidden
            this.Write("\t\t\t<TableRow Background=\"");
            
            #line 28 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(((i & 1) == 0) ? "White" : "WhiteSmoke"));
            
            #line default
            #line hidden
            this.Write("\">\r\n\t\t\t\t<TableCell><Paragraph>");
            
            #line 29 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Functions[i]));
            
            #line default
            #line hidden
            this.Write("</Paragraph></TableCell>\r\n\t\t\t\t<TableCell><Paragraph>");
            
            #line 30 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Usage[this.Functions[i]]));
            
            #line default
            #line hidden
            this.Write("</Paragraph></TableCell>\r\n\t\t\t</TableRow>\r\n");
            
            #line 32 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
	}
            
            #line default
            #line hidden
            this.Write("\t\t\t<TableRow Background=\"Gray\">\r\n\t\t\t\t<TableCell><Paragraph FontWeight=\"Bold\">");
            
            #line 34 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.TitleTotal));
            
            #line default
            #line hidden
            this.Write("</Paragraph></TableCell>\r\n\t\t\t\t<TableCell><Paragraph>");
            
            #line 35 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Usage.Values.Sum()));
            
            #line default
            #line hidden
            this.Write("</Paragraph></TableCell>\r\n\t\t\t</TableRow>\r\n\t\t</TableRowGroup>\r\n\t</Table>\r\n");
            
            #line 39 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
} else {
            
            #line default
            #line hidden
            this.Write("\t<Paragraph FontSize=\"20\" FontWeight=\"Bold\">");
            
            #line 40 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.ReportError(this.BuildMapException.Message)));
            
            #line default
            #line hidden
            this.Write("</Paragraph>\r\n\t<Paragraph FontSize=\"8\" FontWeight=\"Bold\">");
            
            #line 41 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(Properties.Resources.TitleReportErrorDetails));
            
            #line default
            #line hidden
            this.Write("</Paragraph>\r\n\t<Paragraph FontSize=\"8\">");
            
            #line 42 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.BuildMapException.ToString()));
            
            #line default
            #line hidden
            this.Write("</Paragraph>\r\n");
            
            #line 43 "C:\Projects\LogicCircuit\LogicCircuit\master.core.hdl\Sources\LogicCircuit\Dialog\ReportBuilder.tt"
}
            
            #line default
            #line hidden
            this.Write("</FlowDocument>\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
