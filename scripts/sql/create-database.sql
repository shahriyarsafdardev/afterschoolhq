CREATE DATABASE [AfterSchool];
CREATE DATABASE [AfterSchoolTest];
GO

USE [AfterSchool];
GO

CREATE LOGIN AfterSchoolUser WITH PASSWORD = 'Nj8Ed$wpLfJ', CHECK_POLICY = OFF;
CREATE USER AfterSchoolUser FOR LOGIN AfterSchoolUser;
GO

ALTER ROLE db_owner ADD MEMBER AfterSchoolUser;
GO

USE [AfterSchoolTest];
GO

CREATE USER AfterSchoolUser FOR LOGIN AfterSchoolUser;
GO

ALTER ROLE db_owner ADD MEMBER AfterSchoolUser;
GO