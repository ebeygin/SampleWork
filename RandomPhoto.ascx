<%@ Control Language="C#" AutoEventWireup="true" Inherits="Components_RandomPhoto" Codebehind="RandomPhoto.ascx.cs" %>
<%@ Register src="SocialIcons2.ascx" tagname="SocialIcons2" tagprefix="uc1" %>

    <script type="text/javascript">
       var curLang = '<%=(_oSM.CurrentLangType==GV.English?"en-US":"es_MX") %>';
       var curDomain = '<%=Request.Url.Scheme +"://"+ Request.Url.Host  +":"+Request.Url.Port %>'; 
   </script>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#slider').show();
            $('#slider').bxSlider({
                onBeforeSlide: function (currentSlide, totalSlides, obj) {
                    ParseSlide(currentSlide);
                }
            });
            ParseSlide(0);
        });

        function ParseSlide(currentSlide) {
            var combinedID = $("#imageSlide_" + currentSlide + " img").attr("rel");
            var arrID = combinedID.split("|");
           
            var CurrentAlbumNo = arrID[0];
            var CurrentPhotoNo = arrID[1];
            var CurrentUID = arrID[2];

            var CurrentUrl = curDomain + "/PhotoAlbum/PhotoView.aspx?PhotoNo=" + CurrentPhotoNo + "&AlbumNo=" + CurrentAlbumNo + "&UID=" + CurrentUID + "&lang=" + (curLang == "en-US" ? "en" : "es");

            $('#like').html('<fb:like href="' + CurrentUrl + '" layout="button_count" show_faces="false" style="overflow:visible;height:36px;top:2px;" width="' + (curLang == "en-US" ? "85" : "110") + '" ref="fbLikemoj" />');
            if (typeof FB !== 'undefined') {
                FB.XFBML.parse(document.getElementById('#like'));
            }

            loadSharedUrl(arrID);
        }
    </script>
 
    <div class="mojSection marginT12 random-container-shaded-700px">
    <a href="#" title="<%=GetLocalResourceObject("_tip_Album")%>" class="clueTipTitle question-mark">
        <img alt="" src="/images/icon-titlebar-question-mark.png" />
    </a>
    <h2 style="margin: 7px 0 -8px 5px;"><asp:Label ID="lblAlbumTitle" runat="server"></asp:Label></h2>

  <div class="rpgSecContent padding-bottom-5" style="padding-left: 30px">
            <uc1:SocialIcons2 ID="SocialIcons1" runat="server" />
            <asp:Repeater runat="server" ID="rpMyAlbum" OnItemDataBound="rpMyAlbum_ItemDataBound">
            <HeaderTemplate><ul id="slider" style="display:none;"></HeaderTemplate>
            <ItemTemplate>
                <li>
                    <div class="rpContainer">
                        <div class="rpContent">
                            <div class="rpContentImg" id="imageSlide_<%# DataBinder.Eval(Container, "ItemIndex", "")%>">
                            <a class="RPGPhoto" href="<%# GetCommentURL(Eval("AlbumNo"), Eval("PhotoNo"), Eval("UID")) %>"><asp:Image ID="lnkViewPhotos_imgThumb" runat="server" rel='<%# Eval("AlbumNo") +"|"+ Eval("PhotoNo") +"|"+ Eval("UID")+"|"+Eval("Title")+"|photo" %>' ImageUrl='<%# GetImageToDisplay(Eval("BucketName"), Eval("ThumbKey")) %>' /></a>
                            <img class="thumbnail-eyeball" src='<%# ((bool)Eval("HelpRequested")) ? "../../images/thumbnail-100px-eyeball-orange.png" : "../../images/thumbnail-100px-eyeball-hidden.png" %>' />
					        </div>
                            <div class="rpContentTxt">
						        <div class="rpPhotoName"><%=GetLocalResourceObject("_photoName")%><br /><a class="RPGPhotoTitle" href="<%# GetCommentURL(Eval("AlbumNo"), Eval("PhotoNo"), Eval("UID")) %>"><asp:Label ID="lblAlbumName" runat="server" Text=<%# ObjectHelper.Truncate(Eval("Title").ToString(), 25, true) %> ></asp:Label></a></div>
						        <div class="rpFavCom"><span><%# DataBinder.Eval(Container.DataItem, "FavouritesCount")%> <%=GetLocalResourceObject("_favorite")%></span> | <span><%# DataBinder.Eval(Container.DataItem, "CommentsCount")%> <%=GetLocalResourceObject("_comment")%></span><br /><a class="RPGComment" href="<%# GetCommentURL(Eval("AlbumNo"), Eval("PhotoNo"), Eval("UID")) %>"><%=GetLocalResourceObject("_commentLink")%></a></div>
                                <div class="rpViewMember"><asp:Label ID="lblMemberName" runat="server" /></div>
                                <div class="rpRefresh"><a href=""><%=GetLocalResourceObject("_refresh")%></a></div>
                                <asp:Label ID="lblUID" runat="server" Visible="false" Text='<%# Eval("UID") %>' />
					        </div>	
                        </div>
				    </div>
                </li>
            </ItemTemplate>
            <FooterTemplate></ul></FooterTemplate>
        </asp:Repeater>
     <div class="noteIcon"><%=GetLocalResourceObject("_eyeballInfo")%></div>

    </div>
    </div>
  