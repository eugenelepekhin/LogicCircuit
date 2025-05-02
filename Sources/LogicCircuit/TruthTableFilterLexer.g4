lexer grammar TruthTableFilterLexer;

Open: '(';
Close: ')';
Colon: ':';
Comma: ',';

Add: [+-];
Mul: [*/%];
Not: [!~];

BoolAnd: '&&';
BoolOr: '||';

BitAnd: '&';
BitOr: '|';
BitXor: '^';
BitShift: '<<' | '>>';

Equality: '==' | '=' | '!=' | '<>';
Compare: '<' | '<=' | '>=' | '>';

NumberLiteral: BinNumber | HexNumber | OctaNumber | DecNumber;
String: '"' ('\\"' | .)*? '"';

Identifier: Letter (Letter | DecDigit)*;

fragment
Letter: [_a-zA-Z];

fragment
BinNumber: ('0b' | '0B') BinDigit (NumberDecorator? BinDigit)*;

fragment
HexNumber: ('0x' | '0X') HexDigit (NumberDecorator? HexDigit)*;

fragment
OctaNumber: '0' (NumberDecorator? OctaDigit)*;

fragment
DecNumber: NonzeroDigit (NumberDecorator? DecDigit)*;

fragment
BinDigit: [01];

fragment
OctaDigit: [0-7];

fragment
HexDigit: [0-9a-fA-F];

fragment
DecDigit: [0-9];

fragment
NonzeroDigit: [1-9];

fragment
NumberDecorator: [_'];

Whitespace: [ \t\u000C\r\n]+ -> skip;
BlockComment: '/*' .*? '*/' -> skip;
