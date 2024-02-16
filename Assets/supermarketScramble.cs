using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class supermarketScramble : MonoBehaviour
{
    [SerializeField] private KMBombInfo Bomb;
    [SerializeField] private KMAudio Audio;
    [SerializeField] private AudioSource AudioSrc;

    [SerializeField] private AudioClip SupermarketMusic;
    [SerializeField] private List<TextMesh> ItemTexts;
    [SerializeField] private GameObject ListObject;
    [SerializeField] private GameObject AislesParent;
    [SerializeField] private KMSelectable ListToggle;
    [SerializeField] private List<GameObject> ItemButtons;
    [SerializeField] private List<KMSelectable> AisleArrows;
    [SerializeField] private List<GameObject> Aisles;

    List<string> items = new List<string>() { "CUCUMBER", "CELERY", "TOMATO", "SPINACH", "BROCCOLI", "LETTUCE", "ZUCCHINI", "ASPARAGUS", "GARLIC", "CARROT", "ONION", "POTATOES", "CHICKEN", "SAUSAGES", "PORKCHOP", "STEAK", "TURKEY", "HOTDOGS", "BACON", "SALMON", "SHRIMP", "SUSHI", "MUFFINS", "CUPCAKES", "DONUTS", "COOKIES", "DANISHES", "BAGUETTE", "TORTILLAS", "BAGELS", "BREAD", "CROISSANTS", "BANANA", "PINEAPPLE", "COCONUT", "AVOCADO", "LEMON", "ORANGE", "CHERRIES", "GRAPES", "SOUP", "TUNA", "CEREAL", "CRACKERS", "PEANUTS", "COCOA", "MOLASSES", "CINNAMON", "OATMEAL", "SUGAR", "COFFEE", "FLOUR", "BATTERIES", "RAZORS", "DEODORANT", "SHAMPOO", "BLEACH", "SPONGES", "DETERGENT", "NAPKINS", "DIAPERS", "TISSUES", "POPCORN", "CHOCOLATE", "PIZZA", "ICE CREAM", "POPSICLES", "MILK", "JUICE", "CHEESE", "YOGURT", "EGGS", "MARGARINE", "BUTTER", "RICE", "HONEY", "PASTA", "JELLY", "MUSTARD", "VINEGAR" };

    List<string> modUnscrItems = new List<string>() { };
    List<string> modScrItems = new List<string>() { };

    bool moduleStarted;
    bool listView;
    string selectedItem;
    int curAisle;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        GetComponent<KMSelectable>().OnFocus += delegate () { ModuleSelected(); };
        GetComponent<KMSelectable>().OnDefocus += delegate () { AudioSrc.mute = true; };
        ListToggle.OnInteract += delegate () { ToggleList(); return false; };
        foreach (GameObject button in ItemButtons) {
            button.GetComponent<KMSelectable>().OnInteract += delegate () { ItemPress(button); return false; };
        }
        foreach (KMSelectable arrow in AisleArrows) {
            arrow.OnInteract += delegate () { CycleAisles(AisleArrows.IndexOf(arrow)); return false; };
        }
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
            }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };
    }

    void ModuleSelected()
    {
        if (!moduleStarted)
        {
            moduleStarted = true;
            AudioSrc.clip = SupermarketMusic; 
            AudioSrc.Play();

            listView = true;
            ListObject.SetActive(true);
            AislesParent.SetActive(false);
            for (int i = 0; i < Aisles.Count; i++)
            {
                Aisles[i].SetActive(i == 0);
            }

            GenItems();
            ScrambleItems();
        }
        AudioSrc.mute = false;
    }

    void ToggleList()
    {
        listView = !listView;
        ListObject.SetActive(listView);
        AislesParent.SetActive(!listView);
    }

    void ItemPress(GameObject itemObj)
    {
        selectedItem = itemObj.GetComponent<MarketItem>().ItemName;
    }

    void CycleAisles(int dir)
    {
        curAisle += dir * 2 - 1;
        for (int i = 0; i < Aisles.Count; i++)
        {
            Aisles[i].SetActive(i == curAisle);
        }
    }

    void GenItems()
    {
        List<string> tempItems = items.ConvertAll(x => x);
        string logUnscrambled = "";
        for (int i = 0; i < 8; i++)
        {
            modUnscrItems.Add(tempItems[Rnd.Range(0, tempItems.Count - 1)]);
            tempItems.Remove(modUnscrItems[i]);
            logUnscrambled += TitleString(modUnscrItems[i]);
            logUnscrambled += (i == 8 ? "." : ", ");
        }
        Log($"Unscrambled Items are: {logUnscrambled}");
    }

    void ScrambleItems()
    {
        string logScrambled = "";
        for (int i = 0; i < 8; i++)
        {
            modScrItems.Add(ScrambleString(modUnscrItems[i]));
            ItemTexts[i].text = $"{i + 1}. {modScrItems[i]}";
            logScrambled += modScrItems[i];
            logScrambled += (i == 8 ? "." : ", ");
        }
        Log($"Items after Scrambling: {logScrambled}");
    }

    void Start()
    {
        AislesParent.SetActive(false);
    }

    string TitleString(string n)
    {
        return n[0].ToString().ToUpper() + n.Substring(1, n.Length - 1).ToLower();
    }

    string ScrambleString(string n)
    {
        string s = "";
        n = n.Replace(" ", "");
        while (n.Length > 0)
        {
            int idx = Rnd.Range(0, n.Length - 1);
            s += n[idx];
            n = n.Substring(0, idx) + n.Substring(idx + 1);
        }
        return s;
    }

    void Log(string arg)
    {
        Debug.Log($"[Supermarket Scramble #{ModuleId}] {arg}");
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
