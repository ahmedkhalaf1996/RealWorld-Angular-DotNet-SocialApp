import { Injectable, NgZone } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

type ToneStep = { freq: number; duration: number; gain?: number };

@Injectable({
  providedIn: 'root'
})
export class SoundService {
  private audioContext?: AudioContext;
  private unlocked = false;
  private bindOnce = false;
  private readonly mutedSubject = new BehaviorSubject<boolean>(false);
  readonly muted$ = this.mutedSubject.asObservable();

  constructor(private ngZone: NgZone) {}

  setMuted(muted: boolean) {
    this.mutedSubject.next(!!muted);
  }

  bindAutoUnlock() {
    if (this.bindOnce) {
      return;
    }
    this.bindOnce = true;

    const unlock = () => {
      this.unlock();
    };

    window.addEventListener('pointerdown', unlock, { once: true });
    window.addEventListener('keydown', unlock, { once: true });
    window.addEventListener('touchstart', unlock, { once: true });
  }

  unlock() {
    this.ensureContext();
    if (!this.audioContext) {
      return;
    }
    this.audioContext.resume().then(() => {
      this.unlocked = true;
    });
  }

  playMessage() {
    this.playSequence([
      { freq: 880, duration: 0.07, gain: 0.12 },
      { freq: 660, duration: 0.07, gain: 0.12 }
    ]);
  }

  playNotification() {
    this.playSequence([
      { freq: 523.25, duration: 0.08, gain: 0.12 },
      { freq: 783.99, duration: 0.1, gain: 0.12 }
    ]);
  }

  private playSequence(steps: ToneStep[]) {
    if (this.mutedSubject.value) {
      return;
    }
    this.ensureContext();
    if (!this.audioContext || !this.unlocked) {
      return;
    }

    this.ngZone.runOutsideAngular(() => {
      const startAt = this.audioContext!.currentTime + 0.01;
      let offset = 0;
      steps.forEach((step) => {
        this.playTone(step.freq, step.duration, startAt + offset, step.gain ?? 0.12);
        offset += step.duration + 0.02;
      });
    });
  }

  private playTone(freq: number, duration: number, when: number, gainValue: number) {
    if (!this.audioContext) {
      return;
    }
    const osc = this.audioContext.createOscillator();
    const gain = this.audioContext.createGain();

    osc.type = 'sine';
    osc.frequency.value = freq;

    gain.gain.setValueAtTime(0.0001, when);
    gain.gain.exponentialRampToValueAtTime(gainValue, when + 0.01);
    gain.gain.exponentialRampToValueAtTime(0.0001, when + duration);

    osc.connect(gain);
    gain.connect(this.audioContext.destination);

    osc.start(when);
    osc.stop(when + duration + 0.02);
  }

  private ensureContext() {
    if (this.audioContext) {
      return;
    }
    const AudioContextRef = (window as any).AudioContext || (window as any).webkitAudioContext;
    if (AudioContextRef) {
      this.audioContext = new AudioContextRef();
    }
  }
}
