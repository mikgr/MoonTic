#Clear Db
rm -r ~/ticketer/s_p_i_k_e/

#Create users
./bin/Debug/net10.0/Ticketer.Cli new user Conibase coinbase@gmail.com
./bin/Debug/net10.0/Ticketer.Cli new user Ada ada@gmail.com
./bin/Debug/net10.0/Ticketer.Cli new user Bob bob@gmail.com
./bin/Debug/net10.0/Ticketer.Cli new user Mak mak@gmail.com

#Fund Users
./bin/Debug/net10.0/Ticketer.Cli set user 0 + send money 1000 Ada + set user 1 + print balance
./bin/Debug/net10.0/Ticketer.Cli set user 0 + send money 1000 Bob + set user 2 + print balance
./bin/Debug/net10.0/Ticketer.Cli set user 0 + send money 1000 Mak + set user 3 + print balance

#Create Events
./bin/Debug/net10.0/Ticketer.Cli set user 1 + new event "Rock Concert 1" "2026-04-01 20:00" "2026-04-01 23:59" 100 50.0
./bin/Debug/net10.0/Ticketer.Cli set user 1 + publish event 0

./bin/Debug/net10.0/Ticketer.Cli set user 1 + new event "Rock Concert 2" "2026-05-01 20:00" "2026-05-01 23:59" 75 100.0
./bin/Debug/net10.0/Ticketer.Cli set user 1 + publish event 1

./bin/Debug/net10.0/Ticketer.Cli set user 2 + new event "Svampe Tur" "2026-06-20 16:00" "2026-06-20 19:00" 10 150.0
./bin/Debug/net10.0/Ticketer.Cli set user 2 + publish event 2

./bin/Debug/net10.0/Ticketer.Cli set user 2 + new event "Fiske Tur" "2026-06-3 12:00" "2026-06-04 13:00" 12 250.0
./bin/Debug/net10.0/Ticketer.Cli set user 2 + publish event 3

./bin/Debug/net10.0/Ticketer.Cli set user 2 + new event "Bær Tur" "2026-08-1 12:00" "2026-08-04 13:00" 15 200.0
# NB: event 4 is not published