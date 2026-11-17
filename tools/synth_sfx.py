"""Synthesizes the game's sound effects into Assets/_Project/Audio as 16-bit mono WAV files.

Run from the repository root: python tools/synth_sfx.py
Every clip is built from sine, triangle and noise partials with short envelopes, so the output is
deterministic and license-free. Change a recipe below and run the script again to replace a clip.
"""

import math
import os
import random
import struct
import wave

RATE = 22050
OUT_DIR = os.path.join("Assets", "_Project", "Audio")


def silence(seconds):
    return [0.0] * int(RATE * seconds)


def tone(freq_start, freq_end, seconds, shape="sine", attack=0.004, decay=6.0, harmonics=(1.0,)):
    count = int(RATE * seconds)
    samples = []
    phase = 0.0
    for i in range(count):
        t = i / RATE
        progress = i / max(1, count - 1)
        freq = freq_start * (freq_end / freq_start) ** progress
        phase += 2.0 * math.pi * freq / RATE
        value = 0.0
        for index, weight in enumerate(harmonics):
            p = phase * (index + 1)
            if shape == "triangle":
                value += weight * (2.0 / math.pi) * math.asin(math.sin(p))
            else:
                value += weight * math.sin(p)
        envelope = min(1.0, t / attack) * math.exp(-decay * t)
        samples.append(value * envelope)
    return samples


def noise(seconds, decay=40.0, seed=7):
    rng = random.Random(seed)
    count = int(RATE * seconds)
    return [rng.uniform(-1.0, 1.0) * math.exp(-decay * i / RATE) for i in range(count)]


def mix(*layers):
    length = max(len(layer) for layer, _ in layers)
    out = [0.0] * length
    for layer, gain in layers:
        for i, value in enumerate(layer):
            out[i] += value * gain
    return out


def concat(*parts):
    out = []
    for part in parts:
        out.extend(part)
    return out


def overlay_at(base, layer, start_seconds, gain=1.0):
    start = int(RATE * start_seconds)
    needed = start + len(layer)
    if needed > len(base):
        base = base + [0.0] * (needed - len(base))
    for i, value in enumerate(layer):
        base[start + i] += value * gain
    return base


def normalize(samples, peak=0.85):
    top = max(abs(value) for value in samples) or 1.0
    faded = list(samples)
    fade = min(len(faded), int(RATE * 0.01))
    for i in range(fade):
        faded[-1 - i] *= i / fade
    return [value * peak / top for value in faded]


def write(name, samples):
    os.makedirs(OUT_DIR, exist_ok=True)
    path = os.path.join(OUT_DIR, name + ".wav")
    with wave.open(path, "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(RATE)
        file.writeframes(b"".join(struct.pack("<h", int(max(-1.0, min(1.0, v)) * 32767)) for v in samples))
    print(path)


def tap():
    return normalize(mix((tone(1900, 1300, 0.07, decay=60.0, harmonics=(1.0, 0.25)), 1.0),
                         (noise(0.012, decay=400.0), 0.15)), peak=0.6)


def grab():
    return normalize(mix((tone(520, 880, 0.11, decay=28.0, harmonics=(1.0, 0.35, 0.1)), 1.0),
                         (noise(0.01, decay=500.0, seed=3), 0.12)), peak=0.7)


def exit_whoosh():
    pop = tone(330, 990, 0.16, decay=14.0, harmonics=(1.0, 0.3))
    air = noise(0.22, decay=12.0, seed=11)
    sparkle = tone(1560, 2340, 0.12, decay=22.0)
    clip = mix((pop, 1.0), (air, 0.18))
    return normalize(overlay_at(clip, sparkle, 0.06, 0.35), peak=0.75)


def booster():
    clip = silence(0.0)
    for index, freq in enumerate((880, 1175, 1480, 1760, 2350)):
        clip = overlay_at(clip, tone(freq, freq * 1.02, 0.18, decay=16.0, harmonics=(1.0, 0.2)), index * 0.045, 0.8)
    clip = overlay_at(clip, noise(0.3, decay=9.0, seed=5), 0.0, 0.08)
    return normalize(clip, peak=0.75)


def win():
    clip = silence(0.0)
    notes = ((523.25, 0.0), (659.25, 0.11), (783.99, 0.22), (1046.5, 0.33))
    for freq, start in notes[:-1]:
        clip = overlay_at(clip, tone(freq, freq, 0.22, decay=10.0, harmonics=(1.0, 0.4, 0.15)), start)
    last, start = notes[-1]
    clip = overlay_at(clip, tone(last, last, 0.7, decay=3.5, harmonics=(1.0, 0.4, 0.15)), start)
    clip = overlay_at(clip, tone(last * 1.5, last * 1.5, 0.6, decay=4.0), start + 0.02, 0.35)
    for index, freq in enumerate((2093, 2637, 3136)):
        clip = overlay_at(clip, tone(freq, freq, 0.15, decay=20.0), start + 0.08 + index * 0.06, 0.18)
    return normalize(clip, peak=0.8)


def fail():
    clip = silence(0.0)
    notes = ((392.0, 0.0, 0.22), (369.99, 0.22, 0.22), (349.23, 0.44, 0.22), (329.63, 0.66, 0.7))
    for freq, start, length in notes:
        clip = overlay_at(clip, tone(freq, freq * 0.97, length, shape="triangle", attack=0.01, decay=3.0,
                                     harmonics=(1.0, 0.3)), start, 0.9)
    return normalize(clip, peak=0.75)


def tick():
    body = tone(1700, 1500, 0.05, decay=90.0, harmonics=(1.0, 0.5))
    knock = noise(0.02, decay=300.0, seed=13)
    return normalize(mix((body, 1.0), (knock, 0.4)), peak=0.7)


def main():
    write("sfx_tap", tap())
    write("sfx_grab", grab())
    write("sfx_exit", exit_whoosh())
    write("sfx_booster", booster())
    write("sfx_win", win())
    write("sfx_fail", fail())
    write("sfx_tick", tick())


if __name__ == "__main__":
    main()
