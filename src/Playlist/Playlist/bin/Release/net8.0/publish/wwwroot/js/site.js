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

    // Global search across navigation pages and catalog data.
    var globalSearchInput = document.getElementById('globalSearchInput');
    var globalSearchResults = document.getElementById('globalSearchResults');
    var globalSearchTimer;
    var activeSearchRequest;

    function closeGlobalSearch() {
        if (!globalSearchInput || !globalSearchResults) return;
        globalSearchResults.replaceChildren();
        globalSearchResults.classList.remove('visible');
        globalSearchInput.setAttribute('aria-expanded', 'false');
    }

    function renderGlobalSearch(items) {
        globalSearchResults.replaceChildren();

        if (!items.length) {
            var empty = document.createElement('p');
            empty.className = 'global-search-empty';
            empty.textContent = 'No matching pages or music.';
            globalSearchResults.appendChild(empty);
        } else {
            items.forEach(function (item, index) {
                var link = document.createElement('a');
                link.className = 'global-search-result';
                link.href = item.url;
                link.setAttribute('role', 'option');
                link.dataset.searchIndex = index;

                var type = document.createElement('span');
                type.className = 'global-search-type';
                type.textContent = item.type;

                var text = document.createElement('span');
                text.className = 'global-search-text';

                var title = document.createElement('strong');
                title.textContent = item.title;
                var subtitle = document.createElement('small');
                subtitle.textContent = item.subtitle;

                text.append(title, subtitle);
                link.append(type, text);
                globalSearchResults.appendChild(link);
            });
        }

        globalSearchResults.classList.add('visible');
        globalSearchInput.setAttribute('aria-expanded', 'true');
    }

    if (globalSearchInput && globalSearchResults) {
        globalSearchInput.addEventListener('input', function () {
            clearTimeout(globalSearchTimer);
            var term = globalSearchInput.value.trim();

            if (term.length < 2) {
                closeGlobalSearch();
                return;
            }

            globalSearchTimer = setTimeout(function () {
                if (activeSearchRequest) activeSearchRequest.abort();
                activeSearchRequest = new AbortController();

                fetch('/global-search?term=' + encodeURIComponent(term), {
                    signal: activeSearchRequest.signal,
                    headers: { 'Accept': 'application/json' }
                })
                    .then(function (response) {
                        if (!response.ok) throw new Error('Search request failed.');
                        return response.json();
                    })
                    .then(renderGlobalSearch)
                    .catch(function (error) {
                        if (error.name !== 'AbortError') renderGlobalSearch([]);
                    });
            }, 220);
        });

        globalSearchInput.addEventListener('keydown', function (event) {
            var links = Array.from(globalSearchResults.querySelectorAll('.global-search-result'));
            if (event.key === 'ArrowDown' && links.length) {
                event.preventDefault();
                links[0].focus();
            } else if (event.key === 'Escape') {
                closeGlobalSearch();
                globalSearchInput.blur();
            }
        });

        globalSearchResults.addEventListener('keydown', function (event) {
            var links = Array.from(globalSearchResults.querySelectorAll('.global-search-result'));
            var index = links.indexOf(document.activeElement);
            if (event.key === 'ArrowDown' && index < links.length - 1) {
                event.preventDefault();
                links[index + 1].focus();
            } else if (event.key === 'ArrowUp') {
                event.preventDefault();
                if (index > 0) links[index - 1].focus();
                else globalSearchInput.focus();
            } else if (event.key === 'Escape') {
                closeGlobalSearch();
                globalSearchInput.focus();
            }
        });

        document.addEventListener('keydown', function (event) {
            if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
                event.preventDefault();
                globalSearchInput.focus();
                globalSearchInput.select();
            }
        });

        document.addEventListener('click', function (event) {
            if (!document.getElementById('globalSearch').contains(event.target)) closeGlobalSearch();
        });
    }

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
