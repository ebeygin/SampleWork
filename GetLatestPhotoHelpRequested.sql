IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GC_GetLatestPhotoHelpRequested]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GC_GetLatestPhotoHelpRequested]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GC_GetLatestPhotoHelpRequested]
AS
BEGIN
	SET NOCOUNT ON;
		
	DECLARE @tv_LastUploadedPhoto TABLE (
		PhotoNo INT,
		DateCreated DATETIME
	)
	
	DECLARE @photoNo INT
	SET @photoNo = -1
	
	INSERT INTO @tv_LastUploadedPhoto
		SELECT a.[PhotoNo], a.[DateCreated]  FROM GCPhoto a 
		INNER JOIN GCRegistrant b ON a.UID = b.uID
		INNER JOIN GCPhotoAlbum c ON a.AlbumNo = c.AlbumNo  
		WHERE 
			a.IsPrivate=0 AND a.IsApproved=1 AND  
			b.IsPrivate=0 AND c.IsPrivate=0 AND
			(CONVERT(char(10), a.[DateCreated], 101)) > (CONVERT(char(10), GETDATE()-10, 101))
			ORDER BY a.DateCreated DESC
	
	IF((SELECT COUNT(x.PhotoNo) FROM @tv_LastUploadedPhoto x)>10)
	BEGIN
		SET @photoNo = (SELECT TOP 1 x.PhotoNo FROM @tv_LastUploadedPhoto x ORDER BY NEWID())
	END
	ELSE
	BEGIN
		DELETE FROM @tv_LastUploadedPhoto

		INSERT INTO @tv_LastUploadedPhoto
			SELECT TOP 10 a.[PhotoNo], a.[DateCreated]  FROM GCPhoto a  
			INNER JOIN GCRegistrant b ON a.UID = b.uID
			INNER JOIN GCPhotoAlbum c ON a.AlbumNo = c.AlbumNo  
			WHERE 
				a.IsPrivate=0 AND a.IsApproved=1 AND 
				b.IsPrivate=0 AND c.IsPrivate=0
			ORDER BY a.DateCreated DESC

		SET @photoNo = (SELECT TOP 1 x.PhotoNo FROM @tv_LastUploadedPhoto x ORDER BY NEWID())
	END
	
	SELECT a.[UID], b.[FirstName], b.[LastName], b.[ScreenName], a.[AlbumNo], a.[PhotoNo], a.[BucketName], a.[PhotoKey], a.[ThumbKey], a.[Title], a.[Description] 
	FROM GCPhoto a INNER JOIN GCRegistrant b ON a.UID = b.uID 
	WHERE a.PhotoNo=@photoNo
END