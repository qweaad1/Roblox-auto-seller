-- MySQL dump 10.13  Distrib 8.0.28, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: qweaad
-- ------------------------------------------------------
-- Server version	8.4.3

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
-- Table structure for table `bots`
--

DROP TABLE IF EXISTS `bots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bots` (
  `id` int NOT NULL,
  `name` varchar(45) NOT NULL,
  `money_amount` int DEFAULT NULL,
  `last_update` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `catalog`
--

DROP TABLE IF EXISTS `catalog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `catalog` (
  `raw_name` varchar(255) NOT NULL,
  `clean_name_en` varchar(45) DEFAULT NULL,
  `clean_name_ru` varchar(45) DEFAULT NULL,
  `price` decimal(10,2) DEFAULT NULL,
  `link` varchar(255) DEFAULT NULL,
  `SPPRICE` decimal(10,2) DEFAULT '0.00',
  `FIRE` int DEFAULT NULL,
  PRIMARY KEY (`raw_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `inventory`
--

DROP TABLE IF EXISTS `inventory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inventory` (
  `index` int NOT NULL AUTO_INCREMENT,
  `id_string` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `type` varchar(255) NOT NULL,
  `age` int NOT NULL,
  `owner` varchar(45) NOT NULL,
  `time` datetime DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`index`,`id_string`),
  UNIQUE KEY `id_string_UNIQUE` (`id_string`),
  KEY `raw_name_idx` (`id_string`)
) ENGINE=InnoDB AUTO_INCREMENT=117859 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `id_order_ref_funpay` varchar(255) NOT NULL,
  `addressee_player` varchar(255) NOT NULL,
  `servise_name` varchar(255) NOT NULL,
  `item_name` varchar(255) NOT NULL,
  `count_to_send` int NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `date_of_order_open` datetime DEFAULT CURRENT_TIMESTAMP,
  `status` int DEFAULT '0',
  PRIMARY KEY (`id_order_ref_funpay`),
  UNIQUE KEY `id_order_ref_funpay_UNIQUE` (`id_order_ref_funpay`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `payments`
--

DROP TABLE IF EXISTS `payments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `payments` (
  `id` int NOT NULL AUTO_INCREMENT,
  `payment_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'ID платежа в ЮKassa',
  `order_id` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'ID заказа (ORD-...)',
  `username` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Имя пользователя',
  `email` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Email пользователя',
  `amount` decimal(10,2) NOT NULL COMMENT 'Сумма платежа',
  `currency` varchar(10) COLLATE utf8mb4_unicode_ci DEFAULT 'RUB' COMMENT 'Валюта',
  `status` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Статус платежа (pending, succeeded, canceled)',
  `payment_data` json DEFAULT NULL COMMENT 'Полные данные платежа из ЮKassa',
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP COMMENT 'Дата создания',
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Дата обновления',
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_payment_id` (`payment_id`),
  KEY `idx_order_id` (`order_id`),
  KEY `idx_username` (`username`),
  KEY `idx_status` (`status`)
) ENGINE=InnoDB AUTO_INCREMENT=86 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Таблица платежей ЮKassa';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `raw_material_supplier`
--

DROP TABLE IF EXISTS `raw_material_supplier`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `raw_material_supplier` (
  `NAME` varchar(255) NOT NULL,
  `password` varchar(255) DEFAULT NULL,
  `bucks` int DEFAULT NULL,
  `potions` int DEFAULT NULL,
  `on_boot_bucks` int NOT NULL,
  `on_boot_potions` int NOT NULL,
  `buy_count` int NOT NULL,
  PRIMARY KEY (`NAME`),
  UNIQUE KEY `NAME_UNIQUE` (`NAME`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `waybill`
--

DROP TABLE IF EXISTS `waybill`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `waybill` (
  `id_way` int NOT NULL AUTO_INCREMENT,
  `order_id` varchar(45) NOT NULL,
  `item_id` varchar(255) NOT NULL,
  `sender` varchar(255) DEFAULT NULL,
  `addressee` varchar(255) NOT NULL,
  `item_catalog_name` varchar(255) DEFAULT NULL,
  `Done` tinyint DEFAULT '0',
  PRIMARY KEY (`id_way`),
  UNIQUE KEY `item_id_UNIQUE` (`item_id`)
) ENGINE=InnoDB AUTO_INCREMENT=8148 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `waybill_notifications`
--

DROP TABLE IF EXISTS `waybill_notifications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `waybill_notifications` (
  `id` int NOT NULL AUTO_INCREMENT,
  `event_type` varchar(20) NOT NULL,
  `waybill_id` int NOT NULL,
  `done_status` tinyint(1) DEFAULT '0',
  `processed` tinyint(1) DEFAULT '0',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `idx_processed` (`processed`)
) ENGINE=InnoDB AUTO_INCREMENT=13812 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-09-16 18:57:48
