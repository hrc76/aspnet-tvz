/* MusicBar — site.js */
$(function () {

    // ── Flatpickr datepicker ─────────────────────────────────────
    if (typeof flatpickr !== 'undefined') {
        $('.js-datepicker').each(function () {
            flatpickr(this, {
                enableTime: true,
                dateFormat: 'd.m.Y H:i',
                time_24hr: true,
                allowInput: true,
                defaultDate: $(this).val() || null
            });
        });
    }

    // ── Legacy datetime validation (non-Flatpickr fallback) ──────
    $('.js-datetime').on('blur', function () {
        var val = $(this).val().trim();
        var ok  = /^\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}$/.test(val);
        $(this).toggleClass('input-error', val.length > 0 && !ok);
    });

    // ── Song row hover: show play button ─────────────────────────
    $(document).on('mouseenter', '.entity-row[data-song-id]', function () {
        $(this).find('.play-btn').css('opacity', '1');
    }).on('mouseleave', '.entity-row[data-song-id]', function () {
        if (!$(this).hasClass('song-row-playing')) {
            $(this).find('.play-btn').css('opacity', '0.35');
        }
    });

    $('.entity-row[data-song-id]:not(.song-row-playing) .play-btn').css('opacity', '0.35');

});

// ── Album art: dynamically load covers from iTunes Search API ────
function fetchAlbumArt(root) {
    var wraps = (root || document).querySelectorAll('.album-cover-wrap[data-album-search]');
    wraps.forEach(function (wrap) {
        if (wrap.querySelector('.album-cover-real')) return;
        var query = wrap.dataset.albumSearch;
        if (!query) return;
        fetch('https://itunes.apple.com/search?term=' + query + '&entity=album&limit=3&media=music')
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (!data.results || !data.results.length) return;
                var artUrl = data.results[0].artworkUrl100;
                if (!artUrl) return;
                artUrl = artUrl.replace('100x100bb', '600x600bb').replace('100x100', '600x600');
                var img = document.createElement('img');
                img.src = artUrl;
                img.className = 'album-cover-real';
                img.loading = 'lazy';
                img.alt = '';
                img.onerror = function () { this.remove(); };
                wrap.insertBefore(img, wrap.firstChild);
            })
            .catch(function () {});
    });
}

window.fetchAlbumArt = fetchAlbumArt;
fetchAlbumArt();
