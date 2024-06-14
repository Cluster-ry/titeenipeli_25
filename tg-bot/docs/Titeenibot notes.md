# Todo:
- implement /game
- persistent data (connecty to the db)

# done:
- better acceptMenu text
- implement /menu
- have the bot with /start
- english sub
- finish /guild

# Possible problems:
- user's username changes (use uid to track who's who and use username as only a display name)
- bot forgets who has signed in after restart (need persistent data)
- Telegram stores sent command if the bot is offline
	- this means once the bot is back online it will run every command given while offline
	- not a problem?

# MVP:
- user acceptance
- guild choosing
- authentication

# extra:
- change guild?
	- user cannot (for obvious reasons)
	- incase of misclick, admin could change it?
- ctf?
	- /ctf which if given the right code grants user the given ctf
	- code should be long enough so brute force cant happen
- guild icons on guild selection screen
- nicer text




