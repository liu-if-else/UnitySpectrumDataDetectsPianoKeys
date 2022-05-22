using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoKeysDetector : MonoBehaviour
{
    //88键钢琴
    //88 keys piano
    float[] Herz_PianoKeys = new float[88];

    //A4键440标准赫兹，键位49
    //A4 at key 49 with 440Hz
    float Herz_A4 = 440f;
    int Pos_A4 = 49;

    //12平均律
    //twelve-tone equal temperament
    float ET12 = 1f / 12f;

    //过滤当前音乐杂音阈值
    //filter threadshold of current music cliip
    float threadshold = 0.002f;

    //被分析的音乐来自：https://upload.wikimedia.org/wikipedia/commons/f/f0/ChromaticScaleUpDown.ogg
    //music clip from:https://upload.wikimedia.org/wikipedia/commons/f/f0/ChromaticScaleUpDown.ogg
    private AudioSource thisAudioSource;
    private float[] spectrumData = new float[8192];
    //the value denpended on pc, sould be updated in runtime
    private float MaxHerzOfSpectrumData = 22050;

    //钢琴键位<-->spectrumData位置
    //piano key position<-->spectrumData position
    private Dictionary<int, int> KeysDataMap = new Dictionary<int, int>();

    void InitAllPianoKeysHerz()
    {
        for(int i = 0; i < Herz_PianoKeys.Length; i++)
        {
            Herz_PianoKeys[i] = Mathf.Pow(Mathf.Pow(2, ET12), ((i + 1) - Pos_A4))*Herz_A4;
        }
    }

    void BindKeysAndSpectrumData()
    {
        float interval = MaxHerzOfSpectrumData / spectrumData.Length;
        //尝试找到精确的映射。与88和8192两个参数有关。
        //try to find the precise mapping.the algorithm depending on the parameters of 88 and 8192
        for (int i = 0; i < spectrumData.Length; i++)
        {
            for (int j = 0; j < Herz_PianoKeys.Length; j++) {
                if (Mathf.Abs((i + 1) * interval - Herz_PianoKeys[j]) <= 0.05f)
                {
                    KeysDataMap[j] = i;
                }
                else if (KeysDataMap.ContainsKey(j) == false && Mathf.Abs((i + 1) * interval - Herz_PianoKeys[j]) <= 1f)
                {
                    KeysDataMap[j] = i;
                }
                else if (KeysDataMap.ContainsKey(j) == false && Mathf.Abs((i + 1) * interval - Herz_PianoKeys[j]) <= 2f)
                {
                    KeysDataMap[j] = i;
                }
            }
        }
    }

    void AnalyzeMusic() 
    {
        float maxValue = 0;
        int maxKey = 0;
        foreach (var key in KeysDataMap.Keys)
        {
            //find max
            if (spectrumData[KeysDataMap[key]] > maxValue && spectrumData[KeysDataMap[key]] > threadshold)
            {
                maxValue = spectrumData[KeysDataMap[key]];
                maxKey = key;
            }
        }

        if (maxValue>0){
            Debug.Log(maxKey + 1);
            Debug.Log(spectrumData[KeysDataMap[maxKey]]);
            //test
            TestResult(maxKey+1);
        }
    }

    //should be:C4 C#4 D4 D#4 E4 F4 F#4 G4 G#4 A4 A#4 B4 C5 B4 A#4 A4 G#4 G4 F#4 F4 E4 D#4 D4 C#4 C4
    //          40 41  42 43  44 45 46  47 48  49 50  51 52 51 50  49 48  47 46  45 44 43  42 41  40
    List<int> testResults = new List<int>();
    void TestResult(int val)
    {
        if (testResults.Count > 0 && val != testResults[testResults.Count - 1])
        {
            testResults.Add(val);
        }
        else if (testResults.Count == 0)
        {
            testResults.Add(val);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //例如：44100/2=22050
        //eg:44100/2=22050
        MaxHerzOfSpectrumData = AudioSettings.outputSampleRate / 2;
        thisAudioSource = gameObject.GetComponent<AudioSource>();
        InitAllPianoKeysHerz();
        BindKeysAndSpectrumData();
        thisAudioSource.Play();
        Invoke("DebugTestResults", thisAudioSource.clip.length);
    }

    // Update is called once per frame
    void Update()
    {
        thisAudioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        AnalyzeMusic();
    }

    //debug function

    void DebugAllPianoKeysHerz()
    {
        for (int i = 0; i < Herz_PianoKeys.Length; i++)
        {
            Debug.Log(i);
            Debug.Log("Herz:" + Herz_PianoKeys[i]);
        }
    }
    void DebugKeysDataMap()
    {
        foreach (var key in KeysDataMap.Keys)
        {
            Debug.Log(key);
            Debug.Log(KeysDataMap[key]);
        }
    }
    void DebugTestResults()
    {
        string result = "";
        for(int i = 0; i < testResults.Count; i++)
        {
            result += testResults[i].ToString() + " ";
        }
        Debug.Log(result);
    }
}
