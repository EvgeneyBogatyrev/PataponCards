using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{

    private static string pathDeck = Application.persistentDataPath + "/deck.dec";
    private static string pathRunes = Application.persistentDataPath + "/runes.run";
    public static void SaveDeck(List<CardTypes> deck)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(pathDeck, FileMode.Create);

        formatter.Serialize(stream, deck);
        stream.Close();
    }

    public static List<CardTypes> LoadDeck()
    {
        if (File.Exists(pathDeck))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(pathDeck, FileMode.Open);

                List<CardTypes> deck = formatter.Deserialize(stream) as List<CardTypes>;
                stream.Close();

                return deck;
            }
            catch
            {
                return new List<CardTypes>();
            }
        }
        else
        {
            return new List<CardTypes>();
        }
    }

    public static void SaveRunes(List<Runes> runes)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(pathRunes, FileMode.Create);

        formatter.Serialize(stream, runes);
        stream.Close();
    }

    public static List<Runes> LoadRunes()
    {
        if (File.Exists(pathRunes))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(pathRunes, FileMode.Open);

                List<Runes> runes = formatter.Deserialize(stream) as List<Runes>;
                stream.Close();

                return runes;
            }
            catch
            {
                return new List<Runes>();
            }
        }
        else
        {
            return new List<Runes>();
        }
    }
}
