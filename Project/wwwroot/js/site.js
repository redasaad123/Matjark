(function () {
    "use strict";

    /* =========================================================
       DATA
    ========================================================= */

    let products = [];

    const catLabel = {
        man: "رجالي",
        women: "حريمي",
        accessories: "إكسسوار"
    };

    const catClass = {
        man: "cat-man",
        women: "cat-women",
        accessories: "cat-accessories"
    };

    let currentFilter = "all";
    let searchTerm = "";
    let selectedSizes = {};
    let cart = [];

    try {
        const saved = localStorage.getItem("matgark_cart");
        if (saved) {
            cart = JSON.parse(saved);
        }
    } catch (e) {
        cart = [];
    }

    function saveCart() {
        try {
            localStorage.setItem("matgark_cart", JSON.stringify(cart));
        } catch (e) {}
    }

    const fmt = n =>
        Number(n).toLocaleString("ar-EG") + " ج.م";


    /* =========================================================
       NORMALIZE PRODUCT
    ========================================================= */

    function normalizeProduct(p) {

        const rawSizes =
            p.sizes ||
            p.Sizes ||
            [];

        const rawOld =
            p.oldPrice !== undefined && p.oldPrice !== null
                ? p.oldPrice
                : (
                    p.OldPrice !== undefined && p.OldPrice !== null
                        ? p.OldPrice
                        : (
                            p.old !== undefined && p.old !== null
                                ? p.old
                                : null
                        )
                );


        /*
         * Images
         *
         * ممكن تكون:
         *
         * Images: ["a.jpg", "b.jpg"]
         *
         * أو:
         *
         * Images: [
         *   { ImageName: "a.jpg" },
         *   { ImageName: "b.jpg" }
         * ]
         */

        const rawImages =
            p.GetImages ||
            p.GetImages ||
            [];


        const images =
            Array.isArray(rawImages)
                ? rawImages
                    .map(img => {

                        // String
                        if (typeof img === "string") {
                            return img;
                        }

                        // Object
                        if (
                            typeof img === "object" &&
                            img !== null
                        ) {
                            return (
                                img.imageName ||
                                img.ImageName ||
                                img.fileName ||
                                img.FileName ||
                                img.name ||
                                img.Name ||
                                ""
                            );
                        }

                        return "";
                    })
                    .filter(img => img !== "")
                : [];


        return {

            id:
                p.id !== undefined && p.id !== null
                    ? String(p.id)
                    : (p.Id !== undefined && p.Id !== null ? String(p.Id) : ""),

            name:
                p.name ||
                p.Name ||
                "",

            cat:
                (
                    p.cat ||
                    p.Cat ||
                    "man"
                ).toLowerCase(),

            price:
                Number(
                    p.price !== undefined
                        ? p.price
                        : (p.Price ?? 0)
                ),

            old:
                rawOld !== null
                    ? Number(rawOld)
                    : null,

            icon:
                p.icon ||
                p.Icon ||
                "ic-tshirt",

            images: images,

            sizes:
                Array.isArray(rawSizes) &&
                    rawSizes.length > 0
                    ? rawSizes
                    : ["مقاس واحد"],

            isDiscounted:
                p.isDiscounted !== undefined && p.isDiscounted !== null
                    ? Boolean(p.isDiscounted)
                    : (
                        p.IsDiscounted !== undefined && p.IsDiscounted !== null
                            ? Boolean(p.IsDiscounted)
                            : false
                    )
        };
    }


    /* =========================================================
       PRODUCT MEDIA
    ========================================================= */

    function renderProductMedia(p) {

        /*
         * مفيش صور
         */

        if (
            !p.images ||
            p.images.length === 0
        ) {

            return `
                <svg>
                    <use href="#${p.icon}"></use>
                </svg>
            `;
        }


        /*
         * صورة واحدة
         */

        if (p.images.length === 1) {

            return `
                <img
                    src="/Images/${p.images[0]}"
                    class="product-image"
                    alt="${p.name}"
                >
            `;
        }


        /*
         * أكتر من صورة
         */

        const carouselId =
            `carousel-${p.id}`;


        return `
            <div
                id="${carouselId}"
                class="carousel slide product-carousel"
                data-bs-ride="false"
            >

                <div class="carousel-inner">

                    ${p.images.map((img, index) => `

                        <div
                            class="carousel-item ${index === 0
                ? "active"
                : ""
            }"
                        >

                            <img
                                src="/Images/${img}"
                                class="d-block product-image img-fluid"
                                alt="${p.name} "
                            >

                        </div>

                    `).join("")}

                </div>


                <button
                    class="carousel-control-prev"
                    type="button"
                    data-bs-target="#${carouselId}"
                    data-bs-slide="prev"
                >

                    <span
                        class="carousel-control-prev-icon"
                    ></span>

                </button>


                <button
                    class="carousel-control-next"
                    type="button"
                    data-bs-target="#${carouselId}"
                    data-bs-slide="next"
                >

                    <span
                        class="carousel-control-next-icon"
                    ></span>

                </button>

            </div>
        `;
    }


    /* =========================================================
       FILTER
    ========================================================= */

    function matchesFilter(p) {

        if (currentFilter === "all")
            return true;

        if (currentFilter === "sale" || currentFilter === "sales") {
            return p.isDiscounted === true || p.IsDiscounted === true;
        }

        return p.cat === currentFilter;
    }


    /* =========================================================
       SEARCH
    ========================================================= */

    function matchesSearch(p) {

        if (!searchTerm)
            return true;

        const category =
            catLabel[p.cat] || p.cat;

        return (
            p.name
                .toLowerCase()
                .includes(searchTerm) ||

            category.includes(searchTerm)
        );
    }


    /* =========================================================
       RENDER PRODUCTS
    ========================================================= */

    function renderProducts() {

        const grid =
            document.getElementById(
                "productGrid"
            );


        if (!grid)
            return;


        const list =
            products.filter(
                p =>
                    matchesFilter(p) &&
                    matchesSearch(p)
            );


        grid.innerHTML = "";


        if (list.length === 0) {

            grid.innerHTML = `
                <div class="empty-state">
                    مفيش منتجات مطابقة للبحث حالياً
                    — جرّب كلمة تانية.
                </div>
            `;

            return;
        }


        list.forEach(p => {

            if (!selectedSizes[p.id]) {

                selectedSizes[p.id] =
                    p.sizes[0];
            }


            const card =
                document.createElement("div");


            card.className = "card";


            card.innerHTML = `

                <a href="/Home/Details/${p.id}" class="card-media-link" style="display: block; text-decoration: none; color: inherit;">
                    <div
                        class="card-media ${catClass[p.cat] ||
                    "cat-man"
                    }"
                    >

                        ${p.isDiscounted
                        ? `
                                    <div class="tag-fold">
                                        <span>خصم</span>
                                    </div>
                                `
                        : ""
                    }
                        ${renderProductMedia(p)}

                    </div>
                </a>


                <div class="card-body">

                    <span class="card-cat">
                        ${catLabel[p.cat] ||
                    p.cat
                    }
                    </span>


                    <a href="/Home/Details/${p.id}" class="card-name-link" style="text-decoration: none; color: inherit;">
                        <h3 class="card-name">
                            ${p.name}
                        </h3>
                    </a>


                    <div class="price-row">

                        <span class="price">
                            ${fmt(p.price)}
                        </span>

                        ${(p.isDiscounted)
                        ? `
                                    <span class="price-old">
                                        ${fmt(p.old)}
                                    </span>
                                `
                        : ""
                    }

                    </div>


                    <div
                        class="size-row"
                        data-pid="${p.id}"
                    >

                        ${p.sizes.map(s => `

                            <button
                                type="button"
                                class="size-chip ${s ===
                            selectedSizes[p.id]
                            ? "selected"
                            : ""
                        }"
                                data-size="${s}"
                            >
                                ${s}
                            </button>

                        `).join("")}

                    </div>


                    <button
                        class="add-btn"
                        data-pid="${p.id}"
                    >

                        <svg>
                            <use
                                href="#ic-plus"
                            ></use>
                        </svg>

                        أضف للسلة

                    </button>

                </div>
            `;


            grid.appendChild(card);
        });
    }


    /* =========================================================
       CART
    ========================================================= */

    function findProduct(id) {
        return products.find(
            p => String(p.id) === String(id)
        );
    }


    function addToCart(pid, size, qty, productMeta) {
        pid = String(pid);
        const p = findProduct(pid) || productMeta;
        qty = Number(qty) || 1;

        if (!size && p && p.sizes && p.sizes.length > 0) {
            size = p.sizes[0];
        }
        size = size || "مقاس واحد";

        const existing =
            cart.find(
                c =>
                    String(c.id) === pid &&
                    c.size === size
            );

        if (existing) {
            existing.qty += qty;
        } else {
            const images = p ? (p.images || p.GetImages || []) : [];
            const firstImg = images.length > 0 ? (typeof images[0] === 'string' ? images[0] : (images[0].imageName || "")) : "";

            cart.push({
                id: pid,
                size: size,
                qty: qty,
                name: p ? (p.name || p.Name || "") : "",
                price: p ? Number(p.price !== undefined ? p.price : (p.Price ?? 0)) : 0,
                image: firstImg,
                cat: p ? (p.cat || p.Cat || "man") : "man",
                icon: p ? (p.icon || p.Icon || "ic-tshirt") : "ic-tshirt"
            });
        }

        saveCart();
        renderCart();

        if (p) {
            showToast(
                `تمت إضافة "${p.name || p.Name || 'المنتج'}" (${size}) للسلة`
            );
        }

        openCart();
    }

    window.addToCart = addToCart;
    window.openCart = openCart;
    window.renderCart = renderCart;


    function changeQty(pid, size, delta) {
        pid = String(pid);
        const item =
            cart.find(
                c =>
                    String(c.id) === pid &&
                    c.size === size
            );

        if (!item)
            return;

        item.qty += delta;

        if (item.qty <= 0) {
            cart =
                cart.filter(
                    c => !(String(c.id) === pid && c.size === size)
                );
        }

        saveCart();
        renderCart();
    }


    function removeItem(pid, size) {
        pid = String(pid);
        cart =
            cart.filter(
                c =>
                    !(
                        String(c.id) === pid &&
                        c.size === size
                    )
            );

        saveCart();
        renderCart();
    }


    function renderCart() {
        const cartItemsWrap =
            document.getElementById(
                "cartItems"
            );

        const cartBadge =
            document.getElementById(
                "cartBadge"
            );

        const cartFoot =
            document.getElementById(
                "cartFoot"
            );

        if (!cartItemsWrap)
            return;

        const totalQty =
            cart.reduce(
                (s, c) => s + c.qty,
                0
            );

        if (cartBadge) {
            cartBadge.textContent =
                totalQty;
        }

        if (cart.length === 0) {
            cartItemsWrap.innerHTML = `
                <div class="cart-empty">
                    <svg>
                        <use
                            href="#ic-empty-cart"
                        ></use>
                    </svg>
                    <p>
                        السلة لسه فاضية…
                        يلا نملاها!
                    </p>
                </div>
            `;

            if (cartFoot) {
                cartFoot.style.display =
                    "none";
            }

            return;
        }

        if (cartFoot) {
            cartFoot.style.display =
                "block";
        }

        cartItemsWrap.innerHTML =
            cart.map(c => {
                const p =
                    findProduct(c.id);

                const name = p ? p.name : (c.name || "منتج");
                const price = p ? p.price : (c.price || 0);
                const cat = p ? p.cat : (c.cat || "man");
                const images = p ? p.images : (c.image ? [c.image] : []);
                const icon = p ? p.icon : (c.icon || "ic-tshirt");

                const firstImage =
                    images &&
                    images.length > 0
                        ? `
                            <img
                                src="/Images/${images[0]}"
                                alt="${name}"
                            >
                        `
                        : `
                            <svg>
                                <use
                                    href="#${icon}"
                                ></use>
                            </svg>
                        `;

                return `
                    <div class="cart-item">
                        <div
                            class="thumb ${catClass[cat] ||
                            "cat-men"
                            }"
                        >
                            ${firstImage}
                        </div>

                        <div class="ci-info">
                            <span class="ci-name">
                                ${name}
                            </span>

                            <span class="ci-meta">
                                المقاس: ${c.size}
                            </span>

                            <div class="ci-bottom">
                                <div class="qty-ctrl">
                                    <button
                                        type="button"
                                        data-act="minus"
                                        data-id="${c.id}"
                                        data-size="${c.size}"
                                    >
                                        −
                                    </button>

                                    <span>
                                        ${c.qty}
                                    </span>

                                    <button
                                        type="button"
                                        data-act="plus"
                                        data-id="${c.id}"
                                        data-size="${c.size}"
                                    >
                                        +
                                    </button>
                                </div>

                                <span class="ci-price">
                                    ${fmt(
                                        price *
                                        c.qty
                                    )}
                                </span>
                            </div>

                            <button
                                type="button"
                                class="remove-btn"
                                data-act="remove"
                                data-id="${c.id}"
                                data-size="${c.size}"
                            >
                                إزالة
                            </button>
                        </div>
                    </div>
                `;
            }).join("");

        const subtotal =
            cart.reduce(
                (s, c) => {
                    const p =
                        findProduct(c.id);
                    const price = p ? p.price : (c.price || 0);

                    return s + (price * c.qty);
                },
                0
            );

        const sumSubtotal =
            document.getElementById(
                "sumSubtotal"
            );

        const sumTotal =
            document.getElementById(
                "sumTotal"
            );

        if (sumSubtotal) {
            sumSubtotal.textContent =
                fmt(subtotal);
        }

        if (sumTotal) {
            sumTotal.textContent =
                fmt(subtotal);
        }
    }


    /* =========================================================
       CART OPEN / CLOSE
    ========================================================= */

    function openCart() {

        const cartDrawer =
            document.getElementById(
                "cartDrawer"
            );

        const overlay =
            document.getElementById(
                "overlay"
            );


        if (cartDrawer) {

            cartDrawer.classList.add(
                "open"
            );
        }


        if (overlay) {

            overlay.classList.add(
                "show"
            );
        }
    }


    function closeCart() {

        const cartDrawer =
            document.getElementById(
                "cartDrawer"
            );

        const overlay =
            document.getElementById(
                "overlay"
            );


        if (cartDrawer) {

            cartDrawer.classList.remove(
                "open"
            );
        }


        if (overlay) {

            overlay.classList.remove(
                "show"
            );
        }
    }


    /* =========================================================
       TOAST
    ========================================================= */

    function showToast(msg) {

        const wrap =
            document.getElementById(
                "toastWrap"
            );


        if (!wrap)
            return;


        const t =
            document.createElement("div");


        t.className = "toast";


        t.innerHTML = `

            <svg>
                <use
                    href="#ic-check"
                ></use>
            </svg>

            <span>
                ${msg}
            </span>
        `;


        wrap.appendChild(t);


        setTimeout(
            () => t.remove(),
            2700
        );
    }


    window.showToast =
        showToast;


    /* =========================================================
       INIT PRODUCTS
    ========================================================= */

    window.initProducts =
        function (data) {

            if (!Array.isArray(data)) {

                console.error(
                    "Products data is not an array:",
                    data
                );

                return;
            }


            products =
                data.map(
                    normalizeProduct
                );


            console.log(
                "Products:",
                products
            );


            renderProducts();
        };


    /* =========================================================
       DOM READY
    ========================================================= */

    document.addEventListener(
        "DOMContentLoaded",
        function () {


            /* =========================
               PRODUCT GRID
            ========================= */

            const grid =
                document.getElementById(
                    "productGrid"
                );


            if (grid) {

                grid.addEventListener(
                    "click",
                    function (e) {

                        /* SIZE */

                        const sizeBtn =
                            e.target.closest(
                                ".size-chip"
                            );


                        if (sizeBtn) {

                            const row =
                                sizeBtn.closest(
                                    ".size-row"
                                );


                            if (!row)
                                return;


                            const pid =
                                String(
                                    row.dataset.pid
                                );


                            selectedSizes[pid] =
                                sizeBtn.dataset.size;


                            row
                                .querySelectorAll(
                                    ".size-chip"
                                )
                                .forEach(
                                    b => {

                                        b.classList.toggle(
                                            "selected",
                                            b === sizeBtn
                                        );
                                    }
                                );


                            return;
                        }


                        /* ADD TO CART */

                        const addBtn =
                            e.target.closest(
                                ".add-btn"
                            );


                        if (addBtn) {

                            const pid =
                                String(
                                    addBtn.dataset.pid
                                );

                            const currentSelectedSize = selectedSizes[pid] || (findProduct(pid)?.sizes?.[0]) || "مقاس واحد";

                            addToCart(
                                pid,
                                currentSelectedSize
                            );
                        }
                    }
                );
            }


            /* =========================
   FILTERS
========================= */

            const filtersWrap =
                document.getElementById("filters");


            if (filtersWrap) {

                filtersWrap.addEventListener(
                    "click",
                    function (e) {

                        const btn =
                            e.target.closest(".chip");

                        if (!btn)
                            return;


                        currentFilter =
                            btn.dataset.filter;


                        // تحديث الـ Active Button
                        [...filtersWrap.children].forEach(c => {

                            c.classList.toggle(
                                "active",
                                c === btn
                            );

                        });


                        renderProducts();
                    }
                );
            }


            /* =========================
               NAV FILTERS
            ========================= */

            document
                .querySelectorAll("[data-navfilter]")
                .forEach(a => {

                    a.addEventListener(
                        "click",
                        function () {

                            const f =
                                a.dataset.navfilter;


                            currentFilter =
                                f;


                            // تحديث الـ Active Chip
                            if (filtersWrap) {

                                [...filtersWrap.children].forEach(c => {

                                    c.classList.toggle(
                                        "active",
                                        c.dataset.filter === f
                                    );

                                });
                            }


                            // قفل الـ Mobile Menu
                            const mainNav =
                                document.getElementById("mainNav");


                            if (mainNav) {

                                mainNav.classList.remove("open");

                            }


                            renderProducts();
                        }
                    );
                });


            /* =========================
               READ FILTER FROM URL
            ========================= */

            const params =
                new URLSearchParams(
                    window.location.search
                );


            const filterFromUrl =
                params.get("filter");


            if (filterFromUrl) {

                // نتأكد إن الـ Filter موجود
                const validFilters = [
                    "all",
                    "man",
                    "women",
                    "accessories",
                    "sale"
                ];


                if (validFilters.includes(filterFromUrl)) {

                    currentFilter =
                        filterFromUrl;


                    // تحديث شكل الـ Active Filter
                    if (filtersWrap) {

                        [...filtersWrap.children].forEach(c => {

                            c.classList.toggle(
                                "active",
                                c.dataset.filter === currentFilter
                            );

                        });
                    }
                }
            }


            /* =========================
               SEARCH
            ========================= */

            const searchInput =
                document.getElementById(
                    "searchInput"
                );


            if (searchInput) {

                searchInput.addEventListener(
                    "input",
                    function (e) {

                        searchTerm =
                            e.target.value
                                .trim()
                                .toLowerCase();


                        renderProducts();
                    }
                );
            }


            /* =========================
               CART ITEMS
            ========================= */

            const cartItemsWrap =
                document.getElementById(
                    "cartItems"
                );


            if (cartItemsWrap) {

                cartItemsWrap.addEventListener(
                    "click",
                    function (e) {

                        const btn =
                            e.target.closest(
                                "button[data-act]"
                            );


                        if (!btn)
                            return;


                        const id =
                            String(
                                btn.dataset.id
                            );


                        const size =
                            btn.dataset.size;


                        if (
                            btn.dataset.act ===
                            "plus"
                        ) {

                            changeQty(
                                id,
                                size,
                                1
                            );
                        }


                        if (
                            btn.dataset.act ===
                            "minus"
                        ) {

                            changeQty(
                                id,
                                size,
                                -1
                            );
                        }


                        if (
                            btn.dataset.act ===
                            "remove"
                        ) {

                            removeItem(
                                id,
                                size
                            );
                        }
                    }
                );
            }


            /* =========================
               CART BUTTON
            ========================= */

            const cartBtn =
                document.getElementById(
                    "cartBtn"
                );


            if (cartBtn) {

                cartBtn.addEventListener(
                    "click",
                    openCart
                );
            }


            /* =========================
               CLOSE CART
            ========================= */

            const closeCartBtn =
                document.getElementById(
                    "closeCart"
                );


            if (closeCartBtn) {

                closeCartBtn.addEventListener(
                    "click",
                    closeCart
                );
            }


            /* =========================
               OVERLAY
            ========================= */

            const overlay =
                document.getElementById(
                    "overlay"
                );


            if (overlay) {

                overlay.addEventListener(
                    "click",
                    function () {

                        closeCart();


                        const modalOverlay =
                            document.getElementById(
                                "modalOverlay"
                            );


                        if (modalOverlay) {

                            modalOverlay.classList.remove(
                                "show"
                            );
                        }


                        const mainNav =
                            document.getElementById(
                                "mainNav"
                            );


                        if (mainNav) {

                            mainNav.classList.remove(
                                "open"
                            );
                        }
                    }
                );
            }


            /* =========================
               CHECKOUT
            ========================= */

            const checkoutBtn =
                document.getElementById(
                    "checkoutBtn"
                );


            if (checkoutBtn) {

                checkoutBtn.addEventListener(
                    "click",
                    function () {

                        if (cart.length === 0) {

                            showToast(
                                "السلة فاضية، ضيف حاجة الأول 🙂"
                            );

                            return;
                        }


                        cart = [];

                        saveCart();

                        renderCart();


                        closeCart();


                        const modalOverlay =
                            document.getElementById(
                                "modalOverlay"
                            );


                        if (modalOverlay) {

                            modalOverlay.classList.add(
                                "show"
                            );
                        }
                    }
                );
            }


            /* =========================
               MODAL CLOSE
            ========================= */

            const modalCloseBtn =
                document.getElementById(
                    "modalCloseBtn"
                );


            if (modalCloseBtn) {

                modalCloseBtn.addEventListener(
                    "click",
                    function () {

                        const modalOverlay =
                            document.getElementById(
                                "modalOverlay"
                            );


                        if (modalOverlay) {

                            modalOverlay.classList.remove(
                                "show"
                            );
                        }
                    }
                );
            }


            /* =========================
               BURGER
            ========================= */

            const burgerBtn =
                document.getElementById(
                    "burgerBtn"
                );


            if (burgerBtn) {

                burgerBtn.addEventListener(
                    "click",
                    function () {

                        const mainNav =
                            document.getElementById(
                                "mainNav"
                            );


                        if (mainNav) {

                            mainNav.classList.toggle(
                                "open"
                            );
                        }
                    }
                );
            }
            /* =========================
               INITIAL RENDER
            ========================= */

            renderProducts();

            renderCart();

        }
    );

})();