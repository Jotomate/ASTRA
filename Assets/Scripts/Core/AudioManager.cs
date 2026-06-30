using System.Collections.Generic;
using UnityEngine;

namespace ShootingGame.Core
{
    /// <summary>
    /// 런타임 합성 사운드 매니저. 외부 오디오 파일 없이 코드로 레트로 SFX/BGM을 생성한다. (§8)
    /// SFX는 라운드로빈 AudioSource 풀로 피치 변주하며 재생.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        const int SR = 44100;
        const float TAU = 6.2831853f;

        AudioSource[] sfx;
        int sfxIdx;
        AudioSource bgm;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            sfx = new AudioSource[8];
            for (int i = 0; i < sfx.Length; i++)
            {
                var s = gameObject.AddComponent<AudioSource>();
                s.playOnAwake = false; s.volume = 0.45f;
                sfx[i] = s;
            }
            bgm = gameObject.AddComponent<AudioSource>();
            bgm.loop = true; bgm.volume = 0.22f; bgm.playOnAwake = false;

            BuildClips();
            if (clips.TryGetValue("bgm", out var b)) { bgm.clip = b; bgm.Play(); }
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        /// <summary>SFX 재생(피치 변주). 존재하지 않으면 무시.</summary>
        public void Play(string name, float volume = 1f, float pitchVar = 0.06f)
        {
            if (!clips.TryGetValue(name, out var c) || c == null) return;
            var s = sfx[sfxIdx];
            sfxIdx = (sfxIdx + 1) % sfx.Length;
            s.pitch = 1f + Random.Range(-pitchVar, pitchVar);
            s.PlayOneShot(c, volume);
        }

        // ===== 합성 =====
        void BuildClips()
        {
            clips["shoot"] = Clip("shoot", Square(720f, 0.07f, 0.35f, 22f));
            clips["laser"] = Clip("laser", Saw(420f, 0.12f, 0.3f, 9f));
            clips["hit"] = Clip("hit", Noise(0.05f, 0.35f, 45f));
            clips["explosion"] = Clip("explosion", Explosion(0.42f));
            clips["bossexp"] = Clip("bossexp", Explosion(0.9f));
            clips["pickup"] = Clip("pickup", Arp(new float[] { 523f, 659f, 784f, 1046f }, 0.05f, 0.4f));
            clips["power"] = Clip("power", Arp(new float[] { 440f, 554f, 659f, 880f }, 0.05f, 0.4f));
            clips["bomb"] = Clip("bomb", Bomb(0.7f));
            clips["death"] = Clip("death", Slide(440f, 80f, 0.6f, 0.4f, 1));
            clips["warn"] = Clip("warn", Alarm(0.5f));
            clips["bgm"] = Clip("bgm", Bgm());
        }

        AudioClip Clip(string name, float[] data)
        {
            var c = AudioClip.Create(name, data.Length, 1, SR, false);
            c.SetData(data, 0);
            return c;
        }

        static float Env(float t, float dur, float decay)
        {
            float atk = Mathf.Clamp01(t / 0.004f);
            return atk * Mathf.Exp(-decay * t);
        }

        float[] Wave(float freq, float dur, float vol, float decay, int wave)
        {
            int n = (int)(dur * SR); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                float ph = (freq * t) % 1f;
                float s = wave == 0 ? Mathf.Sin(TAU * freq * t) : wave == 1 ? (ph < 0.5f ? 1f : -1f) : (2f * ph - 1f);
                d[i] = s * vol * Env(t, dur, decay);
            }
            return d;
        }
        float[] Square(float f, float dur, float vol, float decay) => Wave(f, dur, vol, decay, 1);
        float[] Saw(float f, float dur, float vol, float decay) => Wave(f, dur, vol, decay, 2);

        float[] Noise(float dur, float vol, float decay)
        {
            int n = (int)(dur * SR); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR;
                d[i] = (Random.value * 2f - 1f) * vol * Mathf.Exp(-decay * t);
            }
            return d;
        }

        float[] Explosion(float dur)
        {
            int n = (int)(dur * SR); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR; float e = Mathf.Exp(-7f * t);
                float rumble = Mathf.Sin(TAU * 70f * t) * 0.4f;
                d[i] = ((Random.value * 2f - 1f) * 0.6f + rumble) * 0.55f * e;
            }
            return d;
        }

        float[] Bomb(float dur)
        {
            int n = (int)(dur * SR); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR; float k = t / dur;
                float f = Mathf.Lerp(900f, 120f, k);
                float s = Mathf.Sin(TAU * f * t) * 0.5f + (Random.value * 2f - 1f) * 0.4f;
                d[i] = s * 0.5f * Mathf.Exp(-2.5f * t);
            }
            return d;
        }

        float[] Slide(float f0, float f1, float dur, float vol, int wave)
        {
            int n = (int)(dur * SR); var d = new float[n]; float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SR; float k = t / dur;
                float f = Mathf.Lerp(f0, f1, k);
                phase += f / SR;
                float ph = phase % 1f;
                float s = wave == 1 ? (ph < 0.5f ? 1f : -1f) : Mathf.Sin(TAU * phase);
                d[i] = s * vol * Mathf.Exp(-2f * t);
            }
            return d;
        }

        float[] Arp(float[] notes, float noteDur, float vol)
        {
            var list = new List<float>();
            foreach (var f in notes) list.AddRange(Square(f, noteDur, vol, 6f));
            return list.ToArray();
        }

        float[] Alarm(float dur)
        {
            var a = Square(880f, dur * 0.5f, 0.3f, 1.5f);
            var b = Square(660f, dur * 0.5f, 0.3f, 1.5f);
            var d = new float[a.Length + b.Length];
            System.Array.Copy(a, 0, d, 0, a.Length);
            System.Array.Copy(b, 0, d, a.Length, b.Length);
            return d;
        }

        // 절차적 BGM: 베이스 + 아르페지오 루프 (마이너 진행)
        float[] Bgm()
        {
            float bpm = 132f; float beat = 60f / bpm; float step = beat * 0.5f; // 8분음표
            // i, VI, III, VII (Am-F-C-G 느낌) 루트와 아르페지오
            int[] roots = { 220, 174, 130, 196 };         // A3, F3, C3, G3
            int[][] arps = {
                new[]{440,523,659,523}, new[]{349,440,523,440},
                new[]{523,659,784,659}, new[]{392,494,587,494}
            };
            int stepN = (int)(step * SR);
            int bars = 4, stepsPerBar = 8;
            int total = stepN * stepsPerBar * bars;
            var d = new float[total];
            for (int bar = 0; bar < bars; bar++)
            {
                float bf = roots[bar];
                int[] arp = arps[bar];
                for (int s = 0; s < stepsPerBar; s++)
                {
                    int off = (bar * stepsPerBar + s) * stepN;
                    // 베이스(매 비트)
                    if (s % 2 == 0) AddInto(d, off, Square(bf, step * 0.9f, 0.18f, 3f));
                    // 아르페지오
                    AddInto(d, off, Square(arp[s % arp.Length], step * 0.8f, 0.12f, 4f));
                }
            }
            return d;
        }

        static void AddInto(float[] dst, int off, float[] src)
        {
            int n = Mathf.Min(src.Length, dst.Length - off);
            for (int i = 0; i < n; i++) dst[off + i] = Mathf.Clamp(dst[off + i] + src[i], -1f, 1f);
        }
    }
}
