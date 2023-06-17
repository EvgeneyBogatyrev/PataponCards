using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardBase : ScriptableObject
{
    public new string name;
    [TextArea(3, 10)]
    public string description;
    [NonSerialized]public Runes[] runes = new Runes[3];

    public Sprite icon;

    public int power = 4;

    public bool canAttack = true;
    public bool canMove = true;

    public bool haste;
    public bool shield;
    public bool lifelink;


    public bool isSpell;

    public BaseCardAbility ability;
    //somehow attach scripts

    //spell

    //endturn

    //onplay

    //ondeath

#if UNITY_EDITOR
    [CustomEditor(typeof(CardBase))]
    public class CardBaseEditor : Editor
    {
        Runes rune1;
        Runes rune2;
        Runes rune3;
        public override void OnInspectorGUI()
        {
            CardBase card = (CardBase)target;

            base.OnInspectorGUI();

            GUIStyle bold = GUI.skin.label;
            bold.fontStyle = FontStyle.Bold;

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Runes", bold);

            EditorGUILayout.BeginHorizontal();

            card.runes[0] = (Runes)EditorGUILayout.EnumPopup(rune1);
            card.runes[1] = (Runes)EditorGUILayout.EnumPopup(rune2);
            card.runes[2] = (Runes)EditorGUILayout.EnumPopup(rune3);

            EditorGUILayout.EndHorizontal();



            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scripts", bold);

            if (card.isSpell)
            {
                EditorGUILayout.LabelField("Spell Script");
            }
            else
            {
                EditorGUILayout.LabelField("Ability Script");
            }

            EditorGUI.DrawPreviewTexture(new Rect(15, 500, 200, 200), card.icon.texture);


        }
    }

#endif

}
