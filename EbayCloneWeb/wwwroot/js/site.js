$(function () {
    var apiBase = '/';

    /* -------------------------------------------------------
       HELPERS
    ------------------------------------------------------- */
    function debounce(fn, delay) {
        var t;
        return function () {
            var args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(null, args); }, delay);
        };
    }

    function getQueryParam(name) {
        return new URLSearchParams(window.location.search).get(name);
    }

    /* -------------------------------------------------------
       COMBINED SORT SELECT helpers
       Maps sortSelect value  <->  orderBy + sortDir pair
    ------------------------------------------------------- */
    function applySortSelect(val) {
        if (!val) {
            $('#orderBy').val('');
            $('#sortDir').val('');
        } else {
            var parts = val.split('_');
            $('#orderBy').val(parts[0] || '');
            $('#sortDir').val(parts[1] || 'asc');
        }
    }

    function restoreSortSelect(orderBy, sortDir) {
        if (!orderBy) { $('#sortSelect').val(''); return; }
        var combined = orderBy + '_' + (sortDir || 'asc');
        if ($('#sortSelect option[value="' + combined + '"]').length) {
            $('#sortSelect').val(combined);
        }
    }

    /* -------------------------------------------------------
       CATEGORIES  —  populate:
         #navCatId    (navbar dropdown)
         #categoryId  (hidden Index filter input)
         #ebCatStrip  (horizontal category nav)
    ------------------------------------------------------- */
    function loadCategories() {
        $.getJSON(apiBase + 'api/Category/list')
            .done(function (data) {
                var $navSel = $('#navCatId');
                var $filterSel = $('#categoryId');
                var $strip = $('#ebCatStrip');
                var current = getQueryParam('categoryId');

                // Navbar dropdown
                if ($navSel.length) {
                    $navSel.find('option[value!=""]').remove();
                    data.forEach(function (c) {
                        $navSel.append($('<option>').val(c.id).text(c.name));
                    });
                    if (current) $navSel.val(current);
                }

                // Hidden filter input on Index page
                if ($filterSel.length && current) {
                    $filterSel.val(current);
                }

                // Category strip (present on all pages)
                if ($strip.length) {
                    $strip.find('a:not(.cat-all-link)').remove();
                    data.forEach(function (c) {
                        var active = current && String(current) === String(c.id) ? ' eb-cat-active' : '';
                        $strip.append(
                            $('<a>')
                                .addClass('eb-cat-link' + active)
                                .attr('href', '/?categoryId=' + c.id)
                                .text(c.name)
                        );
                    });
                    // highlight "All" when no category selected
                    if (!current) $strip.find('a').first().addClass('eb-cat-active');
                }
            })
            .fail(function () { console.warn('Failed to load categories'); });
    }

    /* -------------------------------------------------------
       PRODUCT API URL builder
    ------------------------------------------------------- */
    function buildApiUrl(page) {
        var q = $('#q').val();
        var categoryId = $('#categoryId').val();
        var orderBy = $('#orderBy').val();
        var sortDir = $('#sortDir').val();
        var pageSize = $('#pageSize').val() || '16';
        var url = apiBase + 'api/Product/list?page=' + (page || 1) + '&pageSize=' + pageSize;
        if (q) url += '&q=' + encodeURIComponent(q);
        if (categoryId) url += '&categoryId=' + encodeURIComponent(categoryId);
        if (orderBy) url += '&orderBy=' + encodeURIComponent(orderBy);
        if (sortDir) url += '&sortDir=' + encodeURIComponent(sortDir);
        return url;
    }

    /* -------------------------------------------------------
       LOAD PRODUCTS  (only runs on Index page)
    ------------------------------------------------------- */
    function loadProducts(page) {
        if (!$('#productGridRow').length) return;

        $.getJSON(buildApiUrl(page))
            .done(function (res) {
                renderProducts(res.items);
                renderPagination(res.total, parseInt($('#pageSize').val() || 16), page || 1);

                // update result-count text
                $('.eb-results-count').text((res.total || 0) + ' results');

                // push browser history state
                var params = new URLSearchParams(window.location.search);
                params.set('page', page || 1);
                params.set('pageSize', $('#pageSize').val() || 16);
                if ($('#q').val()) params.set('q', $('#q').val()); else params.delete('q');
                if ($('#categoryId').val()) params.set('categoryId', $('#categoryId').val()); else params.delete('categoryId');
                if ($('#orderBy').val()) params.set('orderBy', $('#orderBy').val()); else params.delete('orderBy');
                if ($('#sortDir').val()) params.set('sortDir', $('#sortDir').val()); else params.delete('sortDir');
                history.replaceState(null, '', window.location.pathname + '?' + params.toString());
            })
            .fail(function () { console.warn('Failed to load products'); });
    }

    /* -------------------------------------------------------
       RENDER PRODUCTS  —  eBay-style cards
    ------------------------------------------------------- */
    function renderProducts(items) {
        var $row = $('#productGridRow').empty();
        if (!items || !items.length) {
            $row.html(
                '<div class="col-12 text-center py-5 text-muted">' +
                '<i class="bi bi-search" style="font-size:3rem;opacity:.3"></i>' +
                '<p class="mt-3">No products found. Try a different search.</p></div>'
            );
            return;
        }
        items.forEach(function (p) {
            var img = '/images/placeholder.png';
            if (p.images) {
                try {
                    var parsed = JSON.parse(p.images);
                    if (Array.isArray(parsed) && parsed.length) img = parsed[0];
                } catch (e) {
                    if (typeof p.images === 'string') img = p.images;
                }
            }
            var sellerText = 'unknown';
            if (p.seller) sellerText = p.seller.username || p.seller.email || ('#' + p.seller.id);

            var col = $('<div class="col-xl-2 col-lg-3 col-md-4 col-sm-6">').append(
                $('<div class="eb-card">').append(
                    $('<div class="eb-card-img-wrap">').append(
                        $('<img loading="lazy">').attr('src', img).attr('alt', p.title || '')
                    ),
                    $('<div class="eb-card-body">').append(
                        $('<div class="eb-card-title">').text(p.title || ''),
                        $('<div class="eb-card-price">').text('US $' + (p.price || 0).toFixed(2)),
                        $('<div class="eb-card-price-alt">').text('or Best Offer'),
                        $('<div class="eb-card-shipping">').html('<i class="bi bi-truck"></i> Free shipping'),
                        $('<div class="eb-card-seller">').html('<i class="bi bi-person-fill"></i> ' + sellerText),
                        $('<a class="eb-btn-buy">').attr('href', '/Order/Create?id=' + p.id)
                            .html('<i class="bi bi-cart-fill"></i> Buy It Now')
                    )
                )
            );
            $row.append(col);
        });
    }

    /* -------------------------------------------------------
       RENDER PAGINATION
    ------------------------------------------------------- */
    function renderPagination(total, pageSize, current) {
        var totalPages = Math.ceil(total / pageSize);
        var $pag = $('.pagination').empty();
        for (var i = 1; i <= totalPages; i++) {
            var li = $('<li>').addClass('page-item' + (i === current ? ' active' : ''));
            var a = $('<a>').addClass('page-link').attr('href', '#').text(i).data('page', i);
            $pag.append(li.append(a));
        }
    }

    /* -------------------------------------------------------
       INIT  —  restore URL query params into UI controls
    ------------------------------------------------------- */
    loadCategories();

    var qs_q = getQueryParam('q');
    var qs_categoryId = getQueryParam('categoryId');
    var qs_orderBy = getQueryParam('orderBy');
    var qs_sortDir = getQueryParam('sortDir');
    var qs_pageSize = getQueryParam('pageSize');
    var qs_page = parseInt(getQueryParam('page') || '1');

    if (qs_q) { $('#q').val(qs_q); $('#heroQ').val(qs_q); }
    if (qs_categoryId) { $('#categoryId').val(qs_categoryId); }
    if (qs_orderBy) { $('#orderBy').val(qs_orderBy); }
    if (qs_sortDir) { $('#sortDir').val(qs_sortDir); }
    if (qs_pageSize) { $('#pageSize').val(qs_pageSize); }

    // Restore the combined sort dropdown
    restoreSortSelect(qs_orderBy, qs_sortDir);

    // Initial product load
    loadProducts(qs_page);

    /* -------------------------------------------------------
       EVENT BINDINGS
    ------------------------------------------------------- */
    var liveLoad = debounce(function () { loadProducts(1); }, 220);

    // Navbar search → sync to hidden #q → reload
    $('#heroQ').on('input', function () {
        $('#q').val($(this).val());
        liveLoad();
    });

    // Navbar category → sync to hidden #categoryId → reload / navigate
    $('#navCatId').on('change', function () {
        var val = $(this).val();
        if ($('#productGridRow').length) {
            $('#categoryId').val(val);
            loadProducts(1);
        } else {
            var params = new URLSearchParams(window.location.search);
            if (val) params.set('categoryId', val); else params.delete('categoryId');
            window.location.href = '/?' + params.toString();
        }
    });

    // Combined sort select → split into orderBy + sortDir → reload
    $('#sortSelect').on('change', function () {
        applySortSelect($(this).val());
        loadProducts(1);
    });

    // Items-per-page
    $('#pageSize').on('change', function () { loadProducts(1); });

    // Clear all filters
    $('#clearFilters').on('click', function () {
        $('#q').val('');
        $('#heroQ').val('');
        $('#categoryId').val('');
        $('#navCatId').val('');
        $('#orderBy').val('');
        $('#sortDir').val('');
        $('#sortSelect').val('');
        $('#pageSize').val('16');
        loadProducts(1);
    });

    // Pagination
    $(document).on('click', '.pagination .page-link', function (e) {
        e.preventDefault();
        var p = $(this).data('page');
        if (p) loadProducts(p);
    });

    // Thumbnail click on product detail page
    $(document).on('click', '.thumb-img', function () {
        $('#productCarousel .carousel-inner .active img').attr('src', $(this).data('src'));
        $('.eb-thumb').removeClass('eb-thumb-active');
        $(this).addClass('eb-thumb-active');
    });
});
