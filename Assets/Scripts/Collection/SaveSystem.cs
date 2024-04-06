using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{

    private static string pathDeck = Application.persistentDataPath + "/deck.dec";
    private static string pathRunes = Application.persistentDataPath + "/runes.run";
    public static bool loading = false;
    public static void SaveDeck(List<CardTypes> deck, int index=0)
    {
        string newPathDeck = pathDeck;
        if (index != 0) newPathDeck += index.ToString();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(newPathDeck, FileMode.Create);

        formatter.Serialize(stream, deck);
        stream.Close();
    }

    public static List<CardTypes> LoadDeck(int index=0)
    {
        string newPathDeck = pathDeck;
        if (index != 0) newPathDeck += index.ToString();
        if (File.Exists(newPathDeck))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(newPathDeck, FileMode.Open);

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

    public static void SaveRunes(List<Runes> runes, int index=0)
    {
        string newPathRunes = pathRunes;
        if (index != 0) newPathRunes += index.ToString();
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(newPathRunes, FileMode.Create);

        formatter.Serialize(stream, runes);
        stream.Close();
    }

    public static List<Runes> LoadRunes(int index=0)
    {
        loading = true;
        string newPathRunes = pathRunes;
        if (index != 0) newPathRunes += index.ToString();
        if (File.Exists(newPathRunes))
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(newPathRunes, FileMode.Open);

                List<Runes> runes = formatter.Deserialize(stream) as List<Runes>;
                stream.Close();
                loading = false;
                return runes;
            }
            catch
            {
                loading = false;
                return new List<Runes>();
            }
        }
        else
        {
            loading = false;
            return new List<Runes>();
        }
    }
}
