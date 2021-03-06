﻿using System;
using System.Diagnostics;
using System.Threading;
using Android.Media;
using Android.OS;

namespace TrackRadar
{
    public sealed class ServiceAlarms : IDisposable
    {
        MediaPlayer offTrackPlayer;
        MediaPlayer gpsLostPlayer;
        MediaPlayer gpsPositiveAcknowledgementPlayer;
        Vibrator vibrator;

        internal void Reset(Vibrator vibrator,
            MediaPlayer distancePlayer,
            MediaPlayer gpsLostPlayer,
            MediaPlayer gpsOnPlayer)
        {
            Interlocked.Exchange(ref this.vibrator, vibrator);
            Interlocked.Exchange(ref this.offTrackPlayer, distancePlayer);
            Interlocked.Exchange(ref this.gpsLostPlayer, gpsLostPlayer);
            Interlocked.Exchange(ref this.gpsPositiveAcknowledgementPlayer, gpsOnPlayer);
        }

        internal void Go(Alarm alarm)
        {
            // https://developer.android.com/reference/android/media/MediaPlayer.html

            MediaPlayer p;
            if (alarm == Alarm.OffTrack)
                p = Interlocked.CompareExchange(ref this.offTrackPlayer, null, null);
            else if (alarm == Alarm.GpsLost)
                p = Interlocked.CompareExchange(ref this.gpsLostPlayer, null, null);
            else if (alarm == Alarm.PositiveAcknowledgement)
                p = Interlocked.CompareExchange(ref this.gpsPositiveAcknowledgementPlayer, null, null);
            else
                p = null;

            if (p != null && !p.IsPlaying)
                p.Start();

            var v = Interlocked.CompareExchange(ref this.vibrator, null, null);
            if (v != null)
                Common.VibrateAlarm(v);
        }

        public void Dispose()
        {
            Common.DestroyMediaPlayer(ref this.offTrackPlayer);
            Common.DestroyMediaPlayer(ref this.gpsLostPlayer);
            Common.DestroyMediaPlayer(ref this.gpsPositiveAcknowledgementPlayer);
        }

    }
}