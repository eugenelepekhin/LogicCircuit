parser grammar TruthTableFilterParser;

options {
	tokenVocab = TruthTableFilterLexer;
}

filter: function* expr EOF;

function: Identifier parameters? Colon expr;

parameters: Open Identifier (Comma Identifier)* Close;

expr: Open expr Close					#ParenExpr
	| (Add | Not) expr					#Unary
	| left=expr op=BitShift right=expr	#Bin
	| left=expr op=BitAnd right=expr	#Bin
	| left=expr op=BitXor right=expr	#Bin
	| left=expr op=BitOr right=expr		#Bin
	| left=expr op=Mul right=expr		#Bin
	| left=expr op=Add right=expr		#Bin
	| left=expr op=Compare right=expr	#Bin
	| left=expr op=Equality right=expr	#Bin
	| left=expr op=BoolAnd right=expr	#Bin
	| left=expr op=BoolOr right=expr	#Bin
	| NumberLiteral						#Literal
	| Identifier						#Variable	// this case is variable or function call without parameters
	| String							#Variable
	| functionCall						#Call
;

// This is syntax of function calls with parameters.
// Functions calls without parameters are taken care in variable visitor
functionCall: Identifier Open expr (Comma expr)* Close;
