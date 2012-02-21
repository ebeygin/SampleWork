<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library"
  exclude-result-prefixes="msxml umbraco.library">

<xsl:output method="html" omit-xml-declaration="yes"/>
   
<xsl:param name="currentPage" />
   
<!--This sets the level that the nav starts at and tells us if we should recurse through child elements-->
<xsl:variable name="startDepth" select="/macro/startingLevel" />
<xsl:variable name="recurse" select="/macro/recurse" />
<xsl:variable name="selectBranches" select="/macro/selectBranches"></xsl:variable>
<xsl:variable name="maxMenuDepth" select="/macro/maxMenuDepth"></xsl:variable>
<xsl:variable name="walkChildren" select="/macro/expandChildren"></xsl:variable>
<xsl:variable name="forceHome" select="/macro/forceHome"></xsl:variable>
<xsl:variable name="securityTrimming" select="/macro/securityTrimming"></xsl:variable>
<!--Alternate page title variable in here-->
  
<!--Styles for the navigation-->
<xsl:variable name="ulBaseClass" select="/macro/ulBaseClass"></xsl:variable>
<xsl:variable name="branchClass" select="/macro/branchClass"></xsl:variable>
<xsl:variable name="selectedClass" select="/macro/selectedClass"></xsl:variable>

<xsl:variable name="startLevel">
   <xsl:value-of select="$startDepth"/>
</xsl:variable>

  <!--This calls first iteration of the navigation, sending the first node at the correct depth found in the ancestors of the current page-->
<xsl:template match="/"> 
    <xsl:variable name="currentNode" select="$currentPage"></xsl:variable>
    <xsl:call-template name="nodeIterator">
      <xsl:with-param name="parentNode" select="$currentNode/ancestor-or-self::*[@isDoc][@level=$startLevel]
                      [
                        string(umbracoNaviHide) != '1'
                        and ($securityTrimming != '1'
                          or umbraco.library:IsProtected(@id, @path) = false()
                          or umbraco.library:HasAccess(@id, @path) = true())
                      ]" />
      <xsl:with-param name="pseudoCurrentPage" select="$currentNode" />
    </xsl:call-template>
</xsl:template>

<xsl:template name="nodeIterator">
    <xsl:param name="parentNode" />
    <xsl:param name="pseudoCurrentPage" />
    <!-- do not show info doc node types-->
  <xsl:variable name="calculatedMenuDepth" select="($parentNode/@level - $startLevel)+1" />

  <xsl:if test="$parentNode/*[@isDoc]">
    <ul>
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="$calculatedMenuDepth = 1">
            <xsl:value-of select="concat($ulBaseClass, ' lv', $calculatedMenuDepth)" />
          </xsl:when>
          <xsl:when test="$calculatedMenuDepth > 1">
            <xsl:value-of select="concat('lv', $calculatedMenuDepth)" />
          </xsl:when>
        </xsl:choose>
      </xsl:attribute>
      
      <!--for each node in the parent node that is not hidden by Umbraco-->
      <xsl:for-each select="$parentNode/*[@isDoc][
                          string(umbracoNaviHide) != '1'
                          and ($securityTrimming != '1'
                            or umbraco.library:IsProtected(@id, @path) = false()
                            or umbraco.library:HasAccess(@id, @path) = true())
                        ]">

        <!--Set the current node id i.e. the node we have looped to not the current page-->
        <xsl:variable name="currentNodeID" select="@id" />

        <!--Is the node a branch? i.e. are there children and is it in the colletion of ancestor nodes -->
        <xsl:variable name="isBranch">
          <xsl:choose>
            <xsl:when test="$currentPage/ancestor-or-self::*[@isDoc][@id = $currentNodeID]/child::*[@isDoc]">1</xsl:when>
          </xsl:choose>
        </xsl:variable>

        <!--Is the node selected? i.e. is it the same as the currentPage node-->
        <xsl:variable name="isSelected">
          <xsl:choose>
            <xsl:when test="$currentPage/@id = $currentNodeID">1</xsl:when>
            <!-- parent selected -->
            <xsl:when test="$pseudoCurrentPage/@id = $currentNodeID">1</xsl:when>
             <!-- If an article is selected, the artile's category must be selected too -->
             <xsl:when test="$currentPage/ancestor::* [@level=4]/@id = $currentNodeID">1</xsl:when>
          </xsl:choose>          
        </xsl:variable>

        <xsl:variable name="isSelectedBranch">
          <xsl:choose>
            <xsl:when test="$isBranch = 1 and $selectBranches = 1">1</xsl:when>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="hasChildren">
          <xsl:choose>
            <xsl:when test="./*[@isDoc]">1</xsl:when>
          </xsl:choose>
        </xsl:variable>

        <li>
          <!-- Create the class attribute for the element-->
          <xsl:call-template name="cssClassConstructor">
            <xsl:with-param name="isSelected" select="$isSelected" />
            <xsl:with-param name="isSelectedBranch" select="$isSelectedBranch" />
            <xsl:with-param name="hasChildren" select="$hasChildren" />
            <xsl:with-param name="selectedClass" select="$selectedClass" />
            <xsl:with-param name="branchClass" select="$branchClass" />
          </xsl:call-template>

          <div class="secondNavContainer"><a href="{umbraco.library:NiceUrl(@id)}">

            <xsl:call-template name="cssClassConstructor">
              <xsl:with-param name="isSelected" select="$isSelected" />
              <xsl:with-param name="isSelectedBranch" select="$isSelectedBranch" />
              <xsl:with-param name="hasChildren" select="0" />
              <xsl:with-param name="selectedClass" select="$selectedClass" />
              <xsl:with-param name="branchClass" select="$branchClass" />
            </xsl:call-template>

            <!--set the innerText for the a element-->
            <xsl:value-of select="./pageTitle/text()"/>
            <xsl:if test="string(./pageTitle/text()) = ''">
              <xsl:value-of select="@nodeName"/>
            </xsl:if>
          </a></div>
  
          <!-- if it's a branch recurse through it's children-->
          <!-- Original code
          <xsl:if test="((($isBranch = 1 and $recurse = 1) or ($walkChildren = 1 and $pseudoCurrentPage/descendant-or-self::*[@isDoc][@id = $currentNodeID]/child::*[@isDoc])) and $maxMenuDepth &gt; $calculatedMenuDepth)">
          -->
          <!-- Don't just do children for current page, but for all) -->
          <xsl:if test="($isBranch = 1 and $recurse = 1 or $walkChildren = 1) and $maxMenuDepth &gt; $calculatedMenuDepth">
            <xsl:call-template name="nodeIterator">
              <xsl:with-param name="parentNode" select="." />
              <xsl:with-param name="pseudoCurrentPage" select="$pseudoCurrentPage" />
            </xsl:call-template>
          </xsl:if>

        </li>

      </xsl:for-each>

    </ul>
  </xsl:if> 
  </xsl:template>
 
  <xsl:template name="cssClassConstructor">
    <xsl:param name="isSelected"></xsl:param>
    <xsl:param name="isSelectedBranch"></xsl:param>
    <xsl:param name="hasChildren"></xsl:param>
    <xsl:param name="selectedClass"></xsl:param>
    <xsl:param name="branchClass"></xsl:param>

    <xsl:variable name="class">
      <xsl:if test="$isSelected = 1">
        <xsl:value-of select="concat($selectedClass,' ')"/>
      </xsl:if>
      <xsl:if test="$isSelectedBranch = 1">
        <xsl:value-of select="concat($branchClass,' ')"/>
      </xsl:if>
      <xsl:if test="$hasChildren = 1">
        <xsl:value-of select="'hasChildren '"/>
      </xsl:if>
    </xsl:variable>

    <xsl:if test="string-length($class) > 0">
      <xsl:attribute name="class">
        <xsl:value-of select="normalize-space($class)"/>
      </xsl:attribute>
    </xsl:if>
    
  </xsl:template>
</xsl:stylesheet>