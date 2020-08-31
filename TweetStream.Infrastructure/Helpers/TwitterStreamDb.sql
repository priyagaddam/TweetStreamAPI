USE [master]
GO
/****** Object:  Database [TwitterStreamDb]    Script Date: 8/30/2020 6:53:53 PM ******/
CREATE DATABASE [TwitterStreamDb]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'TwitterStreamDb', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\TwitterStreamDb.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'TwitterStreamDb_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\TwitterStreamDb_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO


USE [TwitterStreamDb]
GO


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hashtag](
	[Tag] [nvarchar](1024) NULL,
	[TweetId] [varchar](1024) NULL,
	[CreatedAt] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tweet]    Script Date: 8/30/2020 6:53:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tweet](
	[Id] [varchar](128) NOT NULL,
	[Text] [nvarchar](1024) NULL,
	[CreatedAt] [datetime] NULL,
	[Language] [varchar](50) NULL,
	[AuthorId] [varchar](128) NULL,
 CONSTRAINT [PK_Tweet] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Url]    Script Date: 8/30/2020 6:53:53 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Url](
	[ExpandedUrl] [nvarchar](1024) NULL,
	[DisplayUrl] [nvarchar](1024) NULL,
	[TweetId] [varchar](1024) NULL,
	[CreatedAt] [datetime] NULL
) ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [TwitterStreamDb] SET  READ_WRITE 
GO