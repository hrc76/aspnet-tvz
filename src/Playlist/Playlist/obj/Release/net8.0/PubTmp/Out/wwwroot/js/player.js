/* ═══════════════════════════════════════════════════════════
   MusicBar Player  —  HTML5 audio engine
   ═══════════════════════════════════════════════════════════ */
var MBPlayer = (function () {
    'use strict';

    var queue        = [];
    var currentIndex = -1;
    var isShuffling  = false;
    var repeatMode   = 0;   // 0=off  1=all  2=one
    var audio;

    // ── Initialise ───────────────────────────────────────────────
    function init() {
        audio = document.getElementById('mbAudio');
        if (!audio) return;

        bindControls();
        bindAudioEvents();
        bindKeyboard();
        restoreState();
    }

    function bindControls() {
        on('playerPlay',    'click', togglePlay);
        on('playerNext',    'click', next);
        on('playerPrev',    'click', prev);
        on('playerShuffle', 'click', toggleShuffle);
        on('playerRepeat',  'click', toggleRepeat);
        on('playerMute',    'click', toggleMute);
        on('playerLike',    'click', toggleLike);

        var seek = document.getElementById('playerSeek');
        if (seek) seek.addEventListener('input', onSeek);

        var vol = document.getElementById('playerVolume');
        if (vol) {
            vol.addEventListener('input', onVolume);
            audio.volume = vol.value / 100;
        }
    }

    function bindAudioEvents() {
        audio.addEventListener('timeupdate',    onTimeUpdate);
        audio.addEventListener('ended',         onEnded);
        audio.addEventListener('loadedmetadata',onMetadata);
        audio.addEventListener('play',          function () { updatePlayBtn(true); });
        audio.addEventListener('pause',         function () { updatePlayBtn(false); });
        audio.addEventListener('error',         function () {
            document.getElementById('playerArtist').textContent = 'Could not load audio';
        });
    }

    function bindKeyboard() {
        document.addEventListener('keydown', function (e) {
            var tag = e.target.tagName;
            if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;
            if (e.code === 'Space') { e.preventDefault(); togglePlay(); }
            if (e.code === 'ArrowRight') { e.preventDefault(); skip(10); }
            if (e.code === 'ArrowLeft')  { e.preventDefault(); skip(-10); }
            if (e.code === 'ArrowUp')    { e.preventDefault(); adjustVolume(0.05); }
            if (e.code === 'ArrowDown')  { e.preventDefault(); adjustVolume(-0.05); }
        });
    }

    // ── Public: play a single song (replaces current queue) ──────
    function playSong(songData) {
        queue        = [songData];
        currentIndex = 0;
        loadAndPlay();
    }

    // ── Public: replace queue and play from index ─────────────────
    function setQueue(songs, startIndex) {
        queue = songs;
        currentIndex = startIndex || 0;
        loadAndPlay();
    }

    // ── Internal playback ─────────────────────────────────────────
    function loadAndPlay() {
        var song = queue[currentIndex];
        if (!song) return;
        updateNowPlayingUI(song);
        updateNowPlayingRows(song.id);
        saveState();

        getPlayableUrl(song, function (url) {
            if (!url) {
                var el = document.getElementById('playerArtist');
                if (el) el.textContent = 'No preview available';
                return;
            }
            song.audioUrl = url;
            audio.src = url;
            audio.load();
            audio.play().catch(function () {});
        });
    }

    function getPlayableUrl(song, callback) {
        var url = song.audioUrl || '';
        if (url && url.toLowerCase().indexOf('soundhelix') === -1) {
            callback(url);
            return;
        }
        var a = encodeURIComponent((song.artist || '').trim());
        var t = encodeURIComponent((song.title  || '').trim());
        fetch('/Song/DeezerPreview?artist=' + a + '&title=' + t)
            .then(function (r) { return r.ok ? r.json() : { url: null }; })
            .then(function (d) { callback(d.url || url || null); })
            .catch(function ()  { callback(url || null); });
    }

    function togglePlay() {
        if (audio.readyState === 0 && queue.length > 0) {
            loadAndPlay();
            return;
        }
        if (audio.paused) audio.play().catch(function () {});
        else audio.pause();
    }

    function next() {
        if (!queue.length) return;
        if (isShuffling) {
            currentIndex = Math.floor(Math.random() * queue.length);
        } else {
            currentIndex = (currentIndex + 1) % queue.length;
        }
        loadAndPlay();
    }

    function prev() {
        if (audio.currentTime > 3) { audio.currentTime = 0; return; }
        if (!queue.length) return;
        currentIndex = (currentIndex - 1 + queue.length) % queue.length;
        loadAndPlay();
    }

    function skip(seconds) {
        if (!audio.duration) return;
        audio.currentTime = Math.max(0, Math.min(audio.duration, audio.currentTime + seconds));
    }

    function adjustVolume(delta) {
        audio.volume = Math.max(0, Math.min(1, audio.volume + delta));
        var vol = document.getElementById('playerVolume');
        if (vol) vol.value = audio.volume * 100;
        updateMuteBtn();
    }

    // ── Toggle states ─────────────────────────────────────────────
    function toggleShuffle() {
        isShuffling = !isShuffling;
        var btn = document.getElementById('playerShuffle');
        if (btn) btn.classList.toggle('active', isShuffling);
    }

    function toggleRepeat() {
        repeatMode = (repeatMode + 1) % 3;
        var btn = document.getElementById('playerRepeat');
        if (!btn) return;
        btn.classList.remove('active');
        if (repeatMode === 1) { btn.textContent = '↺'; btn.classList.add('active'); btn.title = 'Repeat all'; }
        else if (repeatMode === 2) { btn.textContent = '↻'; btn.classList.add('active'); btn.title = 'Repeat one'; }
        else { btn.textContent = '↺'; btn.title = 'Repeat off'; }
    }

    function toggleMute() {
        audio.muted = !audio.muted;
        updateMuteBtn();
    }

    function toggleLike() {
        var btn = document.getElementById('playerLike');
        if (!btn) return;
        btn.classList.toggle('liked');
        btn.textContent = btn.classList.contains('liked') ? '♥' : '♡';
    }

    // ── Audio event handlers ──────────────────────────────────────
    function onEnded() {
        if (repeatMode === 2) { audio.currentTime = 0; audio.play(); }
        else if (repeatMode === 1 || currentIndex < queue.length - 1) { next(); }
    }

    function onTimeUpdate() {
        if (!audio.duration) return;
        var pct = (audio.currentTime / audio.duration) * 100;
        var seek = document.getElementById('playerSeek');
        if (seek) seek.value = pct;
        var cur = document.getElementById('playerCurrentTime');
        if (cur) cur.textContent = fmt(audio.currentTime);
    }

    function onMetadata() {
        var dur = document.getElementById('playerDuration');
        if (dur) dur.textContent = fmt(audio.duration);
    }

    function onSeek(e) {
        if (!audio.duration) return;
        audio.currentTime = (e.target.value / 100) * audio.duration;
    }

    function onVolume(e) {
        audio.volume = e.target.value / 100;
        if (audio.muted) audio.muted = false;
        updateMuteBtn();
    }

    // ── UI updates ────────────────────────────────────────────────
    function updatePlayBtn(playing) {
        var btn = document.getElementById('playerPlay');
        if (!btn) return;
        btn.textContent = playing ? '⏸' : '▶';
        btn.classList.toggle('playing', playing);

        // Sync equalizer bars on all rows
        document.querySelectorAll('.eq-bars').forEach(function (el) {
            el.classList.toggle('playing', playing);
        });
    }

    function updateMuteBtn() {
        var btn = document.getElementById('playerMute');
        if (!btn) return;
        var v = audio.muted ? 0 : audio.volume;
        btn.textContent = v === 0 ? '🔇' : v < 0.5 ? '🔉' : '🔊';
    }

    function updateNowPlayingUI(song) {
        var title  = document.getElementById('playerTitle');
        var artist = document.getElementById('playerArtist');
        var cover  = document.getElementById('playerCover');
        var letter = document.getElementById('playerCoverLetter');
        if (title)  title.textContent  = song.title;
        if (artist) artist.textContent = song.artist;
        if (cover)  cover.style.setProperty('--cover-color', song.coverColor || '#ff8a1f');
        if (letter) letter.textContent = (song.title || '?').charAt(0).toUpperCase();
        var player = document.getElementById('mbPlayer');
        if (player) player.classList.add('has-song');
    }

    function updateNowPlayingRows(songId) {
        // Remove all active states
        document.querySelectorAll('[data-song-id]').forEach(function (el) {
            el.classList.remove('song-row-playing');
        });
        document.querySelectorAll('.eq-bars').forEach(function (el) {
            el.classList.remove('visible');
        });
        document.querySelectorAll('.play-icon').forEach(function (el) {
            el.textContent = '▶';
        });

        // Activate current song's rows
        var rows = document.querySelectorAll('[data-song-id="' + songId + '"]');
        rows.forEach(function (el) {
            el.classList.add('song-row-playing');
            var eq = el.querySelector('.eq-bars');
            if (eq) eq.classList.add('visible');
            var icon = el.querySelector('.play-icon');
            if (icon) icon.textContent = '⏸';
        });
    }

    // ── Persist / restore queue ───────────────────────────────────
    function saveState() {
        try {
            localStorage.setItem('mb_queue', JSON.stringify(queue));
            localStorage.setItem('mb_index', String(currentIndex));
        } catch (e) {}
    }

    function restoreState() {
        try {
            var VER = 'mb_v3';
            if (localStorage.getItem('mb_ver') !== VER) {
                localStorage.removeItem('mb_queue');
                localStorage.removeItem('mb_index');
                localStorage.setItem('mb_ver', VER);
                return;
            }
            var q = localStorage.getItem('mb_queue');
            var i = localStorage.getItem('mb_index');
            if (!q || i === null) return;
            queue = JSON.parse(q);
            currentIndex = parseInt(i, 10);
            var song = queue[currentIndex];
            if (song) {
                updateNowPlayingUI(song);
                // Don't preload audio on restore — Deezer fetch happens on explicit play
            }
        } catch (e) {}
    }

    // ── Helpers ───────────────────────────────────────────────────
    function fmt(s) {
        if (!s || isNaN(s)) return '0:00';
        var m = Math.floor(s / 60);
        var sec = Math.floor(s % 60);
        return m + ':' + (sec < 10 ? '0' : '') + sec;
    }

    function on(id, event, handler) {
        var el = document.getElementById(id);
        if (el) el.addEventListener(event, handler);
    }

    // ── Boot ──────────────────────────────────────────────────────
    document.addEventListener('DOMContentLoaded', function () {
        init();

        // Wire all ▶ play buttons  (data-song='JSON')
        document.querySelectorAll('.js-play-btn').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                try {
                    var data = JSON.parse(this.dataset.song);
                    playSong(data);
                } catch (err) {}
            });
        });

        // Wire "play all" buttons  (data-queue='JSON array')
        document.querySelectorAll('.js-play-all-btn').forEach(function (btn) {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                try {
                    var songs = JSON.parse(this.dataset.queue);
                    setQueue(songs, 0);
                } catch (err) {}
            });
        });
    });

    return { init: init, playSong: playSong, setQueue: setQueue };
})();
