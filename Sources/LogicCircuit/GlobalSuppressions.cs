// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the
// Error List, point to "Suppress Message(s)", and click
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "jpe", Scope = "resource", Target = "LogicCircuit.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "png", Scope = "resource", Target = "LogicCircuit.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "tif", Scope = "resource", Target = "LogicCircuit.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1701:ResourceStringCompoundWordsShouldBeCasedCorrectly", MessageId = "NOr", Scope = "resource", Target = "LogicCircuit.Properties.Resources.resources")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LogicCircuit")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LogicCircuit", Scope = "namespace", Target = "LogicCircuit.DataPersistent")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "IdLed", Scope = "member", Target = "LogicCircuit.LedMatrix.#.ctor(LogicCircuit.CircuitProject,LogicCircuit.DataPersistent.RowId,LogicCircuit.DataPersistent.RowId)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "rowId", Scope = "member", Target = "LogicCircuit.CircuitSet.#Create(LogicCircuit.DataPersistent.RowId)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "store", Scope = "member", Target = "LogicCircuit.CircuitData.#CreateForeignKeys(LogicCircuit.DataPersistent.StoreSnapshot)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "store", Scope = "member", Target = "LogicCircuit.CollapsedCategoryData.#CreateForeignKeys(LogicCircuit.DataPersistent.StoreSnapshot)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "LogicCircuit.CircuitSet.#CreateItem(System.Guid)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "LogicCircuit.ProjectSet.#CreateItem(System.Guid,System.String,System.String,System.Double,System.Int32,System.Boolean,LogicCircuit.LogicalCircuit,System.Boolean,System.Boolean,System.Boolean)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Scope = "member", Target = "LogicCircuit.CircuitSet.#Create(LogicCircuit.DataPersistent.RowId)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "LogicCircuit.CircuitProject.#StoreVersionChanged(System.Object,LogicCircuit.DataPersistent.VersionChangeEventArgs)")]
