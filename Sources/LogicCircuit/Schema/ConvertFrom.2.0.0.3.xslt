<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:old="http://LogicCircuit.net/2.0.0.3/CircuitProject.xsd"
	xmlns:lc ="http://LogicCircuit.net/2.0.0.4/CircuitProject.xsd"
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

	<!-- Converts all Odd functions for XOR -->
	<xsl:template match="old:CircuitSymbol/old:CircuitId[starts-with(., '00000000-0000-0000-0000-00000006')]">
		<xsl:element name="{name(.)}">
			<xsl:value-of select="concat('00000000-0000-0000-0000-00000005', substring(., 33))"/>
		</xsl:element>
	</xsl:template>

	<!-- Converts all Even functions for XOR NOT -->
	<xsl:template match="old:CircuitSymbol/old:CircuitId[starts-with(., '00000000-0000-0000-0000-00000007')]">
		<xsl:element name="{name(.)}">
			<xsl:value-of select="concat('00000000-0000-0000-0000-00000005', substring(., 33, 2), '01')"/>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>
