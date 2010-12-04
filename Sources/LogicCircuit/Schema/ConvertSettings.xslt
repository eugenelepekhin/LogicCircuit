<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<xsl:stylesheet
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:old="http://LogicCircuit.net/SettingsData.xsd"
	xmlns:lcs ="http://LogicCircuit.net/Settings/Data.xsd"
	exclude-result-prefixes="old"
	version="1.0"
>
	<xsl:output method="xml" version="1.0" standalone="yes" indent="yes"/>

	<xsl:template match="/old:Settings">
		<lcs:settings>
			<xsl:for-each select="old:Settings">
				<lcs:property>
					<xsl:attribute name="name">
						<xsl:choose>
							<xsl:when test="old:Key = 'MainFrame.Width'">Mainframe.Width</xsl:when>
							<xsl:when test="old:Key = 'MainFrame.Height'">Mainframe.Height</xsl:when>
							<xsl:when test="old:Key = 'MainFrame.WindowState'">Mainframe.WindowState</xsl:when>
							<xsl:otherwise><xsl:value-of select="old:Key"/></xsl:otherwise>
						</xsl:choose>
					</xsl:attribute>
					<xsl:value-of select="old:Value"/>
				</lcs:property>
			</xsl:for-each>
			<xsl:for-each select="old:RecentFile">
				<lcs:file>
					<xsl:attribute name="name"><xsl:value-of select="old:FileName"/></xsl:attribute>
					<xsl:attribute name="date"><xsl:value-of select="old:DateTime"/></xsl:attribute>
				</lcs:file>
			</xsl:for-each>
		</lcs:settings>
	</xsl:template>
</xsl:stylesheet>
