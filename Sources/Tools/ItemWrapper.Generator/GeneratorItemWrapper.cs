﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 10.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace ItemWrapper.Generator
{
    using System.Collections.Generic;
    using System;
    
    
    #line 1 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "10.0.0.0")]
    public partial class GeneratorItemWrapper : Transformation
    {
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
        public override string TransformText()
        {
            this.GenerationEnvironment = null;
            this.Write("\t// Class wrapper for a record.\r\n");
            
            #line 4 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"

	string itemClassModifier = string.Empty;
	string itemCtorModifier = "public";
	string itemCtorParamList = ", RowId " + this.Table.Name + "RowId";
	string itemCtorBaseCall = string.Empty;
	string inheritFrom = "INotifyPropertyChanged";
	bool generateNotifyPropertyChanged = true;
	switch(this.Table.ItemModifier) {
	case ItemModifier.Abstract:
		itemClassModifier = "abstract ";
		itemCtorModifier = "protected";
		break;
	}
	bool isSubclass = this.Table.IsSubclass();
	if(isSubclass) {
		foreach(Table parent in this.Table.Ancestors(false)) {
			itemCtorParamList += ", RowId " + parent.Name + "RowId";
			itemCtorBaseCall += ", " + parent.Name + "RowId";
		}
		itemCtorBaseCall = " : base(store" + itemCtorBaseCall + ")";
		inheritFrom = this.Table.BaseName();
		generateNotifyPropertyChanged = false;
	}
	if(this.Table.ItemBaseClass != null) {
		inheritFrom = this.Table.ItemBaseClass;
		generateNotifyPropertyChanged = false;
	}

            
            #line default
            #line hidden
            this.Write("\t");
            
            #line 32 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(itemClassModifier));
            
            #line default
            #line hidden
            this.Write("partial class ");
            
            #line 32 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write(" : ");
            
            #line 32 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(inheritFrom));
            
            #line default
            #line hidden
            this.Write(" {\r\n\r\n\t\t// RowId of the wrapped record\r\n\t\tinternal RowId ");
            
            #line 35 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId { get; private set; }\r\n");
            
            #line 36 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
if(!isSubclass) {
            
            #line default
            #line hidden
            this.Write("\t\t// Store this wrapper belongs to\r\n\t\tpublic ");
            
            #line 38 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Store.Name));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 38 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Store.Name));
            
            #line default
            #line hidden
            this.Write(" { get; private set; }\r\n");
            
            #line 39 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n\t\t// Constructor\r\n\t\t");
            
            #line 42 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(itemCtorModifier));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 42 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("(");
            
            #line 42 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Store.Name));
            
            #line default
            #line hidden
            this.Write(" store");
            
            #line 42 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(itemCtorParamList));
            
            #line default
            #line hidden
            this.Write(")");
            
            #line 42 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(itemCtorBaseCall));
            
            #line default
            #line hidden
            this.Write(" {\r\n\t\t\tDebug.Assert(!");
            
            #line 43 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId.IsEmpty);\r\n");
            
            #line 44 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
if(!isSubclass) {
            
            #line default
            #line hidden
            this.Write("\t\t\tthis.");
            
            #line 45 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Store.Name));
            
            #line default
            #line hidden
            this.Write(" = store;\r\n");
            
            #line 46 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
}
            
            #line default
            #line hidden
            this.Write("\t\t\tthis.");
            
            #line 47 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId = ");
            
            #line 47 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId;\r\n");
            
            #line 48 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
if(this.RealmType == RealmType.Universe) {
            
            #line default
            #line hidden
            this.Write("\t\t\t// Link back to record. Assuming that a transaction is started\r\n\t\t\tthis.Table." +
                    "SetField(this.");
            
            #line 50 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId, ");
            
            #line 50 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data.");
            
            #line 50 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Field.Field, this);\r\n");
            
            #line 51 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
}
            
            #line default
            #line hidden
            this.Write("\t\t\tthis.Initialize");
            
            #line 52 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("();\r\n\t\t}\r\n\r\n\t\tpartial void Initialize");
            
            #line 55 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("();\r\n\r\n\t\t// Gets table storing this item.\r\n\t\tprivate TableSnapshot<");
            
            #line 58 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data> Table { get { return this.");
            
            #line 58 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Store.Name));
            
            #line default
            #line hidden
            this.Write(".");
            
            #line 58 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Set.Table; } }\r\n\r\n");
            
            #line 60 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
if(!isSubclass) {
            
            #line default
            #line hidden
            this.Write("\t\t// Deletes object\r\n\t\tpublic virtual void Delete() {\r\n\t\t\tthis.Table.Delete(this." +
                    "");
            
            #line 63 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId);\r\n\t\t}\r\n\r\n\t\t// Checks if the item is deleted\r\n\t\tpublic bool IsDeleted() {\r\n" +
                    "\t\t\treturn this.Table.IsDeleted(this.");
            
            #line 68 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId);\r\n\t\t}\r\n");
            
            #line 70 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n\t\t//Properties of ");
            
            #line 72 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("\r\n\r\n");
            
            #line 74 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"

foreach(Column column in this.Table.Columns) {
	string access = column.AccessModifierName() + (column.PropertyOverrides ? " override" : string.Empty);
	Key key = this.Table.ForeignKey(column);
	if(key != null && !this.Table.IsPrimary(column)) {
		Table parent = key.Parent();
		Key primaryKey = parent.PrimaryKey();
		string parentId = (primaryKey.KeyType == KeyType.Auto) ? parent.Name + "RowId" : primaryKey[0].Name;
		string findSuffix = (primaryKey.KeyType == KeyType.Auto || !parent.IsSubclass()) ? string.Empty : "By" + parentId;
		string returnType = parent.Name;
		string cast = "";
		if(!string.IsNullOrEmpty(key.PropertyType)) {
			returnType = key.PropertyType;
			cast = "(" + returnType + ")";
		}

            
            #line default
            #line hidden
            this.Write("\t\t// Gets ");
            
            #line 90 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.ReadOnly ? "" : "or sets "));
            
            #line default
            #line hidden
            this.Write("the value reffered by the foreign key on field ");
            
            #line 90 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write("\r\n\t\t");
            
            #line 91 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(access));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 91 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(returnType));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 91 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.PropertyNamePrefix));
            
            #line default
            #line hidden
            
            #line 91 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(key.RoleName()));
            
            #line default
            #line hidden
            this.Write(" {\r\n\t\t\tget { return ");
            
            #line 92 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(cast));
            
            #line default
            #line hidden
            this.Write("this.");
            
            #line 92 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Store.Name));
            
            #line default
            #line hidden
            this.Write(".");
            
            #line 92 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(parent.Name));
            
            #line default
            #line hidden
            this.Write("Set.Find");
            
            #line 92 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(findSuffix));
            
            #line default
            #line hidden
            this.Write("(this.Table.GetField(this.");
            
            #line 92 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId, ");
            
            #line 92 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data.");
            
            #line 92 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write("Field.Field)); }\r\n");
            
            #line 93 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
		if(!column.ReadOnly) {
            
            #line default
            #line hidden
            this.Write("\t\t\tset { this.Table.SetField(this.");
            
            #line 94 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId, ");
            
            #line 94 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data.");
            
            #line 94 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write("Field.Field, value.");
            
            #line 94 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(parentId));
            
            #line default
            #line hidden
            this.Write("); }\r\n");
            
            #line 95 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
		}
            
            #line default
            #line hidden
            this.Write("\t\t}\r\n");
            
            #line 97 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"

	} else {

            
            #line default
            #line hidden
            this.Write("\t\t// Gets ");
            
            #line 100 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.ReadOnly ? "" : "or sets "));
            
            #line default
            #line hidden
            this.Write("value of the ");
            
            #line 100 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write(" field.\r\n\t\t");
            
            #line 101 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(access));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 101 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Type));
            
            #line default
            #line hidden
            this.Write(" ");
            
            #line 101 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.PropertyNamePrefix));
            
            #line default
            #line hidden
            
            #line 101 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write(" {\r\n\t\t\tget { return this.Table.GetField(this.");
            
            #line 102 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId, ");
            
            #line 102 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data.");
            
            #line 102 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write("Field.Field); }\r\n");
            
            #line 103 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
		if(!column.ReadOnly) {
            
            #line default
            #line hidden
            this.Write("\t\t\tset { this.Table.SetField(this.");
            
            #line 104 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("RowId, ");
            
            #line 104 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data.");
            
            #line 104 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write("Field.Field, value); }\r\n");
            
            #line 105 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
		}
            
            #line default
            #line hidden
            this.Write("\t\t}\r\n");
            
            #line 107 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"

	}

            
            #line default
            #line hidden
            this.Write("\r\n");
            
            #line 111 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
}
            
            #line default
            #line hidden
            
            #line 112 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
if(generateNotifyPropertyChanged) {
            
            #line default
            #line hidden
            this.Write("\t\tpublic event PropertyChangedEventHandler PropertyChanged;\r\n\r\n\t\tprotected void N" +
                    "otifyPropertyChanged(string name) {\r\n\t\t\tPropertyChangedEventHandler handler = th" +
                    "is.PropertyChanged;\r\n\t\t\tif(handler != null) {\r\n");
            
            #line 118 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
	if(this.UseDispatcher) {
            
            #line default
            #line hidden
            this.Write("\t\t\t\tSystem.Windows.Threading.Dispatcher dispatcher = this.");
            
            #line 119 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Store.Name));
            
            #line default
            #line hidden
            this.Write(".Dispatcher;\r\n\t\t\t\tif(dispatcher != null && dispatcher.Thread != System.Threading." +
                    "Thread.CurrentThread) {\r\n\t\t\t\t\tdispatcher.Invoke(new Action<string>(this.NotifyPr" +
                    "opertyChanged), name);\r\n\t\t\t\t\treturn;\r\n\t\t\t\t}\r\n");
            
            #line 124 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
	}
            
            #line default
            #line hidden
            this.Write("\t\t\t\thandler(this, new PropertyChangedEventArgs(name));\r\n\t\t\t}\r\n\t\t}\r\n\r\n\t\tprotected " +
                    "bool HasListener { get { return this.PropertyChanged != null; } }\r\n");
            
            #line 130 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
}
            
            #line default
            #line hidden
            this.Write("\r\n\t\tinternal void NotifyChanged(TableChange<");
            
            #line 132 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data> change) {\r\n\t\t\tif(this.HasListener) {\r\n\t\t\t\t");
            
            #line 134 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data oldData, newData;\r\n\t\t\t\tchange.GetOldData(out oldData);\r\n\t\t\t\tchange.GetNewDat" +
                    "a(out newData);\r\n");
            
            #line 137 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"

foreach(Column column in this.Table.Columns) {
	Key key = this.Table.ForeignKey(column);
	string propertyName = column.Name;
	if(key != null && !this.Table.IsPrimary(column)) {
		propertyName = key.RoleName();
	}

            
            #line default
            #line hidden
            this.Write("\t\t\t\tif(");
            
            #line 145 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Data.");
            
            #line 145 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(column.Name));
            
            #line default
            #line hidden
            this.Write("Field.Field.Compare(ref oldData, ref newData) != 0) {\r\n\t\t\t\t\tthis.NotifyPropertyCh" +
                    "anged(\"");
            
            #line 146 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(propertyName));
            
            #line default
            #line hidden
            this.Write("\");\r\n\t\t\t\t}\r\n");
            
            #line 148 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
}
            
            #line default
            #line hidden
            this.Write("\t\t\t}\r\n\t\t\tthis.On");
            
            #line 150 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Changed();\r\n\t\t}\r\n\r\n\t\tpartial void On");
            
            #line 153 "E:\Projects\SnapData\Tools\ItemWrapper.Generator\GeneratorItemWrapper.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(this.Table.Name));
            
            #line default
            #line hidden
            this.Write("Changed();\r\n\t}\r\n");
            return this.GenerationEnvironment.ToString();
        }
    }
    
    #line default
    #line hidden
}
