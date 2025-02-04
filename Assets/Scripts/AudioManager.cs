﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class AudioManager : MonoBehaviour
{
    public AudioSound[] clips;
    // Start is called before the first frame update
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        foreach (AudioSound c in clips)
        {
            c.source = gameObject.AddComponent<AudioSource>();
            c.source.clip = c.clip;
            c.source.volume = c.volume;
            c.source.pitch = c.pitch;
            c.source.loop = c.loop;
        }
    }

    // Update is called once per frame
    public void Play(string name, bool checkIfPlay = false)
    {
        AudioSound s = Array.Find(clips, sound => sound.name == name);
        if (s == null) {
            return;
        }
        Debug.Log("Sound is playng");
        if (!s.source.isPlaying || !checkIfPlay) {
            s.source.Play();
        }
    }
}
