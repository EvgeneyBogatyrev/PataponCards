#!/usr/bin/env python3
"""
Admin tool for PataponCards redeemable chest codes.

Replaces the old in-game "Create Code" developer panel - code creation now happens here,
completely outside the game, using the Firebase Admin SDK (a service-account key), which
bypasses the Realtime Database's security rules entirely. The app itself only ever reads and
deletes a code (when a player redeems it) - it never writes one - so the database rules can
require auth for reads/deletes but disallow client writes to redeemCodes altogether.

Setup (one-time):
    1. pip install firebase-admin
    2. Firebase console -> Project Settings -> Service Accounts -> Generate new private key.
       Save the downloaded JSON somewhere safe (NOT inside the Unity project / not committed to
       source control - it's a full admin credential for your Firebase project).
    3. Note your Realtime Database URL (Firebase console -> Realtime Database -> top of page),
       e.g. https://your-project-default-rtdb.firebaseio.com

Usage:
    python redeem_codes.py --database-url <url> create <CODE> <CHESTS>
    python redeem_codes.py --database-url <url> delete <CODE>
    python redeem_codes.py --database-url <url> list
    python redeem_codes.py --database-url <url> log [--limit N]

    --credentials defaults to serviceAccountKey.json in the current directory; pass
    --credentials path/to/key.json to point elsewhere.

Data model (matches Assets/Scripts/Game/Networking/FirebaseCodes.cs):
    /redeemCodes/{code} -> { "chests": <int> }   - deleted by the app the moment it's redeemed
    /redeemLog/{pushId}  -> { "uid", "nickname", "code", "chests", "timestamp" }
"""
import argparse
import sys
from datetime import datetime

import firebase_admin
from firebase_admin import credentials, db


def init_app(credentials_path: str, database_url: str) -> None:
    cred = credentials.Certificate(credentials_path)
    firebase_admin.initialize_app(cred, {"databaseURL": database_url})


def create_code(code: str, chests: int) -> None:
    if chests <= 0:
        print("Chest count must be positive.", file=sys.stderr)
        sys.exit(1)
    db.reference(f"redeemCodes/{code}").set({"chests": chests})
    print(f"Created code '{code}' for {chests} chest(s).")


def delete_code(code: str) -> None:
    ref = db.reference(f"redeemCodes/{code}")
    if ref.get() is None:
        print(f"Code '{code}' doesn't exist (nothing to delete).")
        return
    ref.delete()
    print(f"Deleted code '{code}'.")


def list_codes() -> None:
    codes = db.reference("redeemCodes").get() or {}
    if not codes:
        print("No active codes.")
        return
    for code, data in codes.items():
        print(f"{code}: {data.get('chests')} chest(s)")


def show_log(limit: int) -> None:
    entries = db.reference("redeemLog").order_by_child("timestamp").limit_to_last(limit).get() or {}
    if not entries:
        print("No redemptions logged yet.")
        return
    for _, entry in sorted(entries.items(), key=lambda kv: kv[1].get("timestamp", 0)):
        when = datetime.fromtimestamp(entry.get("timestamp", 0)).strftime("%Y-%m-%d %H:%M:%S")
        print(f"[{when}] {entry.get('nickname')} redeemed '{entry.get('code')}' "
              f"for {entry.get('chests')} chest(s) (uid={entry.get('uid')})")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--credentials", default="serviceAccountKey.json",
                         help="Path to the Firebase service-account JSON key (default: serviceAccountKey.json)")
    parser.add_argument("--database-url", required=True,
                         help="Firebase Realtime Database URL, e.g. https://your-project-default-rtdb.firebaseio.com")

    subparsers = parser.add_subparsers(dest="command", required=True)

    create_parser = subparsers.add_parser("create", help="Create a new redeemable code")
    create_parser.add_argument("code")
    create_parser.add_argument("chests", type=int)

    delete_parser = subparsers.add_parser("delete", help="Delete a code without it ever being redeemed")
    delete_parser.add_argument("code")

    subparsers.add_parser("list", help="List all active (unredeemed) codes")

    log_parser = subparsers.add_parser("log", help="Show recent redemptions")
    log_parser.add_argument("--limit", type=int, default=20)

    args = parser.parse_args()
    init_app(args.credentials, args.database_url)

    if args.command == "create":
        create_code(args.code, args.chests)
    elif args.command == "delete":
        delete_code(args.code)
    elif args.command == "list":
        list_codes()
    elif args.command == "log":
        show_log(args.limit)


if __name__ == "__main__":
    main()
