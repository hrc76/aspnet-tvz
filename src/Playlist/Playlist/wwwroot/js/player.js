/* ═══════════════════════════════════════════════════════════
   MusicBar Player  —  HTML5 audio engine
   ═══════════════════════════════════════════════════════════ */
var MBPlayer = (function () {
    'use strict';

    var queue        = [];
    var currentIndex = -1;
    var isShuffling  = false;
    var isSmartQueue = false;
    var isExtendingQueue = false;
    var repeatMode   = 0;   // 0=off  1=all  2=one
    var audio;
    var listenedMilliseconds = 0;
    var listeningStartedAt = null;
    var listeningTimer = null;
    var historyRecorded = false;

    // ── Initialise ───────────────────────────────────────────────
    function init() {
        audio = document.getElementById('mbAudio');
        if (!audio) return;

        bindControls();
        bindAudioEvents();
        bindKeyboard();
        restoreState();
        renderQueue();
    }

    function bindControls() {
        on('playerPlay',    'click', togglePlay);
        on('playerNext',    'click', next);
        on('playerPrev',    'click', prev);
        on('playerShuffle', 'click', toggleShuffle);
        on('playerRepeat',  'click', toggleRepeat);
        on('playerMute',    'click', toggleMute);
        on('playerLike',    'click', toggleLike);
        on('playerQueueToggle', 'click', toggleQueueDrawer);
        on('playerQueueClose',  'click', closeQueueDrawer);
        on('playerQueueClear',  'click', clearQueue);
        on('playerSmartQueue',  'click', toggleSmartQueue);

        var seek = document.getElementById('playerSeek');
        if (seek) seek.addEventListener('input', onSeek);

        var vol = document.getElementById('playerVolume');
        if (vol) {
            vol.addEventListener('input', onVolume);
            restoreAudioPreferences(vol);
        }
    }

    function bindAudioEvents() {
        audio.addEventListener('timeupdate',    onTimeUpdate);
        audio.addEventListener('ended',         onEnded);
        audio.addEventListener('loadedmetadata',onMetadata);
        audio.addEventListener('play',          function () {
            startListeningClock();
            updatePlayBtn(true);
        });
        audio.addEventListener('pause',         function () {
            pauseListeningClock();
            updatePlayBtn(false);
        });
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
        ensureQueueVisibility();
        loadAndPlay();
    }

    // ── Public: replace queue and play from index ─────────────────
    function setQueue(songs, startIndex) {
        queue = songs;
        currentIndex = startIndex || 0;
        ensureQueueVisibility();
        renderQueue();
        loadAndPlay();
    }

    function addToQueue(songData) {
        if (!songData || !songData.id) return;
        queue.push(songData);
        if (currentIndex < 0) {
            currentIndex = 0;
            updateNowPlayingUI(songData);
        }
        saveState();
        renderQueue();
        showQueueToast(songData.title + ' added to queue');
    }

    // ── Internal playback ─────────────────────────────────────────
    function loadAndPlay() {
        var song = queue[currentIndex];
        if (!song) return;
        resetListeningClock();
        updateNowPlayingUI(song);
        updateNowPlayingRows(song.id);
        saveState();
        renderQueue();

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
        saveAudioPreferences();
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
        saveAudioPreferences();
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
        else if (isSmartQueue) { extendSmartQueue(); }
    }

    function toggleSmartQueue() {
        // Smart Queue po potrebi trazi slicne pjesme od servera kada dode do kraja reda.
        isSmartQueue = !isSmartQueue;
        try { localStorage.setItem('mb_smart_queue', isSmartQueue ? '1' : '0'); } catch (e) {}
        updateSmartQueueButton();
        showQueueToast('Smart Queue ' + (isSmartQueue ? 'enabled' : 'disabled'));
    }

    function updateSmartQueueButton() {
        var button = document.getElementById('playerSmartQueue');
        if (!button) return;
        button.textContent = 'Smart: ' + (isSmartQueue ? 'on' : 'off');
        button.setAttribute('aria-pressed', String(isSmartQueue));
    }

    function extendSmartQueue() {
        var seed = queue[currentIndex];
        if (!seed || isExtendingQueue) return;
        isExtendingQueue = true;
        var params = new URLSearchParams({ seedSongId: String(seed.id) });
        queue.forEach(function (song) { params.append('excludeIds', String(song.id)); });
        fetch('/Song/SmartQueue?' + params.toString(), { credentials: 'same-origin' })
            .then(function (response) { return response.ok ? response.json() : []; })
            .then(function (songs) {
                songs.forEach(function (song) {
                    if (!queue.some(function (queued) { return Number(queued.id) === Number(song.id); })) queue.push(song);
                });
                if (currentIndex < queue.length - 1) {
                    saveState();
                    renderQueue();
                    next();
                    showQueueToast('Smart Queue added ' + songs.length + ' tracks');
                }
            })
            .catch(function () {})
            .finally(function () { isExtendingQueue = false; });
    }

    function onTimeUpdate() {
        if (!audio.duration) return;
        var pct = (audio.currentTime / audio.duration) * 100;
        var seek = document.getElementById('playerSeek');
        if (seek) seek.value = pct;
        var cur = document.getElementById('playerCurrentTime');
        if (cur) cur.textContent = fmt(audio.currentTime);
    }

    function resetListeningClock() {
        // Svaka nova pjesma dobiva vlastiti brojac; pauza ne smije glumiti slusanje.
        if (listeningTimer !== null) clearTimeout(listeningTimer);
        listeningTimer = null;
        listeningStartedAt = null;
        listenedMilliseconds = 0;
        historyRecorded = false;
    }

    function startListeningClock() {
        // History se biljezi tek nakon ukupno pet stvarnih sekundi reprodukcije.
        if (historyRecorded || currentIndex < 0 || listeningStartedAt !== null) return;
        listeningStartedAt = performance.now();
        var remaining = Math.max(0, 5000 - listenedMilliseconds);
        listeningTimer = window.setTimeout(function () {
            listeningTimer = null;
            if (audio.paused || historyRecorded) return;
            listenedMilliseconds = 5000;
            listeningStartedAt = null;
            recordListeningHistory(queue[currentIndex]);
        }, remaining);
    }

    function pauseListeningClock() {
        if (listeningStartedAt !== null) {
            listenedMilliseconds += performance.now() - listeningStartedAt;
            listeningStartedAt = null;
        }
        if (listeningTimer !== null) {
            clearTimeout(listeningTimer);
            listeningTimer = null;
        }
    }

    function recordListeningHistory(song) {
        var songId = Number(song && song.id);
        if (!Number.isInteger(songId) || songId <= 0) return;
        historyRecorded = true;
        var token = document.querySelector('#playerAntiForgeryToken input[name="__RequestVerificationToken"]');
        if (!token) return;

        fetch('/ListeningHistory/RecordPlay', {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token.value
            },
            body: JSON.stringify({ songId: songId })
        }).catch(function () {
            // Playback must continue even if history logging is temporarily unavailable.
        });
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
        saveAudioPreferences();
        updateMuteBtn();
    }

    function restoreAudioPreferences(volumeControl) {
        // localStorage pripada browseru pa glasnoca ostaje ista nakon navigacije/refresha.
        var storedVolume = null;
        var storedMuted = false;
        try {
            storedVolume = localStorage.getItem('mb_volume');
            storedMuted = localStorage.getItem('mb_muted') === '1';
        } catch (e) {}

        var volume = storedVolume === null ? 0.5 : Number(storedVolume);
        if (!Number.isFinite(volume)) volume = 0.5;
        volume = Math.max(0, Math.min(1, volume));
        audio.volume = volume;
        audio.muted = storedMuted;
        volumeControl.value = String(Math.round(volume * 100));
        updateMuteBtn();
    }

    function saveAudioPreferences() {
        try {
            localStorage.setItem('mb_volume', String(audio.volume));
            localStorage.setItem('mb_muted', audio.muted ? '1' : '0');
        } catch (e) {}
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

    function ensureQueueVisibility() {
        var player = document.getElementById('mbPlayer');
        var toggle = document.getElementById('playerQueueToggle');
        if (!player || !toggle) return;
        if (!player.classList.contains('has-song')) {
            player.classList.add('has-song');
        }
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

    // Queue drawer
    var queueDrawerTimer = null;

    function toggleQueueDrawer() {
        var drawer = document.getElementById('playerQueueDrawer');
        var toggle = document.getElementById('playerQueueToggle');
        if (!drawer) return;
        var opening = !toggle || toggle.getAttribute('aria-expanded') !== 'true';
        if (!opening) {
            closeQueueDrawer();
            return;
        }

        if (queueDrawerTimer) clearTimeout(queueDrawerTimer);
        drawer.hidden = false;
        drawer.classList.remove('is-closing');
        renderQueue();
        requestAnimationFrame(function () {
            drawer.classList.add('is-open');
        });
        if (toggle) toggle.setAttribute('aria-expanded', 'true');
    }

    function closeQueueDrawer() {
        var drawer = document.getElementById('playerQueueDrawer');
        var toggle = document.getElementById('playerQueueToggle');
        if (drawer && !drawer.hidden) {
            drawer.classList.remove('is-open');
            drawer.classList.add('is-closing');
            if (queueDrawerTimer) clearTimeout(queueDrawerTimer);
            queueDrawerTimer = setTimeout(function () {
                drawer.hidden = true;
                drawer.classList.remove('is-closing');
                queueDrawerTimer = null;
            }, 280);
        }
        if (toggle) toggle.setAttribute('aria-expanded', 'false');
    }

    function clearQueue() {
        queue = [];
        currentIndex = -1;
        resetListeningClock();
        audio.pause();
        audio.removeAttribute('src');
        audio.load();
        var player = document.getElementById('mbPlayer');
        if (player) player.classList.remove('has-song');
        var title = document.getElementById('playerTitle');
        var artist = document.getElementById('playerArtist');
        if (title) title.textContent = '—';
        if (artist) artist.textContent = 'Pick a song to play';
        saveState();
        renderQueue();
    }

    function removeFromQueue(index) {
        if (index < 0 || index >= queue.length) return;
        var removingCurrent = index === currentIndex;
        queue.splice(index, 1);
        if (!queue.length) {
            clearQueue();
            return;
        }
        if (index < currentIndex) currentIndex--;
        if (removingCurrent) {
            currentIndex = Math.min(currentIndex, queue.length - 1);
            loadAndPlay();
        } else {
            saveState();
            renderQueue();
        }
    }

    function renderQueue() {
        var list = document.getElementById('playerQueueList');
        var empty = document.getElementById('playerQueueEmpty');
        var count = document.getElementById('playerQueueCount');
        if (count) count.textContent = String(queue.length);
        if (!list || !empty) return;
        list.replaceChildren();
        empty.hidden = queue.length > 0;

        queue.forEach(function (song, index) {
            var row = document.createElement('div');
            row.className = 'player-queue-item' + (index === currentIndex ? ' current' : '');

            var play = document.createElement('button');
            play.type = 'button';
            play.className = 'player-queue-item-play';
            play.textContent = index === currentIndex && !audio.paused ? 'Ⅱ' : '▶';
            play.setAttribute('aria-label', 'Play ' + song.title);
            play.addEventListener('click', function () { currentIndex = index; loadAndPlay(); });

            var meta = document.createElement('div');
            meta.className = 'player-queue-item-meta';
            var title = document.createElement('strong');
            title.textContent = song.title || 'Unknown song';
            var artist = document.createElement('span');
            artist.textContent = song.artist || 'Unknown artist';
            meta.append(title, artist);

            var state = document.createElement('span');
            state.className = 'player-queue-item-state';
            state.textContent = index === currentIndex ? 'NOW' : String(index + 1).padStart(2, '0');

            var remove = document.createElement('button');
            remove.type = 'button';
            remove.className = 'player-queue-item-remove';
            remove.textContent = 'X';
            remove.setAttribute('aria-label', 'Remove ' + song.title + ' from queue');
            remove.addEventListener('click', function () { removeFromQueue(index); });

            row.append(play, meta, state, remove);
            list.appendChild(row);
        });
    }

    function showQueueToast(message) {
        var oldToast = document.querySelector('.queue-toast');
        if (oldToast) oldToast.remove();
        var toast = document.createElement('div');
        toast.className = 'queue-toast';
        toast.textContent = message;
        document.body.appendChild(toast);
        window.setTimeout(function () { toast.remove(); }, 2200);
    }

    // Queue se sprema lokalno kako promjena Razor stranice ne bi obrisala red pjesama.
    function saveState() {
        try {
            localStorage.setItem('mb_queue', JSON.stringify(queue));
            localStorage.setItem('mb_index', String(currentIndex));
        } catch (e) {}
    }

    function restoreState() {
        try {
            isSmartQueue = localStorage.getItem('mb_smart_queue') === '1';
            updateSmartQueueButton();
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

        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.js-queue-btn');
            if (!btn) return;
            e.preventDefault();
            e.stopPropagation();
            try { addToQueue(JSON.parse(btn.dataset.song)); } catch (err) {}
        });
    });

    return { init: init, playSong: playSong, setQueue: setQueue, addToQueue: addToQueue };
})();
