-- MySQL dump 10.13  Distrib 8.0.22, for Win64 (x86_64)
--
-- Host: localhost    Database: Painless
-- ------------------------------------------------------
-- Server version	8.0.22

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `Accounts`
--

DROP TABLE IF EXISTS `Accounts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Accounts` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `Guid` varchar(45) NOT NULL,
  `OwnerId` int NOT NULL DEFAULT '0',
  `Domain` varchar(100) NOT NULL,
  `Enabled` int NOT NULL DEFAULT '1',
  `Deleted` int NOT NULL DEFAULT '0',
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `enabled` (`Enabled`) /*!80000 INVISIBLE */,
  KEY `deleted` (`Deleted`) /*!80000 INVISIBLE */,
  KEY `guid` (`Guid`) /*!80000 INVISIBLE */,
  KEY `domain` (`Domain`),
  KEY `created` (`Created`),
  KEY `ownerId` (`OwnerId`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Devices`
--

DROP TABLE IF EXISTS `Devices`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Devices` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `Guid` varchar(45) NOT NULL,
  `AccountId` int NOT NULL DEFAULT '0',
  `EventId` int NOT NULL DEFAULT '0',
  `Pin` int NOT NULL DEFAULT '0',
  `Name` varchar(50) NOT NULL,
  `Location` varchar(100) NOT NULL,
  `Version` int NOT NULL DEFAULT '0',
  `LastLogon` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `Enabled` int NOT NULL DEFAULT '1',
  `Deleted` int NOT NULL DEFAULT '0',
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `guid` (`Guid`) /*!80000 INVISIBLE */,
  KEY `accountId` (`AccountId`) /*!80000 INVISIBLE */,
  KEY `eventId` (`EventId`) /*!80000 INVISIBLE */,
  KEY `pin` (`Pin`) /*!80000 INVISIBLE */,
  KEY `lastLogon` (`LastLogon`) /*!80000 INVISIBLE */,
  KEY `enabled` (`Enabled`) /*!80000 INVISIBLE */,
  KEY `deleted` (`Deleted`) /*!80000 INVISIBLE */,
  KEY `created` (`Created`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Events`
--

DROP TABLE IF EXISTS `Events`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Events` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `Guid` varchar(45) NOT NULL,
  `AccountId` int NOT NULL DEFAULT '0',
  `Url` varchar(100) NOT NULL,
  `Title` varchar(300) NOT NULL,
  `Subtitle` varchar(300) NOT NULL,
  `TimeZone` varchar(100) NOT NULL,
  `Enabled` int NOT NULL DEFAULT '1',
  `Deleted` int NOT NULL DEFAULT '0',
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `guid` (`Guid`) /*!80000 INVISIBLE */,
  KEY `accountId` (`AccountId`) /*!80000 INVISIBLE */,
  KEY `url` (`Url`),
  KEY `enabled` (`Enabled`) /*!80000 INVISIBLE */,
  KEY `deleted` (`Deleted`) /*!80000 INVISIBLE */,
  KEY `created` (`Created`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `FileDownloads`
--

DROP TABLE IF EXISTS `FileDownloads`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `FileDownloads` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL DEFAULT '0',
  `FileId` int NOT NULL DEFAULT '0',
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `userId` (`UserId`) /*!80000 INVISIBLE */,
  KEY `fileId` (`FileId`) /*!80000 INVISIBLE */,
  KEY `created` (`Created`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Files`
--

DROP TABLE IF EXISTS `Files`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Files` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Guid` varchar(45) NOT NULL,
  `AccountId` int NOT NULL DEFAULT '0',
  `EventId` int NOT NULL DEFAULT '0',
  `Name` varchar(300) NOT NULL,
  `Extension` varchar(20) NOT NULL,
  `Size` int NOT NULL,
  `Description` varchar(1000) NOT NULL,
  `UploadedBy` int NOT NULL DEFAULT '0',
  `AdminAccess` int NOT NULL DEFAULT '1',
  `UserAccess` int NOT NULL DEFAULT '0',
  `VisitorAccess` int NOT NULL DEFAULT '0',
  `Enabled` int NOT NULL DEFAULT '1',
  `Deleted` int NOT NULL DEFAULT '0',
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `accountId` (`AccountId`) /*!80000 INVISIBLE */,
  KEY `eventId` (`EventId`) /*!80000 INVISIBLE */,
  KEY `name` (`Name`),
  KEY `extension` (`Extension`) /*!80000 INVISIBLE */,
  KEY `size` (`Size`) /*!80000 INVISIBLE */,
  KEY `enabled` (`Enabled`) /*!80000 INVISIBLE */,
  KEY `deleted` (`Deleted`) /*!80000 INVISIBLE */,
  KEY `created` (`Created`) /*!80000 INVISIBLE */,
  KEY `uploadedBy` (`UploadedBy`),
  KEY `adminAccess` (`AdminAccess`),
  KEY `userAccess` (`UserAccess`),
  KEY `visitorAccess` (`VisitorAccess`),
  KEY `guid` (`Guid`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Nodes`
--

DROP TABLE IF EXISTS `Nodes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Nodes` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `Domain` varchar(150) NOT NULL,
  `IsActive` int NOT NULL DEFAULT '1',
  `Latitude` double NOT NULL DEFAULT '0',
  `Longitude` double NOT NULL DEFAULT '0',
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `latitude` (`Latitude`) /*!80000 INVISIBLE */,
  KEY `longitude` (`Longitude`) /*!80000 INVISIBLE */,
  KEY `isActive` (`IsActive`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserRoles`
--

DROP TABLE IF EXISTS `UserRoles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserRoles` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `UserId` int NOT NULL,
  `Role` varchar(45) NOT NULL,
  `Created` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `userId` (`UserId`) /*!80000 INVISIBLE */,
  KEY `role` (`Role`) /*!80000 INVISIBLE */,
  KEY `created` (`Created`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Users`
--

DROP TABLE IF EXISTS `Users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Users` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `AccountId` int NOT NULL DEFAULT '0',
  `EventId` int NOT NULL DEFAULT '0',
  `Username` varchar(100) NOT NULL,
  `PasswordHash` varbinary(500) NOT NULL,
  `PasswordSalt` varbinary(200) NOT NULL,
  `Email` varchar(320) NOT NULL,
  `EmailVerified` int NOT NULL DEFAULT '0',
  `Firstname` varchar(100) NOT NULL,
  `Lastname` varchar(100) NOT NULL,
  `AvatarUrl` varchar(300) DEFAULT NULL,
  `Enabled` int NOT NULL DEFAULT '1',
  `Deleted` int NOT NULL DEFAULT '0',
  `Created` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  KEY `accountId` (`AccountId`) /*!80000 INVISIBLE */,
  KEY `eventId` (`EventId`) /*!80000 INVISIBLE */,
  KEY `username` (`Username`) /*!80000 INVISIBLE */,
  KEY `email` (`Email`) /*!80000 INVISIBLE */,
  KEY `emailVerified` (`EmailVerified`),
  KEY `enabled` (`Enabled`) /*!80000 INVISIBLE */,
  KEY `deleted` (`Deleted`) /*!80000 INVISIBLE */,
  KEY `created` (`Created`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping events for database 'Painless'
--

--
-- Dumping routines for database 'Painless'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-09-08 10:54:35
