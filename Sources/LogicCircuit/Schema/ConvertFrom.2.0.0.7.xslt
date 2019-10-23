<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:old="http://LogicCircuit.net/2.0.0.7/CircuitProject.xsd"
	xmlns:lc ="http://LogicCircuit.net/2.0.0.8/CircuitProject.xsd"
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

	<!-- This will rename all Description fields to Note on all LogicalCircuits and Project -->
	<xsl:template match="old:Description">
		<lc:Note><xsl:value-of select="."/></lc:Note>
	</xsl:template>

</xsl:stylesheet>
