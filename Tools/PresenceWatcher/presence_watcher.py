#!/usr/bin/env python3
"""
Patapon Cards - online presence watcher.

Polls the game's Firebase Realtime Database for player presence
(users/{uid}/presence/lastSeen, written by the game client's PresenceHeartbeat
every ~20s while it's running) and sends a Telegram message whenever a
player transitions from offline to online.

Setup instructions live in README.md next to this file.
"""

import os
import sys
import time
import logging

import requests
import firebase_admin
from firebase_admin import credentials, db
from dotenv import load_dotenv

load_dotenv()

# Found in Assets/Scripts/Game/Networking/FirebaseConfig.cs - fixed for this project,
# no need to configure it separately.
DATABASE_URL = "https://pataponcardgame-default-rtdb.firebaseio.com"

SERVICE_ACCOUNT_PATH = os.environ.get("FIREBASE_SERVICE_ACCOUNT", "serviceAccountKey.json")
TELEGRAM_BOT_TOKEN = os.environ.get("TELEGRAM_BOT_TOKEN")
TELEGRAM_CHAT_ID = os.environ.get("TELEGRAM_CHAT_ID")

POLL_SECONDS = int(os.environ.get("POLL_SECONDS", "15"))
# Matches PresenceHeartbeat.StaleAfterSeconds in the game client - a presence write older
# than this is treated as "no longer online", same threshold the game itself uses for
# GetPresence()/GetMatchInfo() when deciding whether a friend is online/in a match.
STALE_AFTER_SECONDS = int(os.environ.get("STALE_AFTER_SECONDS", "45"))

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
)
log = logging.getLogger("presence-watcher")


def require_config():
    missing = []
    if not TELEGRAM_BOT_TOKEN:
        missing.append("TELEGRAM_BOT_TOKEN")
    if not TELEGRAM_CHAT_ID:
        missing.append("TELEGRAM_CHAT_ID")
    if not os.path.isfile(SERVICE_ACCOUNT_PATH):
        log.error(
            "Firebase service account key not found at '%s'. "
            "Set FIREBASE_SERVICE_ACCOUNT in .env or place the file there.",
            SERVICE_ACCOUNT_PATH,
        )
        sys.exit(1)
    if missing:
        log.error(
            "Missing required setting(s) in .env: %s. See README.md.",
            ", ".join(missing),
        )
        sys.exit(1)


def init_firebase():
    cred = credentials.Certificate(SERVICE_ACCOUNT_PATH)
    firebase_admin.initialize_app(cred, {"databaseURL": DATABASE_URL})


def send_telegram(text: str):
    url = f"https://api.telegram.org/bot{TELEGRAM_BOT_TOKEN}/sendMessage"
    try:
        resp = requests.post(url, data={"chat_id": TELEGRAM_CHAT_ID, "text": text}, timeout=10)
        resp.raise_for_status()
    except requests.RequestException as e:
        log.error("Failed to send Telegram message: %s", e)


def get_nickname(uid: str, cache: dict) -> str:
    if uid in cache:
        return cache[uid]
    nickname = db.reference(f"users/{uid}/profile/nickname").get()
    nickname = nickname or uid
    cache[uid] = nickname
    return nickname


def poll_once() -> set:
    """Returns the set of uids whose presence is currently fresh (i.e. online)."""
    # Shallow fetch just gets {uid: true, ...} - not each user's full profile/collection/
    # deck data, which would be wasteful to pull every poll cycle just to check presence.
    uids = db.reference("users").get(shallow=True) or {}
    online = set()
    now = time.time()
    for uid in uids:
        presence = db.reference(f"users/{uid}/presence").get()
        if not presence:
            continue
        last_seen = presence.get("lastSeen")
        if last_seen is None:
            continue
        if now - float(last_seen) <= STALE_AFTER_SECONDS:
            online.add(uid)
    return online


def main():
    require_config()
    init_firebase()
    log.info("Connected to %s, polling every %ss", DATABASE_URL, POLL_SECONDS)

    nickname_cache = {}
    known_online = set()
    # The first poll just establishes a baseline - without this, restarting the watcher
    # while several players are already online would fire a notification for every one of
    # them, which isn't a real "just came online" event.
    first_poll = True

    while True:
        try:
            online_now = poll_once()

            if not first_poll:
                newly_online = online_now - known_online
                for uid in newly_online:
                    nickname = get_nickname(uid, nickname_cache)
                    log.info("%s came online", nickname)
                    send_telegram(f"\U0001F3AE {nickname} just came online in Patapon Cards!")

            known_online = online_now
            first_poll = False

        except Exception as e:
            log.error("Poll failed, will retry: %s", e)

        time.sleep(POLL_SECONDS)


if __name__ == "__main__":
    main()
