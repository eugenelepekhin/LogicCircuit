<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:old="http://LogicCircuit.net/2.0.0.5/CircuitProject.xsd"
	xmlns:lc ="http://LogicCircuit.net/2.0.0.6/CircuitProject.xsd"
	exclude-result-prefixes="old"
	version="1.0"
>
	<xsl:output method="xml" version="1.0" standalone="yes" indent="yes"/>

	<!-- This will just change namespace of the file, so no new files will be opened by old CircuitProject -->

	<xsl:template match="*">
		<xsl:element name="{name(.)}">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<!-- Convert Gate Probe to separate circuit -->
	<xsl:template match="old:CircuitSymbol[starts-with(old:CircuitId, '00000000-0000-0000-0000-00000009')]">
		<!-- Lets reuse CircuitSymbolId for: CircuitProbeId, Name, and a new CircuitSymbolId -->
		<xsl:variable name="newId" select="old:CircuitSymbolId"/>
		<lc:CircuitProbe>
			<lc:CircuitProbeId><xsl:value-of select="$newId"/></lc:CircuitProbeId>
			<lc:Name><xsl:value-of select="$newId"/></lc:Name>
		</lc:CircuitProbe>
		<lc:CircuitSymbol>
			<lc:CircuitSymbolId><xsl:value-of select="$newId"/></lc:CircuitSymbolId>
			<lc:CircuitId><xsl:value-of select="$newId"/></lc:CircuitId>
			<lc:LogicalCircuitId><xsl:value-of select="old:LogicalCircuitId"/></lc:LogicalCircuitId>
			<lc:X><xsl:value-of select="old:X"/></lc:X>
			<lc:Y><xsl:value-of select="old:Y"/></lc:Y>
		</lc:CircuitSymbol>
	</xsl:template>
</xsl:stylesheet>
