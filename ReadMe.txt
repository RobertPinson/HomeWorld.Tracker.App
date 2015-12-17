Test cards
04-64-81-6A-D1-1E-80
FD-A6-4A-95

Migration Commands

dnx ef migrations add initial -c TrackerDbContext
dnx ef database update initial -c TrackerDbContext

--rollback DB
dnx ef database update 0 -c TrackerDbContext
dnx ef migrations remove -c TrackerDbContext

