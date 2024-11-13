Для успешного запуска необходимо:

1. Открыть проект в Visual Studio;
2. Открыть файл appsettings.json (он находится в корневом каталоге);
3. Изменить строку подключения к базе данных SQL Server (в этой СУБД должна быть создана БД с именем Clinic).

Для создания БД Clinic нужно:

1. Создать БД с именем Clinic;
2. После этого необходимо выполнить следующий запрос к этой базе данных:

CREATE TABLE Specialties (
    SpecialtyID INT PRIMARY KEY,
    SpecialtyName VARCHAR(50)
);

CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY,
    CategoryName VARCHAR(50)
);

CREATE TABLE VisitTypes (
    VisitTypeID INT PRIMARY KEY,
    VisitTypeName VARCHAR(50)
);

CREATE TABLE Roles (
    RoleID INT PRIMARY KEY,
    RoleName VARCHAR(50)
);

CREATE TABLE Doctors (
    DoctorID INT PRIMARY KEY,
    LastName VARCHAR(50),
    FirstName VARCHAR(50),
    MiddleName VARCHAR(50),
    Login VARCHAR(50),
    Password VARCHAR(50),
    SpecialtyID INT,
    CategoryID INT,
    RoleID INT,
    FOREIGN KEY (SpecialtyID) REFERENCES Specialties(SpecialtyID),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

CREATE TABLE Patients (
    PatientID INT PRIMARY KEY,
    LastName VARCHAR(50),
    FirstName VARCHAR(50),
    MiddleName VARCHAR(50),
    BirthYear INT
);

CREATE TABLE Visits (
    VisitID INT PRIMARY KEY,
    DoctorID INT,
    PatientID INT,
    VisitDate DATE,
    Diagnosis VARCHAR(255),
    TreatmentCost DECIMAL(10, 2),
    VisitTypeID INT,
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID),
    FOREIGN KEY (VisitTypeID) REFERENCES VisitTypes(VisitTypeID)
);

-- Вставка данных в таблицу Specialties
INSERT INTO Specialties (SpecialtyID, SpecialtyName) VALUES
(1, 'Cardiology'),
(2, 'Dermatology'),
(3, 'Pediatrics'),
(4, 'Neurology');

-- Вставка данных в таблицу Categories
INSERT INTO Categories (CategoryID, CategoryName) VALUES
(1, 'Junior'),
(2, 'Middle'),
(3, 'Senior');

-- Вставка данных в таблицу VisitTypes
INSERT INTO VisitTypes (VisitTypeID, VisitTypeName) VALUES
(1, 'Consultation'),
(2, 'Procedure'),
(3, 'Follow-up');

-- Вставка данных в таблицу Roles
INSERT INTO Roles (RoleID, RoleName) VALUES
(1, 'Doctor'),
(2, 'Nurse'),
(3, 'Admin');

-- Вставка данных в таблицу Doctors
INSERT INTO Doctors (DoctorID, LastName, FirstName, MiddleName, Login, Password, SpecialtyID, CategoryID, RoleID) VALUES
(1, 'Smith', 'John', 'A.', 'jsmith', 'password1', 1, 3, 1),
(2, 'Doe', 'Jane', 'B.', 'jdoe', 'password2', 2, 2, 1),
(3, 'Brown', 'Emily', 'C.', 'ebrown', 'password3', 3, 1, 2),
(4, 'Johnson', 'Michael', 'D.', 'mjohnson', 'password4', 4, 3, 3);

-- Вставка данных в таблицу Patients
INSERT INTO Patients (PatientID, LastName, FirstName, MiddleName, BirthYear) VALUES
(1, 'Williams', 'Alice', 'E.', 1985),
(2, 'Jones', 'Bob', 'F.', 1990),
(3, 'Miller', 'Charlie', 'G.', 1975),
(4, 'Davis', 'Diana', 'H.', 2000);

-- Вставка данных в таблицу Visits
INSERT INTO Visits (VisitID, DoctorID, PatientID, VisitDate, Diagnosis, TreatmentCost, VisitTypeID) VALUES
(1, 1, 1, '2023-10-01', 'Hypertension', 150.00, 1),
(2, 2, 2, '2023-10-02', 'Eczema', 100.00, 2),
(3, 3, 3, '2023-10-03', 'Common Cold', 50.00, 3),
(4, 4, 4, '2023-10-04', 'Migraine', 200.00, 1);