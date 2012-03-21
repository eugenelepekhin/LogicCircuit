<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:old="http://LogicCircuit.net/1.0.0.3/CircuitProject.xsd"
	xmlns:lc ="http://LogicCircuit.net/2.0.0.1/CircuitProject.xsd"
	exclude-result-prefixes="old"
	version="1.0"
>
	<xsl:strip-space elements="*"/>
		<xsl:output method="xml" version="1.0" standalone="yes" indent="yes"/>
	<xsl:key name="splitters" match="old:Splitter" use="old:SplitterId/text()"/>

	<!-- This will just change namespace of the file, so no new files will be opened by old CircuitProject -->

	<xsl:template match="*">
		<xsl:element name="{name(.)}">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

	<xsl:template match="old:Splitter/old:Rotation">
		<xsl:if test=". = 'Left' or . = 'Down'">
			<lc:Clockwise>True</lc:Clockwise>
		</xsl:if>
	</xsl:template>

	<xsl:template match="old:CircuitSymbol">
		<xsl:variable name="circuit" select="key('splitters', old:CircuitId)" />
		<xsl:element name="{name(.)}">
			<xsl:choose>
				<xsl:when test="$circuit">
					<xsl:call-template name="TranslateSplitterSymbol">
						<xsl:with-param name="circuit" select="$circuit" />
					</xsl:call-template>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:element>
	</xsl:template>

	<xsl:template name="TranslateSplitterSymbol">
		<xsl:param name="circuit" />

		<!-- output unchanging nodes -->
		<xsl:apply-templates select="old:CircuitSymbolId" />
		<xsl:apply-templates select="old:CircuitId" />
		<xsl:apply-templates select="old:LogicalCircuitId" />

		<!-- query and output Rotation node -->
		<xsl:variable name="rotation" select="$circuit/old:Rotation"/>

		<!-- adjust symbol position depending on Rotation and PinCount -->
		<xsl:variable name="X" select="./old:X" />
		<xsl:variable name="Y" select="./old:Y" />
		<xsl:variable name="pins" select='$circuit/old:PinCount'/>

		<xsl:choose>
			<xsl:when test="$rotation = 'Up' or $rotation = 'Down'" >
				<lc:Rotation>Left</lc:Rotation>
				<!-- X += ($pins+1) / 2; Y -= $pins / 2; -->
				<lc:X><xsl:value-of select="$X + floor(($pins+1) div 2)"/></lc:X>
				<lc:Y><xsl:value-of select="$Y - floor(($pins+0) div 2)"/></lc:Y>
			</xsl:when>
			<xsl:otherwise>
				<lc:X><xsl:value-of select="$X"/></lc:X>
				<lc:Y><xsl:value-of select="$Y"/></lc:Y>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>
