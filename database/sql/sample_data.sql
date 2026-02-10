INSERT INTO ROLE (RoleId, rolename, createdAt)
VALUES (ROLE_SEQ.NEXTVAL, 'Admin', SYSDATE);

INSERT INTO ROLE (RoleId, rolename, createdAt)
VALUES (ROLE_SEQ.NEXTVAL, 'Organizer', SYSDATE);

INSERT INTO ROLE (RoleId, rolename, createdAt)
VALUES (ROLE_SEQ.NEXTVAL, 'UniversityMember', SYSDATE);

INSERT INTO ROLE (RoleId, rolename, createdAt)
VALUES (ROLE_SEQ.NEXTVAL, 'ExternalMember', SYSDATE);

INSERT INTO "USER"
(member_id, Full_Name, email, phone, passwordHash, userType, UniversityNumber, Isverified, status, createdAt)
VALUES
(MEMBER_SEQ.NEXTVAL, 'Admin User', 'admin@uni.lk', '0771234567',
 'hashedpassword', 'Admin', 'UNI001', 'Y', 'Active', SYSDATE);

INSERT INTO "USER"
(member_id, Full_Name, email, phone, passwordHash, userType, UniversityNumber, Isverified, status, createdAt)
VALUES
(MEMBER_SEQ.NEXTVAL, 'Student One', 'student1@uni.lk', '0779876543',
 'hashedpassword', 'UniversityMember', 'UNI100', 'Y', 'Active', SYSDATE);

INSERT INTO EVENT_CATEGORY
(categoryID, categoryName, CreatedAt)
VALUES
(EVENT_CATEGORY_SEQ.NEXTVAL, 'Academic', SYSDATE);

INSERT INTO EVENT_CATEGORY
(categoryID, categoryName, CreatedAt)
VALUES
(EVENT_CATEGORY_SEQ.NEXTVAL, 'Cultural', SYSDATE);

INSERT INTO ORGANIZER_UNIT
(OrganizerID, unittime, UnitType, ContactEmail, ContactPhone, status, CreatedAt)
VALUES
(ORGANIZER_UNIT_SEQ.NEXTVAL, 'Faculty of Computing', 'Faculty',
 'computing@uni.lk', '0112233445', 'A', SYSDATE);

INSERT INTO EVENT
(eventID, title, Description, StartDateTime, endDateTime, venue, IsOnline,
 Agenda, maplink, organizerUnitID, categoryID, USER_member_id, createdAt, updatedAt)
VALUES
(EVENT_SEQ.NEXTVAL, 'Tech Symposium 2026',
 'Annual university technology symposium',
 SYSDATE, SYSDATE + 1,
 'Main Auditorium', 'N',
 'Keynote sessions and workshops',
 'https://maps.google.com',
 1, 1, 1, SYSDATE, SYSDATE);
