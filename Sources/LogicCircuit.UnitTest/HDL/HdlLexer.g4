lexer grammar HdlLexer;

@header {
	#pragma warning disable 3021
}

Chip: 'CHIP';
Begin: '{';
End: '}';
Parts: 'PARTS:';
In: 'IN';
Out: 'OUT';
Comma: ',';
Semicolon: ';';
Open: '(';
Close: ')';
Equal: '=';
OpenBit: '[';
CloseBit: ']';
Elapse: '..';

Identifier: Letter (Letter | DecDigit)*;

DecNumber: '0' | (NonzeroDigit DecDigit*);

fragment
Letter: [a-zA-Z];

fragment
DecDigit: [0-9];

fragment
NonzeroDigit: [1-9];

Whitespace: [ \t\u000C\r\n]+ -> skip;

BlockComment: '/*' .*? '*/' -> skip;
LineComment: '//' ~ [\r\n]* -> skip;
