using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Sound {
    Ignition,
    Separation,
    Parachute
    
    
}
public class SoundController : MonoBehaviour {
   
    [Serializable] 
    public class SoundClip {
        public Sound sound;
        public AudioClip clip;
    }
    
    [Serializable]
    public class SequenceClip {
        public Sound sound;
        public bool keepPlayin = true;
        
        public AudioClip[] intro;
        public AudioClip[] loop;
        public AudioClip[] outro;

    }
    
    public SoundClip[] soundAssets;
    public SequenceClip[] sequenceAssets;

    private void Awake() {
        SoundManager.AssignDicts(soundAssets, sequenceAssets);
    } 
}
