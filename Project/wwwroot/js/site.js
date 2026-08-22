(function () {
    "use strict";

    /* ---------------- DATA ---------------- */
    const products = [
        { id: 1, name: "تيشيرت أبيض أوفرسايز", cat: "men", price: 450, old: null, icon: "ic-tshirt", sizes: ["S", "M", "L", "XL"] },
        { id: 2, name: "جاكيت دنيم كلاسيك", cat: "men", price: 950, old: null, icon: "ic-jacket", sizes: ["M", "L", "XL"] },
        { id: 3, name: "هودي بيچ مريح", cat: "men", price: 650, old: 820, icon: "ic-hoodie", sizes: ["S", "M", "L", "XL"] },
        { id: 4, name: "بنطلون تيلر أسود", cat: "men", price: 700, old: null, icon: "ic-trousers", sizes: ["30", "32", "34", "36"] },
        { id: 5, name: "قميص كتان فاتح", cat: "men", price: 550, old: null, icon: "ic-shirt", sizes: ["S", "M", "L", "XL"] },
        { id: 6, name: "فستان صيفي مطبع بالورد", cat: "women", price: 800, old: 1000, icon: "ic-dress", sizes: ["S", "M", "L"] },
        { id: 7, name: "معطف صوف كاميل", cat: "women", price: 1800, old: null, icon: "ic-coat", sizes: ["S", "M", "L"] },
        { id: 8, name: "سويتر تريكو حريمي", cat: "women", price: 600, old: null, icon: "ic-sweater", sizes: ["S", "M", "L", "XL"] },
        { id: 9, name: "شنطة كروس جلد", cat: "accessories", price: 700, old: null, icon: "ic-bag", sizes: ["مقاس واحد"] },
        { id: 10, name: "قبعة كاجوال", cat: "accessories", price: 250, old: 340, icon: "ic-cap", sizes: ["مقاس واحد"] },
    ];
    const catLabel = { men: "رجالي", women: "حريمي", accessories: "إكسسوار" };
    const catClass = { men: "cat-men", women: "cat-women", accessories: "cat-accessories" };

    let currentFilter = "all";
    let searchTerm = "";
    let selectedSizes = {}; // productId -> size
    let cart = []; // {id, size, qty}

    const fmt = n => n.toLocaleString('ar-EG') + " ج.م";

    /* ---------------- RENDER PRODUCTS ---------------- */
    const grid = document.getElementById('productGrid');

    function matchesFilter(p) {
        if (currentFilter === "all") return true;
        if (currentFilter === "sale") return !!p.old;
        return p.cat === currentFilter;
    }
    function matchesSearch(p) {
        if (!searchTerm) return true;
        return p.name.toLowerCase().includes(searchTerm) || catLabel[p.cat].includes(searchTerm);
    }

    function renderProducts() {
        const list = products.filter(p => matchesFilter(p) && matchesSearch(p));
        grid.innerHTML = "";
        if (list.length === 0) {
            grid.innerHTML = '<div class="empty-state">مفيش منتجات مطابقة للبحث حالياً — جرّب كلمة تانية.</div>';
            return;
        }
        list.forEach(p => {
            if (!selectedSizes[p.id]) selectedSizes[p.id] = p.sizes[0];
            const card = document.createElement('div');
            card.className = "card";
            card.innerHTML = `
        <div class="card-media ${catClass[p.cat]}">
          ${p.old ? `<div class="tag-fold"><span>خصم</span></div>` : ""}
          <svg><use href="#${p.icon}"/></svg>
        </div>
        <div class="card-body">
          <span class="card-cat">${catLabel[p.cat]}</span>
          <h3 class="card-name">${p.name}</h3>
          <div class="price-row">
            <span class="price">${fmt(p.price)}</span>
            ${p.old ? `<span class="price-old">${fmt(p.old)}</span>` : ""}
          </div>
          <div class="size-row" data-pid="${p.id}">
            ${p.sizes.map(s => `<button type="button" class="size-chip ${s === selectedSizes[p.id] ? 'selected' : ''}" data-size="${s}">${s}</button>`).join("")}
          </div>
          <button class="add-btn" data-pid="${p.id}">
            <svg><use href="#ic-plus"/></svg> أضف للسلة
          </button>
        </div>
      `;
            grid.appendChild(card);
        });
    }

    grid.addEventListener('click', (e) => {
        const sizeBtn = e.target.closest('.size-chip');
        if (sizeBtn) {
            const row = sizeBtn.closest('.size-row');
            const pid = Number(row.dataset.pid);
            selectedSizes[pid] = sizeBtn.dataset.size;
            row.querySelectorAll('.size-chip').forEach(b => b.classList.toggle('selected', b === sizeBtn));
            return;
        }
        const addBtn = e.target.closest('.add-btn');
        if (addBtn) {
            const pid = Number(addBtn.dataset.pid);
            addToCart(pid, selectedSizes[pid]);
        }
    });

    /* ---------------- FILTERS + SEARCH ---------------- */
    const filtersWrap = document.getElementById('filters');
    filtersWrap.addEventListener('click', (e) => {
        const btn = e.target.closest('.chip');
        if (!btn) return;
        currentFilter = btn.dataset.filter;
        [...filtersWrap.children].forEach(c => c.classList.toggle('active', c === btn));
        renderProducts();
    });

    document.querySelectorAll('[data-navfilter]').forEach(a => {
        a.addEventListener('click', () => {
            const f = a.dataset.navfilter;
            currentFilter = f;
            [...filtersWrap.children].forEach(c => c.classList.toggle('active', c.dataset.filter === f));
            document.getElementById('mainNav').classList.remove('open');
            renderProducts();
        });
    });

    document.getElementById('searchInput').addEventListener('input', (e) => {
        searchTerm = e.target.value.trim().toLowerCase();
        renderProducts();
    });

    /* ---------------- CART LOGIC ---------------- */
    const cartDrawer = document.getElementById('cartDrawer');
    const overlay = document.getElementById('overlay');
    const cartItemsWrap = document.getElementById('cartItems');
    const cartBadge = document.getElementById('cartBadge');
    const cartFoot = document.getElementById('cartFoot');

    function findProduct(id) { return products.find(p => p.id === id); }

    function addToCart(pid, size) {
        const existing = cart.find(c => c.id === pid && c.size === size);
        if (existing) { existing.qty++; }
        else { cart.push({ id: pid, size: size, qty: 1 }); }
        renderCart();
        const p = findProduct(pid);
        showToast(`تمت إضافة "${p.name}" (${size}) للسلة`);
        openCart();
    }

    function changeQty(pid, size, delta) {
        const item = cart.find(c => c.id === pid && c.size === size);
        if (!item) return;
        item.qty += delta;
        if (item.qty <= 0) { cart = cart.filter(c => c !== item); }
        renderCart();
    }

    function removeItem(pid, size) {
        cart = cart.filter(c => !(c.id === pid && c.size === size));
        renderCart();
    }

    function renderCart() {
        const totalQty = cart.reduce((s, c) => s + c.qty, 0);
        cartBadge.textContent = totalQty;

        if (cart.length === 0) {
            cartItemsWrap.innerHTML = `
        <div class="cart-empty">
          <svg><use href="#ic-empty-cart"/></svg>
          <p>السلة لسه فاضية… يلا نملاها!</p>
        </div>`;
            cartFoot.style.display = "none";
            return;
        }
        cartFoot.style.display = "block";

        cartItemsWrap.innerHTML = cart.map(c => {
            const p = findProduct(c.id);
            return `
        <div class="cart-item">
          <div class="thumb ${catClass[p.cat]}"><svg><use href="#${p.icon}"/></svg></div>
          <div class="ci-info">
            <span class="ci-name">${p.name}</span>
            <span class="ci-meta">المقاس: ${c.size}</span>
            <div class="ci-bottom">
              <div class="qty-ctrl">
                <button type="button" data-act="minus" data-id="${c.id}" data-size="${c.size}" aria-label="تقليل الكمية">−</button>
                <span>${c.qty}</span>
                <button type="button" data-act="plus" data-id="${c.id}" data-size="${c.size}" aria-label="زيادة الكمية">+</button>
              </div>
              <span class="ci-price">${fmt(p.price * c.qty)}</span>
            </div>
            <button type="button" class="remove-btn" data-act="remove" data-id="${c.id}" data-size="${c.size}">إزالة</button>
          </div>
        </div>`;
        }).join("");

        const subtotal = cart.reduce((s, c) => s + findProduct(c.id).price * c.qty, 0);
        document.getElementById('sumSubtotal').textContent = fmt(subtotal);
        document.getElementById('sumTotal').textContent = fmt(subtotal);
    }

    cartItemsWrap.addEventListener('click', (e) => {
        const btn = e.target.closest('button[data-act]');
        if (!btn) return;
        const id = Number(btn.dataset.id);
        const size = btn.dataset.size;
        if (btn.dataset.act === "plus") changeQty(id, size, 1);
        if (btn.dataset.act === "minus") changeQty(id, size, -1);
        if (btn.dataset.act === "remove") removeItem(id, size);
    });

    function openCart() {
        cartDrawer.classList.add('open');
        overlay.classList.add('show');
    }
    function closeCart() {
        cartDrawer.classList.remove('open');
        overlay.classList.remove('show');
    }
    document.getElementById('cartBtn').addEventListener('click', openCart);
    document.getElementById('closeCart').addEventListener('click', closeCart);
    overlay.addEventListener('click', () => { closeCart(); closeModal(); document.getElementById('mainNav').classList.remove('open'); });

    /* ---------------- CHECKOUT MODAL ---------------- */
    const modalOverlay = document.getElementById('modalOverlay');
    function openModal() { modalOverlay.classList.add('show'); }
    function closeModal() { modalOverlay.classList.remove('show'); }
    document.getElementById('checkoutBtn').addEventListener('click', () => {
        if (cart.length === 0) { showToast("السلة فاضية، ضيف حاجة الأول 🙂"); return; }
        cart = [];
        renderCart();
        closeCart();
        openModal();
    });
    document.getElementById('modalCloseBtn').addEventListener('click', closeModal);

    /* ---------------- MOBILE NAV ---------------- */
    document.getElementById('burgerBtn').addEventListener('click', () => {
        document.getElementById('mainNav').classList.toggle('open');
    });

    /* ---------------- TOAST ---------------- */
    function showToast(msg) {
        const wrap = document.getElementById('toastWrap');
        const t = document.createElement('div');
        t.className = "toast";
        t.innerHTML = `<svg><use href="#ic-check"/></svg><span>${msg}</span>`;
        wrap.appendChild(t);
        setTimeout(() => t.remove(), 2700);
    }
    window.showToast = showToast;

    /* ---------------- INIT ---------------- */
    renderProducts();
    renderCart();
})();
