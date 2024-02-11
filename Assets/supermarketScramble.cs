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

    bool moduleStarted;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        GetComponent<KMSelectable>().OnFocus += delegate () { ModuleSelected(); };
        GetComponent<KMSelectable>().OnDefocus += delegate () { AudioSrc.mute = true; };
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
        }
        AudioSrc.mute = false;
    }

    void Start()
    {

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
