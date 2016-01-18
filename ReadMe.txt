Test cards
04-64-81-6A-D1-1E-80
FD-A6-4A-95

Migration Commands
--Identity schema
dnx ef database update 00000000000000_CreateIdentitySchema -c ApplicationDbContext

dnx ef migrations add initial -c TrackerDbContext
dnx ef database update initial -c TrackerDbContext

--rollback DB
dnx ef database update 0 -c TrackerDbContext
dnx ef migrations remove -c TrackerDbContext

