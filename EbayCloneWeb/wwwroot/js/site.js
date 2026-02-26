// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function(){
    const apiBase = 'http://localhost:5174/';

    function loadCategories(){
        $.getJSON(apiBase + 'api/Category/list').done(function(data){
            var $sel = $('#categoryId');
            $sel.find('option[value!=""]').remove();
            data.forEach(function(c){
                $sel.append($('<option>').val(c.id).text(c.name));
            });
            // set value from querystring if present
            var current = getQueryParam('categoryId');
            if(current) $sel.val(current);
        }).fail(function(){
            console.warn('Failed to load categories');
        });
    }

    function debounce(fn, delay){
        var t;
        return function(){
            var args = arguments;
            clearTimeout(t);
            t = setTimeout(function(){ fn.apply(null, args); }, delay);
        };
    }

    function getQueryParam(name){
        var params = new URLSearchParams(window.location.search);
        return params.get(name);
    }

    function buildApiUrl(page){
        var q = $('#q').val();
        var categoryId = $('#categoryId').val();
        var orderBy = $('#orderBy').val();
        var sortDir = $('#sortDir').val();
        var pageSize = $('#pageSize').val() || '16';
        var url = apiBase + 'api/Product/list?page=' + (page||1) + '&pageSize=' + pageSize;
        if(q) url += '&q=' + encodeURIComponent(q);
        if(categoryId) url += '&categoryId=' + encodeURIComponent(categoryId);
        if(orderBy) url += '&orderBy=' + encodeURIComponent(orderBy);
        if(sortDir) url += '&sortDir=' + encodeURIComponent(sortDir);
        return url;
    }

    function loadProducts(page, pushState){
        pushState = pushState !== false;
        var url = buildApiUrl(page);
        $.getJSON(url).done(function(res){
            renderProducts(res.items);
            renderPagination(res.total, parseInt($('#pageSize').val()||16), page||1);
            // update server-rendered pagination and product area when server initially rendered
            // replace server-side pagination markup
            var $serverPag = $('#pagination');
            if($serverPag.length){
                $serverPag.empty();
                for(var i=1;i<=Math.ceil(res.total/parseInt($('#pageSize').val()||16));i++){
                    var li = $('<li>').addClass('page-item ' + (i==page? 'active':''));
                    var a = $('<a>').addClass('page-link').attr('href','#').attr('data-page',i).text(i);
                    li.append(a);
                    $serverPag.append(li);
                }
            }
            if(pushState){
                var params = new URLSearchParams(window.location.search);
                params.set('page', page || 1);
                params.set('pageSize', $('#pageSize').val() || 16);
                if($('#q').val()) params.set('q', $('#q').val()); else params.delete('q');
                if($('#categoryId').val()) params.set('categoryId', $('#categoryId').val()); else params.delete('categoryId');
                if($('#orderBy').val()) params.set('orderBy', $('#orderBy').val()); else params.delete('orderBy');
                if($('#sortDir').val()) params.set('sortDir', $('#sortDir').val()); else params.delete('sortDir');
                var newUrl = window.location.pathname + '?' + params.toString();
                history.replaceState(null, '', newUrl);
            }
        }).fail(function(){
            console.warn('Failed to load products');
        });
    }

    function renderProducts(items){
        var $row = $('#productGridRow');
        // remove existing product cols
        $row.empty();
        items.forEach(function(p){
            var img = '/images/placeholder.png';
            if(p.images){
                try{
                    var parsed = JSON.parse(p.images);
                    if(Array.isArray(parsed) && parsed.length) img = parsed[0];
                }catch(e){
                    if(typeof p.images === 'string') img = p.images;
                }
            }
            var sellerText = 'Seller: unknown';
            if(p.seller){ sellerText = 'Seller: ' + (p.seller.email || p.seller.username || ('#'+p.seller.id)); }

            var col = $('<div class="col-lg-3 col-md-4 col-sm-6 mb-4">').append(
                $('<div class="card h-100 border rounded shadow-sm">').append(
                    $('<div class="ratio ratio-4x3">').append($('<img>').attr('src', img).addClass('card-img-top rounded-top').css('object-fit','cover')),
                    $('<div class="card-body d-flex flex-column">').append(
                        $('<h5>').addClass('card-title').text(p.title),
                        $('<p>').addClass('text-muted small mb-2').text(p.description),
                        $('<div>').addClass('d-flex align-items-center mb-2').append(
                            $('<h5>').addClass('text-danger mb-0 me-3').text('$' + (p.price||0).toFixed(2)),
                            $('<span>').addClass('badge bg-success ms-auto').text('⭐ 4.0')
                        ),
                        $('<div>').addClass('mb-3 text-muted small').html('<i class="bi bi-person-fill"></i> ' + sellerText),
                        $('<div>').addClass('mt-auto').append(
                            $('<a>').addClass('btn btn-primary w-100').attr('href','/Order/Create?id='+p.id).html('<i class="bi bi-cart-fill"></i> Buy Now')
                        )
                    )
                )
            );
            $row.append(col);
        });
    }

    function renderPagination(total, pageSize, current){
        var totalPages = Math.ceil(total / pageSize);
        var $pag = $('.pagination').empty();
        for(var i=1;i<=totalPages;i++){
            var li = $('<li>').addClass('page-item ' + (i===current? 'active':''));
            var a = $('<a>').addClass('page-link').attr('href','#').text(i).data('page',i);
            li.append(a);
            $pag.append(li);
        }
    }

    // initial
    loadCategories();
    var initialPage = parseInt(getQueryParam('page')||'1');
    // initialise form values from querystring
    if(getQueryParam('q')) $('#q').val(getQueryParam('q'));
    if(getQueryParam('categoryId')) $('#categoryId').val(getQueryParam('categoryId'));
    if(getQueryParam('orderBy')) $('#orderBy').val(getQueryParam('orderBy'));
    if(getQueryParam('sortDir')) $('#sortDir').val(getQueryParam('sortDir'));
    if(getQueryParam('pageSize')) $('#pageSize').val(getQueryParam('pageSize'));

    loadProducts(initialPage);

    // filter actions (live)
    var liveLoad = debounce(function(){ loadProducts(1); }, 150);
    $('#q').on('input', liveLoad);
    // hero search sync
    $('#heroQ').on('input', function(){ $('#q').val($(this).val()); liveLoad(); });
    $('#categoryId, #orderBy, #sortDir').on('change', function(){ loadProducts(1); });
    $('#pageSize').on('change', function(){ loadProducts(1); });
    $('#clearFilters').on('click', function(){ $('#q').val(''); $('#categoryId').val(''); $('#orderBy').val(''); $('#sortDir').val('asc'); $('#pageSize').val('16'); loadProducts(1); });

    $(document).on('click', '.pagination .page-link', function(e){ e.preventDefault(); var p = $(this).data('page'); loadProducts(p); });

    // thumbnail click for product detail (delegated)
    $(document).on('click', '.thumb-img', function(){
        var src = $(this).data('src');
        var $activeImg = $('#productCarousel .carousel-inner .active img');
        if($activeImg.length) $activeImg.attr('src', src);
    });
});
