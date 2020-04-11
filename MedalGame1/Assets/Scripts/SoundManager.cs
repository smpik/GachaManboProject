using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	public AudioClip SoundCoinHit;//コイン衝突時の音
	public AudioClip SoundSensored;//コイン検出時の音

	private AudioSource AudioPlayer;//音を再生するオブジェクト

    // Start is called before the first frame update
    void Start()
    {
		AudioPlayer = GameObject.Find("AudioPlayer").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void PlaySoundCoinHit()
	{
		AudioPlayer.PlayOneShot(SoundCoinHit, 0.5f);
	}
	public void PlaySoundSensored()
	{
		AudioPlayer.PlayOneShot(SoundSensored, 0.5f);
	}
}
