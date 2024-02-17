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
    [SerializeField] private List<GameObject> ItemTexts;
    [SerializeField] private GameObject ListObject;
    [SerializeField] private GameObject AislesParent;
    [SerializeField] private KMSelectable ListToggle;
    [SerializeField] private AudioClip ToggleClip;
    [SerializeField] private List<GameObject> ItemButtons;
    [SerializeField] private List<KMSelectable> AisleArrows;
    [SerializeField] private List<GameObject> Aisles;
    [SerializeField] private TextMesh HighlightText;
    [SerializeField] private TextMesh AisleNumberText;
    [SerializeField] private List<KMSelectable> SlotButtons;
    [SerializeField] private AudioClip SuccessClip;
    [SerializeField] private GameObject CheckoutAisle;
    [SerializeField] private GameObject SolveButton;
    [SerializeField] private TextMesh SolveText;

    List<string> items = new List<string>() { "CUCUMBER", "CELERY", "TOMATO", "SPINACH", "BROCCOLI", "LETTUCE", "ZUCCHINI", "ASPARAGUS", "GARLIC", "CARROT", "ONION", "POTATOES", "CHICKEN", "SAUSAGES", "PORKCHOP", "STEAK", "TURKEY", "HOTDOGS", "BACON", "SALMON", "SHRIMP", "SUSHI", "MUFFINS", "CUPCAKES", "DONUTS", "COOKIES", "DANISHES", "BAGUETTE", "TORTILLAS", "BAGELS", "BREAD", "CROISSANTS", "BANANA", "PINEAPPLE", "COCONUT", "AVOCADO", "LEMON", "ORANGE", "CHERRIES", "GRAPES", "SOUP", "TUNA", "CEREAL", "CRACKERS", "PEANUTS", "COCOA", "MOLASSES", "CINNAMON", "OATMEAL", "SUGAR", "COFFEE", "FLOUR", "BATTERIES", "RAZORS", "DEODORANT", "SHAMPOO", "BLEACH", "SPONGES", "DETERGENT", "NAPKINS", "DIAPERS", "TISSUES", "POPCORN", "CHOCOLATE", "PIZZA", "ICE CREAM", "POPSICLES", "MILK", "JUICE", "CHEESE", "YOGURT", "EGGS", "MARGARINE", "BUTTER", "RICE", "HONEY", "PASTA", "JELLY", "MUSTARD", "VINEGAR" };

    List<string> modUnscrItems = new List<string>() { };
    List<string> modScrItems = new List<string>() { };

    bool moduleStarted;
    bool listView;
    string selectedItem;
    int curAisle;
    string[] slottedItems = new string[8];
    int correctCount;
    bool success;
    int hours, minutes, seconds = 0;

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
            button.GetComponent<KMSelectable>().OnSelect += delegate () { HighlightText.text = button.GetComponent<MarketItem>().ItemName == "ICECREAM" ? "ICE CREAM" : button.GetComponent<MarketItem>().ItemName; };
            button.GetComponent<KMSelectable>().OnDeselect += delegate () { HighlightText.text = ""; };
        }
        foreach (KMSelectable slot in SlotButtons) {
            slot.OnInteract += delegate () { SlotPress(SlotButtons.IndexOf(slot)); return false; };
            slot.OnSelect += delegate () { HighlightText.text = slottedItems[SlotButtons.IndexOf(slot)]; };
            slot.OnDeselect += delegate () { HighlightText.text = ""; };
        }
        foreach (KMSelectable arrow in AisleArrows) {
            arrow.OnInteract += delegate () { CycleAisles(AisleArrows.IndexOf(arrow)); return false; };
        }

        SolveButton.GetComponent<KMSelectable>().OnInteract += delegate () { Solved(); return false; };
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
            foreach (GameObject iText in ItemTexts)
            {
                iText.SetActive(true);
            }
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
        ListToggle.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, ListToggle.transform);
        Audio.PlaySoundAtTransform(ToggleClip.name, transform);
        listView = !listView;
        ListObject.SetActive(listView);
        AislesParent.SetActive(!listView);
    }

    void ItemPress(GameObject itemObj)
    {
        itemObj.GetComponent<KMSelectable>().AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, itemObj.transform);
        selectedItem = itemObj.GetComponent<MarketItem>().ItemName;
    }

    void SlotPress(int slotIdx)
    {
        SlotButtons[slotIdx].GetComponent<KMSelectable>().AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, SlotButtons[slotIdx].transform);
        if (selectedItem != "" & !success)
        {
            bool itemWasCorrect = slottedItems[slotIdx] == modUnscrItems[slotIdx];

            slottedItems[slotIdx] = selectedItem == "ICECREAM" ? "ICE CREAM" : selectedItem;
            HighlightText.text = selectedItem;

            bool itemCorrect = slottedItems[slotIdx] == modUnscrItems[slotIdx];
            if (!itemWasCorrect)
            {
                correctCount += itemCorrect ? 1 : 0;
            }
            else if (!itemCorrect)
            {
                correctCount -= 1;
            }
            Log($"Put {(selectedItem == "ICECREAM" ? "ICE CREAM" : TitleString(selectedItem))} in slot {slotIdx + 1}. That is {(itemCorrect ? "correct" : "incorrect")}. {correctCount}/8 correct items.");
            success = correctCount == 8;
            if (success)
            {
                Log("All items collected. Checkout is permanently open!");
                Audio.PlaySoundAtTransform(SuccessClip.name, transform);
                SolveButton.SetActive(true);
            }
        }
    }

    void CycleAisles(int dir)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, AisleArrows[dir].transform);
        curAisle += dir * 2 - 1;
        if (curAisle == Aisles.Count)
        {
            curAisle = Aisles.Count - 1;
        }
        else if (curAisle == -2)
        {
            curAisle = -1;
        }

        AisleNumberText.text = curAisle == -1 ? "C" : (curAisle + 1).ToString();
        AisleNumberText.fontSize = curAisle > 8 ? 105 : 200;
        for (int i = 0; i < Aisles.Count; i++)
        {
            Aisles[i].SetActive(i == curAisle);
        }
        CheckoutAisle.SetActive(curAisle == -1);
    }

    void Solved()
    {
        GetComponent<KMBombModule>().HandlePass();
        ModuleSolved = true;

        ListObject.SetActive(false);
        AislesParent.SetActive(false);
        ListToggle.gameObject.SetActive(false);
        AudioSrc.Stop();
        DisplayTimer();
    }

    void DisplayTimer()
    {
        SolveText.gameObject.SetActive(true);
        string timerText = "";
        if (hours > 0)
        {
            timerText += hours + ":";
        }
        timerText += $"{(minutes < 10 ? "0" + minutes.ToString() : minutes.ToString())}:{(seconds < 10 ? "0" + seconds.ToString() : seconds.ToString())}";
        SolveText.text = $"YOU'VE FINISHED 1ST!\nYOUR TIME:\n{timerText}";
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
            ItemTexts[i].GetComponent<TextMesh>().text = $"{i + 1}. {modScrItems[i]}";
            logScrambled += modScrItems[i];
            logScrambled += (i == 8 ? "." : ", ");
        }
        Log($"Items after Scrambling: {logScrambled}");
    }

    void Start()
    {
        ListObject.SetActive(true);
        AislesParent.SetActive(false);
        SolveButton.SetActive(false);
        foreach (GameObject iText in ItemTexts)
        {
            iText.SetActive(false);
        }
        StartCoroutine("Timer");
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

    IEnumerator Timer()
    {
        while (!ModuleSolved)
        {
            yield return new WaitForSeconds(1);
            seconds += 1;
            if (seconds == 60)
            {
                minutes += 1;
                seconds = 0;
                if (minutes == 60)
                {
                    hours += 1;
                    minutes = 0;
                }
            }
        }
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
