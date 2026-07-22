# Patapon Cards presence watcher

Sends you a Telegram message whenever a player becomes online in Patapon Cards.
Works by polling the game's own Firebase Realtime Database for `users/{uid}/presence/lastSeen`
(the same heartbeat the game client writes every ~20s via `PresenceHeartbeat.cs`) and detecting
offline -> online transitions.

## 1. Create a Firebase service account key

1. Open the [Firebase console](https://console.firebase.google.com/), select the `pataponcardgame` project.
2. Gear icon (top left) -> **Project settings** -> **Service accounts** tab.
3. Click **Generate new private key** -> confirm. A JSON file downloads.
4. Rename it to `serviceAccountKey.json`.

This key grants full read/write access to the entire database, bypassing all security rules -
treat it like a master password. Never commit it to git or share it.

## 2. Create a Telegram bot and find your chat ID

1. In Telegram, open a chat with **@BotFather**, send `/newbot`, follow the prompts (pick a
   display name and a username ending in `bot`). It replies with a **token** that looks like
   `123456789:AAExampleTokenFromBotFather` - save it.
2. Send your new bot any message first (e.g. "hi") - bots can't message you until you've
   messaged them.
3. In a browser, open `https://api.telegram.org/bot<YOUR_TOKEN>/getUpdates` (replace
   `<YOUR_TOKEN>`). Find `"chat":{"id": ...}` in the JSON response - that number is your chat ID.

## 3. Set up the server

Assuming a Debian/Ubuntu-style Linux VPS:

```bash
ssh you@your-server

sudo apt update && sudo apt install -y python3 python3-pip python3-venv

mkdir -p ~/patapon-watcher && cd ~/patapon-watcher
```

From your own machine, copy this whole `Tools/PresenceWatcher` folder to the server (adjust
the path/host):

```bash
scp -r "Tools/PresenceWatcher/"* you@your-server:~/patapon-watcher/
```

Also copy `serviceAccountKey.json` from step 1 into that same folder on the server, then back
on the server:

```bash
cd ~/patapon-watcher
chmod 600 serviceAccountKey.json

cp .env.example .env
nano .env   # fill in TELEGRAM_BOT_TOKEN and TELEGRAM_CHAT_ID from step 2
chmod 600 .env

python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

Test it directly first:

```bash
python presence_watcher.py
```

It should log `Connected to https://pataponcardgame-default-rtdb.firebaseio.com, polling every
15s`. Open the game on another device/account and sign in - within ~15-60 seconds you should
get a Telegram message. Ctrl+C to stop the test run.

## 4. Run it permanently (systemd)

So it survives reboots and SSH disconnects, and restarts itself if it ever crashes.

Edit `patapon-watcher.service` (from this folder) and replace every
`REPLACE_WITH_YOUR_LINUX_USERNAME` with your actual server username, then:

```bash
sudo cp patapon-watcher.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now patapon-watcher
```

Check it's running:

```bash
sudo systemctl status patapon-watcher
journalctl -u patapon-watcher -f    # live logs, Ctrl+C to stop watching
```

To stop/restart later:

```bash
sudo systemctl stop patapon-watcher
sudo systemctl restart patapon-watcher
```

## Notes

- `POLL_SECONDS` (default 15) trades notification latency against how often the server reads
  from Firebase. Lower = faster notifications, more reads.
- `STALE_AFTER_SECONDS` (default 45) matches the game client's own `PresenceHeartbeat.
  StaleAfterSeconds` - how old a `lastSeen` write can be before that player is considered
  offline again.
- This notifies for *any* player entering the game, not just friends of a specific account -
  it reads presence for every registered user.
- Restarting the watcher never fires notifications for players already online at that moment
  (only for actual offline->online transitions seen after it starts).
