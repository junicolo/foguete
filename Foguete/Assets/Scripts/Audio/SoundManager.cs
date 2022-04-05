using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager _;

    private void Awake() => _ = this;

    static Dictionary<Sound, AudioClip> sClip = new Dictionary<Sound, AudioClip>();
    static Dictionary<Sound, SoundController.SequenceClip> sequences = new Dictionary<Sound, SoundController.SequenceClip>();

    public static void AssignDicts(SoundController.SoundClip[] soundAssets, SoundController.SequenceClip[] sequenceAssets ){
        foreach (SoundController.SoundClip sc  in soundAssets) sClip.Add(sc.sound, sc.clip);
        foreach (SoundController.SequenceClip seq  in sequenceAssets) sequences.Add(seq.sound, seq);
    }
    
    public static void PlaySound(Sound soundName, Transform parent) {
        GameObject soundObj = new GameObject();
        AudioSource src = soundObj.AddComponent<AudioSource>();
        src.spatialBlend = 1;

        soundObj.transform.parent = parent;
        soundObj.transform.position = parent.position;

       AudioClip clip = sClip[soundName];
       src.clip = clip;
       src.Play();
       Destroy(soundObj, clip.length);
    }

    public void PlaySequence(Sound soundName, Transform parent){
        GameObject soundObj = new GameObject();
        AudioSource src = soundObj.AddComponent<AudioSource>();
        src.spatialBlend = 1;
        
        soundObj.transform.parent = parent;
        soundObj.transform.position = parent.position;
        StartCoroutine(SequenceAudio(sequences[soundName], src));
    }

    public void StopSequence(Sound soundName) {
        sequences[soundName].keepPlayin = false;
    }

    IEnumerator SequenceAudio(SoundController.SequenceClip seq, AudioSource src) {
        AudioClip intro = seq.intro[Random.Range(0, seq.intro.Length - 1)];
        src.clip = intro;
        src.Play();
        yield return new WaitForSeconds(intro.length - 0.4f);
       
        while (seq.keepPlayin) {
            AudioClip loop = seq.loop[Random.Range(0, seq.loop.Length - 1)];
            src.clip = loop;
            src.Play();
            yield return new WaitForSeconds(loop.length - 0.4f);
        }

        AudioClip outro = seq.outro[Random.Range(0, seq.outro.Length - 1)];
        src.clip = outro;
        src.Play();
        yield return new WaitForSeconds(outro.length);
        seq.keepPlayin = true;
    }
}

