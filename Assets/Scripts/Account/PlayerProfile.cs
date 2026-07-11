// Local cache of the signed-in player's own nickname, used for on-screen display (main menu,
// during a match). Set at sign-up (CreateAccountController) or sign-in (FirebaseAuth.SignIn).
public static class PlayerProfile
{
    public static string Nickname = "Player";
}
