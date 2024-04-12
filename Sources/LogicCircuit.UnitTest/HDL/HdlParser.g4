parser grammar HdlParser;

options {
	tokenVocab = HdlLexer;
}

@header {
	#pragma warning disable 3021
}

chip: Chip chipName Begin inputPins? outputPins? Parts parts End;

chipName: Identifier;

inputPins: In pins;

outputPins: Out pins;

pins: ioPin (Comma ioPin)* Semicolon;

ioPin: pinName (OpenBit DecNumber CloseBit)?;

pinName: Identifier;

parts: (part Semicolon)+;

part: partName Open partConnections Close;

partName: Identifier;

partConnections: partConnection (Comma partConnection)*;

partConnection: jam Equal pin;

jam: jamName (OpenBit bits CloseBit)?;

jamName: Identifier;

bits: DecNumber (Elapse DecNumber)?;

pin: jam;
