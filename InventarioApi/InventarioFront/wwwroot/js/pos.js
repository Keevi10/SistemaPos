const appEl = document.getElementById('app');
const API_URL = appEl.dataset.apiUrl;

let allProducts = [];
let cart = [];
let currentView = 'ventas';
let pendingDelete = null;
let pendingBarcode = '';
let foundProduct = null;

document.getElementById('current-date').textContent =
    new Date().toLocaleDateString('es-MX', { weekday: 'long', day: 'numeric', month: 'short' });

async function apiGet(endpoint) {
    const response = await fetch(`${API_URL}${endpoint}`);
    if (!response.ok) {
        const error = await response.json().catch(() => ({}));
        throw new Error(error.mensaje || 'Error en la solicitud');
    }
    return response.json();
}

async function apiPost(endpoint, body) {
    const response = await fetch(`${API_URL}${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.mensaje || 'Error en la solicitud');
    return data;
}

async function apiDelete(endpoint) {
    const response = await fetch(`${API_URL}${endpoint}`, { method: 'DELETE' });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.mensaje || 'Error al eliminar');
    return data;
}

async function apiPut(endpoint, body) {
    const response = await fetch(`${API_URL}${endpoint}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    const data = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(data.mensaje || 'Error en la solicitud');
    return data;
}

async function loadProducts() {
    try {
        allProducts = await apiGet('Productos');
        renderProducts();
        renderInventory();
    } catch (err) {
        showToast(err.message, true);
    }
}

function switchView(view) {
    if (view === 'inventario') {
        document.getElementById('password-modal').classList.remove('hidden');
        document.getElementById('inp-clave-inventario').value = '';
        document.getElementById('clave-error').classList.add('hidden');
        document.getElementById('inp-clave-inventario').focus();
        return;
    }

    currentView = view;
    ['ventas', 'inventario', 'dashboard'].forEach(v => {
        document.getElementById(`view-${v}`).classList.toggle('hidden', v !== view);
        document.getElementById(`nav-${v}`).classList.toggle('active', v === view);
    });
    const el = document.getElementById(`view-${view}`);
    el.classList.remove('fade-in');
    void el.offsetWidth;
    el.classList.add('fade-in');

    if (view === 'dashboard') renderDashboard();
}

async function confirmClaveInventario() {
    const clave = document.getElementById('inp-clave-inventario').value.trim();
    if (!clave) return;

    const btn = document.querySelector('#password-modal .btn-primary');
    btn.disabled = true;
    btn.textContent = 'Validando...';

    try {
        const response = await fetch('/Pos/ValidarClaveInventario', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ clave })
        });
        const data = await response.json();
        if (!data.valido) throw new Error(data.mensaje);

        document.getElementById('password-modal').classList.add('hidden');
        btn.disabled = false;
        btn.innerHTML = 'Acceder';

        currentView = 'inventario';
        ['ventas', 'inventario', 'dashboard'].forEach(v => {
            document.getElementById(`view-${v}`).classList.toggle('hidden', v !== 'inventario');
            document.getElementById(`nav-${v}`).classList.toggle('active', v === 'inventario');
        });
        const el = document.getElementById('view-inventario');
        el.classList.remove('fade-in');
        void el.offsetWidth;
        el.classList.add('fade-in');
    } catch (err) {
        document.getElementById('clave-error').classList.remove('hidden');
        document.getElementById('clave-error').textContent = err.message || 'Clave incorrecta';
        document.getElementById('inp-clave-inventario').value = '';
        document.getElementById('inp-clave-inventario').focus();
    } finally {
        btn.disabled = false;
        btn.innerHTML = 'Acceder';
    }
}

function cancelarClaveInventario() {
    document.getElementById('password-modal').classList.add('hidden');
    document.getElementById('clave-error').classList.add('hidden');
}

function filterProducts() {
    renderProducts();
}

function getCategoryEmoji(cat) {
    const map = { Bebidas: '🥤', Alimentos: '🍔', 'Electrónica': '📱', General: '📦', Otro: '🏷️' };
    return map[cat] || '📦';
}

function renderProducts() {
    const search = (document.getElementById('search-products').value || '').toLowerCase();
    const grid = document.getElementById('products-grid');
    const filtered = allProducts.filter(p =>
        p.cantidad > 0 &&
        (p.nombre.toLowerCase().includes(search) ||
         (p.codigoBarras && p.codigoBarras.includes(search)))
    );

    grid.innerHTML = filtered.map(p => `
        <div class="glass-card p-3 cursor-pointer hover:bg-white/10 transition-all duration-300 hover:scale-[1.02] active:scale-[0.98]"
             onclick="addToCart(${p.id})">
            <div class="text-2xl mb-2">${getCategoryEmoji(p.categoria)}</div>
            <p class="text-sm font-medium truncate">${p.nombre}</p>
            <p class="text-red-300 font-bold text-sm">$${Number(p.precio).toFixed(2)}</p>
            <p class="text-[10px] text-white/30 mt-1">Stock: ${p.cantidad}</p>
        </div>
    `).join('');
}

function addToCart(id) {
    const product = allProducts.find(p => p.id === id);
    if (!product) return;

    const inCart = cart.find(c => c.id === id);
    if (inCart) {
        if (inCart.qty >= product.cantidad) {
            showToast('Sin stock suficiente', true);
            return;
        }
        inCart.qty++;
    } else {
        cart.push({ id, name: product.nombre, price: product.precio, qty: 1 });
    }
    renderCart();
}

function removeFromCart(id) {
    cart = cart.filter(c => c.id !== id);
    renderCart();
}

function renderCart() {
    const container = document.getElementById('cart-items');
    let total = 0;

    container.innerHTML = cart.map(item => {
        total += item.price * item.qty;
        return `
            <div class="cart-item glass-card p-2 flex items-center justify-between">
                <div class="flex-1 min-w-0">
                    <p class="text-xs font-medium truncate">${item.name}</p>
                    <p class="text-[10px] text-white/40">${item.qty} × $${Number(item.price).toFixed(2)}</p>
                </div>
                <div class="flex items-center gap-1">
                    <span class="text-xs font-bold text-red-300">$${(item.price * item.qty).toFixed(2)}</span>
                    <button type="button" onclick="removeFromCart(${item.id})" class="text-red-400/60 hover:text-red-400 ml-1">
                        <i data-lucide="x" style="width:12px;height:12px;"></i>
                    </button>
                </div>
            </div>
        `;
    }).join('');

    document.getElementById('cart-total').textContent = `$${total.toFixed(2)}`;
    lucide.createIcons();
}

async function completeSale() {
    if (cart.length === 0) {
        showToast('El carrito está vacío', true);
        return;
    }

    const btn = document.getElementById('btn-complete-sale');
    btn.disabled = true;
    btn.innerHTML = '<span class="animate-pulse">Procesando...</span>';

    try {
        await apiPost('Ventas', {
            items: cart.map(c => ({ productoId: c.id, cantidad: c.qty }))
        });
        cart = [];
        renderCart();
        await loadProducts();
        showToast('¡Venta completada!');
    } catch (err) {
        showToast(err.message, true);
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i data-lucide="check-circle" style="width:16px;height:16px;"></i> Completar Venta';
        lucide.createIcons();
    }
}

function renderInventory() {
    const tbody = document.getElementById('inventory-table');
    tbody.innerHTML = allProducts.map(p => `
        <tr class="border-b border-white/5 hover:bg-white/5 transition">
            <td class="py-2 px-3">${getCategoryEmoji(p.categoria)} ${p.nombre}</td>
            <td class="py-2 px-3 text-white/50">${p.categoria}</td>
            <td class="py-2 px-3 text-red-300 font-medium">$${Number(p.precio).toFixed(2)}</td>
            <td class="py-2 px-3 ${p.cantidad < 5 ? 'text-red-400' : 'text-white/70'}">${p.cantidad}</td>
            <td class="py-2 px-3 text-emerald-300">${p.vendidos || 0}</td>
            <td class="py-2 px-3">
                <button type="button" onclick="deleteProduct(${p.id})" class="text-red-400/50 hover:text-red-400 transition">
                    <i data-lucide="trash-2" style="width:14px;height:14px;"></i>
                </button>
            </td>
        </tr>
    `).join('');
    lucide.createIcons();
}

function deleteProduct(id) {
    pendingDelete = id;
    const overlay = document.createElement('div');
    overlay.className = 'confirm-overlay';
    overlay.id = 'confirm-dialog';
    overlay.innerHTML = `
        <div class="glass p-6 max-w-sm w-full mx-4 text-center">
            <p class="text-lg font-semibold mb-2">¿Eliminar producto?</p>
            <p class="text-sm text-white/50 mb-5">Esta acción no se puede deshacer</p>
            <div class="flex gap-3 justify-center">
                <button type="button" onclick="cancelDelete()" class="px-5 py-2 rounded-xl glass-card text-sm font-medium">Cancelar</button>
                <button type="button" onclick="confirmDelete()" class="btn-danger px-5 py-2 rounded-xl text-sm font-medium">Eliminar</button>
            </div>
        </div>
    `;
    document.body.appendChild(overlay);
}

function cancelDelete() {
    pendingDelete = null;
    document.getElementById('confirm-dialog')?.remove();
}

async function confirmDelete() {
    document.getElementById('confirm-dialog')?.remove();
    try {
        await apiDelete(`Productos/${pendingDelete}`);
        showToast('Producto eliminado');
        await loadProducts();
    } catch (err) {
        showToast(err.message, true);
    }
    pendingDelete = null;
}

document.getElementById('inp-barcode').addEventListener('keydown', async (e) => {
    if (e.key !== 'Enter') return;
    e.preventDefault();
    const barcode = document.getElementById('inp-barcode').value.trim();
    if (!barcode) return;

    try {
        const product = await apiGet(`Productos/codigo/${encodeURIComponent(barcode)}`);
        foundProduct = product;
        document.getElementById('barcode-product-name').textContent = product.nombre;
        document.getElementById('barcode-current-stock').textContent = product.cantidad;
        document.getElementById('barcode-product-emoji').textContent = getCategoryEmoji(product.categoria);
        document.getElementById('inp-add-stock').value = 1;
        document.getElementById('barcode-exists-info').classList.remove('hidden');
        document.getElementById('add-product-form').classList.add('hidden');
        document.getElementById('inp-barcode').value = '';
        document.getElementById('inp-add-stock').focus();
        showToast(`Producto encontrado: ${product.nombre}`);
    } catch {
        document.getElementById('barcode-exists-info').classList.add('hidden');
        document.getElementById('add-product-form').classList.remove('hidden');
        foundProduct = null;
        pendingBarcode = barcode;
        document.getElementById('inp-name').value = `Producto ${barcode}`;
        document.getElementById('inp-name').focus();
        showToast(`Código registrado: ${barcode}`);
    }
});

async function confirmAddStock() {
    if (!foundProduct) return;
    const cantidad = parseInt(document.getElementById('inp-add-stock').value);
    if (!cantidad || cantidad <= 0) {
        showToast('Ingresa una cantidad válida', true);
        return;
    }

    const btn = document.querySelector('#barcode-exists-info button');
    btn.disabled = true;
    btn.textContent = 'Guardando...';

    try {
        await apiPut(`Productos/codigo/${encodeURIComponent(foundProduct.codigoBarras)}/stock`, { cantidad });
        showToast(`Se agregaron ${cantidad} unidades a ${foundProduct.nombre}`);
        cancelBarcodeFound();
        await loadProducts();
    } catch (err) {
        showToast(err.message, true);
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i data-lucide="package-plus" style="width:14px;height:14px;"></i> Agregar Stock';
        lucide.createIcons();
    }
}

function cancelBarcodeFound() {
    foundProduct = null;
    document.getElementById('barcode-exists-info').classList.add('hidden');
    document.getElementById('add-product-form').classList.remove('hidden');
    document.getElementById('inp-barcode').focus();
}

document.getElementById('search-products').addEventListener('keydown', (e) => {
    if (e.key !== 'Enter') return;
    e.preventDefault();
    const search = document.getElementById('search-products').value.trim();
    if (search.length < 3) return;

    const product = allProducts.find(p =>
        (p.codigoBarras === search || p.nombre.toLowerCase().includes(search.toLowerCase())) &&
        p.cantidad > 0
    );

    if (product) {
        addToCart(product.id);
        document.getElementById('search-products').value = '';
        showToast(`${product.nombre} añadido al carrito`);
    } else {
        showToast('Producto no encontrado', true);
    }
});

document.getElementById('add-product-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const name = document.getElementById('inp-name').value.trim();
    const price = parseFloat(document.getElementById('inp-price').value);
    const quantity = parseInt(document.getElementById('inp-qty').value);
    const category = document.getElementById('inp-category').value;
    const barcode = document.getElementById('inp-barcode').value.trim();

    if (!name || isNaN(price) || isNaN(quantity)) {
        showToast('Completa todos los campos', true);
        return;
    }

    const btn = e.target.querySelector('button[type="submit"]');
    btn.disabled = true;
    btn.textContent = 'Guardando...';

    try {
        await apiPost('Productos', {
            nombre: name,
            precio: price,
            cantidad: quantity,
            categoria: category,
            codigoBarras: barcode || null
        });
        showToast(`${name} agregado`);
        e.target.reset();
        document.getElementById('inp-barcode').value = '';
        pendingBarcode = '';
        document.getElementById('inp-barcode').focus();
        await loadProducts();
    } catch (err) {
        showToast(err.message, true);
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i data-lucide="plus" style="width:16px;height:16px;"></i> Agregar';
        lucide.createIcons();
    }
});

async function renderDashboard() {
    try {
        const [dashboard, ventasPorDia, topHoy, topTotal] = await Promise.all([
            apiGet('Ventas/dashboard'),
            apiGet('Ventas/por-dia?dias=7'),
            apiGet('Ventas/top-productos-hoy?limite=10'),
            apiGet('Ventas/top-productos?limite=5')
        ]);

        document.getElementById('stat-products').textContent = dashboard.totalProductos;
        document.getElementById('stat-sales').textContent = `$${Number(dashboard.ventasTotales).toFixed(0)}`;
        document.getElementById('stat-items').textContent = dashboard.itemsVendidos;

        renderSalesByDayChart(ventasPorDia);
        renderDailyTopProducts(topHoy);
        renderTopProducts(topTotal);
    } catch (err) {
        showToast(err.message, true);
    }
}

function renderSalesByDayChart(salesData) {
    const maxAmount = Math.max(...salesData.map(d => d.monto), 1);
    const chart = document.getElementById('sales-chart');
    const noMsg = document.getElementById('no-chart-msg');
    const hasSales = salesData.some(d => d.monto > 0);

    if (!hasSales) {
        chart.innerHTML = '';
        noMsg.classList.remove('hidden');
        return;
    }

    noMsg.classList.add('hidden');
    chart.innerHTML = salesData.map(d => {
        const heightPercent = (d.monto / maxAmount) * 100 || 5;
        return `
            <div class="flex flex-col items-center gap-1 flex-1">
                <div class="w-full bg-gradient-to-t from-red-500 to-red-600 rounded-t-lg opacity-80 hover:opacity-100 transition-all cursor-pointer group relative"
                     style="height: ${heightPercent}%; min-height: 8px;"
                     title="$${Number(d.monto).toFixed(0)}">
                    <div class="absolute -top-7 left-1/2 -translate-x-1/2 bg-white/10 backdrop-filter backdrop-blur px-2 py-1 rounded text-[10px] font-semibold opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">
                        $${Number(d.monto).toFixed(0)}
                    </div>
                </div>
                <span class="text-[10px] text-white/40 text-center">${d.etiquetaDia}</span>
            </div>
        `;
    }).join('');
}

function renderDailyTopProducts(topToday) {
    const tbody = document.getElementById('daily-top-products');
    const noMsg = document.getElementById('no-daily-msg');
    const medals = ['🥇', '🥈', '🥉', '4️⃣', '5️⃣', '6️⃣', '7️⃣', '8️⃣', '9️⃣', '🔟'];

    if (topToday.length === 0) {
        tbody.innerHTML = '';
        noMsg.classList.remove('hidden');
        return;
    }

    noMsg.classList.add('hidden');
    tbody.innerHTML = topToday.map((p, i) => `
        <tr class="border-b border-white/5 hover:bg-white/5 transition">
            <td class="py-2 px-3 text-white/70">${medals[i] || i + 1}</td>
            <td class="py-2 px-3">${getCategoryEmoji(p.categoria)} ${p.nombre}</td>
            <td class="py-2 px-3 text-red-300 font-semibold">${p.cantidadVendida}</td>
            <td class="py-2 px-3 text-emerald-300 font-semibold">$${Number(p.ingresos).toFixed(2)}</td>
        </tr>
    `).join('');
}

function renderTopProducts(top) {
    const container = document.getElementById('top-products');
    const noMsg = document.getElementById('no-sales-msg');

    if (top.length === 0) {
        container.innerHTML = '';
        noMsg.classList.remove('hidden');
        return;
    }

    noMsg.classList.add('hidden');
    const maxSold = top[0].cantidadVendida;
    const medals = ['🥇', '🥈', '🥉', '4️⃣', '5️⃣'];

    container.innerHTML = top.map((p, i) => `
        <div class="flex items-center gap-3 fade-in stagger-${i + 1}">
            <span class="text-lg w-8 text-center">${medals[i]}</span>
            <div class="flex-1">
                <div class="flex justify-between items-center mb-1">
                    <span class="text-sm font-medium">${p.nombre}</span>
                    <span class="text-xs text-white/50">${p.cantidadVendida} vendidos</span>
                </div>
                <div class="h-1.5 rounded-full bg-white/10 overflow-hidden">
                    <div class="progress-bar h-full rounded-full bg-gradient-to-r from-red-500 to-red-600"
                         style="width:${(p.cantidadVendida / maxSold) * 100}%"></div>
                </div>
            </div>
        </div>
    `).join('');
}

function showToast(msg, error = false) {
    const t = document.createElement('div');
    t.className = `toast ${error ? 'error' : ''}`;
    t.textContent = msg;
    document.body.appendChild(t);
    setTimeout(() => t.remove(), 3000);
}

const mouseLight = document.getElementById('mouse-light');
let mouseInside = false;

document.addEventListener('mouseenter', () => {
    mouseInside = true;
    mouseLight.classList.add('active');
});
document.addEventListener('mouseleave', () => {
    mouseInside = false;
    mouseLight.classList.remove('active');
});
document.addEventListener('mousemove', (e) => {
    if (mouseInside) {
        mouseLight.style.left = e.clientX + 'px';
        mouseLight.style.top = e.clientY + 'px';
    }
});

lucide.createIcons();
loadProducts();
