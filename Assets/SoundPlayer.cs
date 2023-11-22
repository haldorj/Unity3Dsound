using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
        AudioSource audioSource;
        const int buffer= 1024;
        float[] audioData;
        private int currentPosition = 0;
        [SerializeField , Header("3Dsound")]
        float maxDistance = 200.0f;
        [SerializeField]
        public float orbitSpeed = 20f;
        [SerializeField]
        public Transform player;
        [SerializeField]
        float Radius = 5.0f;
        // used awake to read and load the file 
        void Awake()
        {
            // Load an audio file  from a text file
            string audioFilePath = ReadAudioFilePathFromFile();
            audioSource = GetComponent<AudioSource>();
            //3D settings
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1.0f; // 1.0 means full 3D spatialization
            audioSource.spatializePostEffects = true;
            audioSource.spatialize = true;
            audioSource.maxDistance = 400.0f;
            //GameObject player = GetComponent<GameObject>();
            
            if (!string.IsNullOrEmpty(audioFilePath))
            {
                audioData = LoadAudioData(audioFilePath);
                PlayAudio();
            }
            else
            {
                Debug.LogError("Failed to read audio file path.");
            }
        }

        void PlayAudio()
        {
            if (audioData != null && audioData.Length > 0)
            {
                audioSource = GetComponent<AudioSource>();
                audioSource.clip = AudioClip.Create("LoadedClip", audioData.Length, 1, 44100, false);
                audioSource.clip.SetData(audioData, 0);
                audioSource.Play(0);
            }
            else
            {
                Debug.LogError("Failed to load audio data.");
            }
        }

        float[] LoadAudioData(string filePath)
        {
            try
            {
                // Load WAV file 
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                int headerOffset = 44; // Assuming a standard WAV header size
                float[] audioData = new float[(fileBytes.Length - headerOffset) / 2];
                /*
                fileBytes.Length: This refers to the length of the byte array named fileBytes.
                headerOffset: This is likely a variable representing the number of bytes to skip at the beginning of the fileBytes. 
                This is common when dealing with audio file formats that have a header containing metadata information.
                */
                for (int i = 0; i < audioData.Length; i++)
                { 
                    audioData[i] = (short)(fileBytes[headerOffset + i * 2] | (fileBytes[headerOffset + i * 2 + 1] << 8)) / 32768.0f; // refer to footnotes Part1
                }
                //Debug.Log(audioData.Length);
                return audioData;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load audio data: {ex.Message}");
                return null;
            }
        }

        string ReadAudioFilePathFromFile()
        {
   
            string p1 = Application.dataPath; // get the path 
            p1 = p1.Replace("/Assets", "/Assets/Audios/myfile.wav"); // replace the app address with the following
            Debug.Log(p1.ToString());
            return p1.ToString();
            
        }

        private void LateUpdate()
        {
            RotateAround();
            Vector3 playerPosition = Camera.main.transform.position; // player position
            Vector3 soundPosition = transform.position; // source of audio // rotating cube
            float distance = Vector3.Distance(playerPosition, soundPosition);
           // Debug.Log(soundPosition.ToString());
           //audioSource.transform.position = soundPosition
           audioSource.volume = Mathf.Clamp01(1.0f - distance / maxDistance);   //  adjust volume based on distance
          
        }
        void RotateAround()
            {
                float angle = Time.time * orbitSpeed;
                // Calculate the new position of the cube around the center
                Vector3 offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * Radius; // You can change radius
                Vector3 newPosition = player.transform.position + offset;

                // Set the position of the cube
                transform.position = newPosition;
        
                // Make the cube look at the center cube to face it while rotating
                transform.LookAt(player);
                Debug.Log("New Position: " + newPosition);
            }
}

// part 1
/*
fileBytes[headerOffset + i * 2]: Accesses the i-th pair of bytes from the fileBytes array. The i * 2 is used to skip every other byte, assuming each audio sample is represented by two bytes.
fileBytes[headerOffset + i * 2 + 1] << 8: Accesses the next byte and shifts its bits to the left by 8 positions. This is done to combine it with the previous byte, effectively merging two bytes into a 16-bit short.
(short)(...): Casts the combined bytes to a 16-bit signed short. This assumes that the audio data is stored as signed 16-bit PCM (common for uncompressed audio).
... / 32768.0f: Divides the obtained short value by 32768.0 to normalize the audio sample to the range [-1.0, 1.0]. This is a common practice when working with audio data in a floating-point representation.
audioData[i] = ...: Assigns the normalized floating-point value to the i-th element of the audioData array.
*/