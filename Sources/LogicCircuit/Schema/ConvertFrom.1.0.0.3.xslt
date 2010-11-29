<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:old="http://LogicCircuit.net/1.0.0.3/CircuitProject.xsd"
	xmlns:lc ="http://LogicCircuit.net/1.0.0.4/CircuitProject.xsd"
	exclude-result-prefixes="old"
	version="1.0"
>
	<xsl:output method="xml" version="1.0" standalone="yes" indent="yes"/>

	<!-- This will just change namespace of the file, so no new files will be opened by old ProjectManager -->

	<xsl:template match="*">
		<xsl:element name="{name(.)}">
			<xsl:apply-templates select="node()"/>
		</xsl:element>
	</xsl:template>

</xsl:stylesheet>
