create table Tasks 
(
	ID int identity(1,1) primary key,
	ConsumerID int,
	CreationTime datetime,
	ModificationTime datetime,
	TaskText nvarchar(255),
	_Status tinyint
);

--drop table Tasks;