using System;
using System.Collections.Generic;

// Accounts (by email) treated as developer/test accounts - these get every collectable card at 3
// copies, granted and re-saved every time MainMenu loads (see MainMenuController.Start()), rather
// than the normal starter-collection/earned-progression rules. Add test emails here as needed.
public static class DeveloperAccounts
{
    private static readonly HashSet<string> Emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "evgeneyzim@mail.ru",
    };

    public static bool IsDeveloper(string email)
    {
        return !string.IsNullOrEmpty(email) && Emails.Contains(email);
    }
}
