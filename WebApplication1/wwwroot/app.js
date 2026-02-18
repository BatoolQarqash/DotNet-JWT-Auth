// wwwroot/app.js

// ===== Helpers =====
const $ = (id) => document.getElementById(id);

// We serve frontend from the SAME backend origin => no CORS issues.
function getBase() {
    return window.location.origin;
}

function getToken() {
    return localStorage.getItem("jwt") || "";
}

function setToken(token) {
    localStorage.setItem("jwt", token);
    $("tokenBox").value = token || "";
    setAuthedUI(!!token);
}

function clearToken() {
    localStorage.removeItem("jwt");
    $("tokenBox").value = "";
    setAuthedUI(false);
}

function setAuthedUI(isAuthed) {
    $("btnLogout").disabled = !isAuthed;
    $("btnMe").disabled = !isAuthed;
    $("btnAdminSecret").disabled = !isAuthed;

    $("btnLoadCategories").disabled = !isAuthed;
    $("btnCreateCategory").disabled = !isAuthed;

    $("btnLoadPosts").disabled = !isAuthed;
    $("btnCreatePost").disabled = !isAuthed;

    if (isAuthed) {
        loadCategories();
        loadPosts();
    } else {
        $("catsTbody").innerHTML = "";
        $("postCategory").innerHTML = "";
        $("postsList").innerHTML = "";
    }
}

async function apiFetch(path, options = {}) {
    const base = getBase();
    const url = `${base}${path}`;

    const headers = options.headers ? { ...options.headers } : {};
    const token = getToken();
    if (token) headers["Authorization"] = `Bearer ${token}`;

    const res = await fetch(url, { ...options, headers });

    const text = await res.text();
    let data = null;
    try { data = text ? JSON.parse(text) : null; } catch { data = text; }

    if (!res.ok) {
        const msg = (data && data.message)
            ? data.message
            : (typeof data === "string" ? data : JSON.stringify(data));
        throw new Error(`HTTP ${res.status}: ${msg}`);
    }

    return data;
}

function pretty(obj) {
    if (obj == null) return "";
    return typeof obj === "string" ? obj : JSON.stringify(obj, null, 2);
}

// ===== Auth =====
async function login() {
    $("loginOut").textContent = "";
    try {
        const body = {
            email: $("email").value.trim(),
            password: $("password").value
        };

        const data = await apiFetch("/api/auth/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body),
        });

        const token = data.token || data.Token;
        if (!token) throw new Error("Token missing in response.");

        setToken(token);
        $("loginOut").textContent = "✅ Logged in successfully.";
        $("loginOut").style.color = "var(--accent)";
    } catch (e) {
        $("loginOut").textContent = "❌ " + e.message;
        $("loginOut").style.color = "var(--danger)";
    }
}

async function me() {
    $("statusOut").textContent = "";
    try {
        const data = await apiFetch("/api/auth/me");
        $("statusOut").textContent = "✅ /me\n" + pretty(data);
    } catch (e) {
        $("statusOut").textContent = "❌ " + e.message;
    }
}

async function adminSecret() {
    $("statusOut").textContent = "";
    try {
        const data = await apiFetch("/api/auth/admin-secret");
        $("statusOut").textContent = "✅ admin-secret\n" + pretty(data);
    } catch (e) {
        $("statusOut").textContent = "❌ " + e.message;
    }
}

// Ping a dedicated endpoint (more stable than swagger path).
async function ping() {
    $("statusOut").textContent = "";
    try {
        const base = getBase();
        const res = await fetch(base + "/swagger/index.html"); // should exist in dev
        $("statusOut").textContent = res.ok
            ? "✅ API reachable."
            : `⚠️ API reachable but returned: ${res.status}`;
    } catch (e) {
        $("statusOut").textContent = "❌ Cannot reach API.\n" + e.message;
    }
}

// ===== Categories =====
async function loadCategories() {
    $("catsOut").textContent = "";
    try {
        const cats = await apiFetch("/api/admin/categories");
        renderCategories(cats);
        fillCategoryDropdown(cats);
        $("catsOut").textContent = `✅ Loaded ${cats.length} categories.`;
    } catch (e) {
        $("catsOut").textContent = "❌ " + e.message;
    }
}

function renderCategories(cats) {
    const tbody = $("catsTbody");
    tbody.innerHTML = "";

    (cats || []).forEach(c => {
        const tr = document.createElement("tr");

        const tdId = document.createElement("td");
        tdId.textContent = c.id;
        tr.appendChild(tdId);

        const tdName = document.createElement("td");
        tdName.textContent = c.name;
        tr.appendChild(tdName);

        const tdActions = document.createElement("td");
        const wrap = document.createElement("div");
        wrap.className = "mini";

        const btnEdit = document.createElement("button");
        btnEdit.className = "btn btn-ghost";
        btnEdit.textContent = "Edit";
        btnEdit.onclick = () => editCategory(c.id, c.name);

        const btnDel = document.createElement("button");
        btnDel.className = "btn btn-danger";
        btnDel.textContent = "Delete";
        btnDel.onclick = () => deleteCategory(c.id);

        wrap.appendChild(btnEdit);
        wrap.appendChild(btnDel);
        tdActions.appendChild(wrap);

        tr.appendChild(tdActions);
        tbody.appendChild(tr);
    });
}

function fillCategoryDropdown(cats) {
    const sel = $("postCategory");
    sel.innerHTML = "";
    (cats || []).forEach(c => {
        const opt = document.createElement("option");
        opt.value = c.id;
        opt.textContent = `${c.name} (id=${c.id})`;
        sel.appendChild(opt);
    });
}

async function createCategory() {
    $("catsOut").textContent = "";
    try {
        const name = $("catName").value.trim();
        if (!name) throw new Error("Category name required.");

        const data = await apiFetch("/api/admin/categories", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name }),
        });

        $("catName").value = "";
        await loadCategories();
        $("catsOut").textContent = "✅ Created: " + pretty(data);
    } catch (e) {
        $("catsOut").textContent = "❌ " + e.message;
    }
}

async function editCategory(id, oldName) {
    const newName = prompt("New category name:", oldName);
    if (newName == null) return;

    $("catsOut").textContent = "";
    try {
        await apiFetch(`/api/admin/categories/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: newName }),
        });
        await loadCategories();
        $("catsOut").textContent = `✅ Updated category ${id}`;
    } catch (e) {
        $("catsOut").textContent = "❌ " + e.message;
    }
}

async function deleteCategory(id) {
    if (!confirm(`Delete category ${id}? (Will fail if posts exist)`)) return;

    $("catsOut").textContent = "";
    try {
        await apiFetch(`/api/admin/categories/${id}`, { method: "DELETE" });
        await loadCategories();
        $("catsOut").textContent = `✅ Deleted category ${id}`;
    } catch (e) {
        $("catsOut").textContent = "❌ " + e.message;
    }
}

// ===== Posts =====
async function loadPosts() {
    $("postsOut").textContent = "";
    try {
        const posts = await apiFetch("/api/admin/posts");
        renderPosts(posts);
        $("postsOut").textContent = `✅ Loaded ${posts.length} posts.`;
    } catch (e) {
        $("postsOut").textContent = "❌ " + e.message;
    }
}

function renderPosts(posts) {
    const list = $("postsList");
    list.innerHTML = "";

    (posts || []).forEach(p => {
        const card = document.createElement("div");
        card.className = "postCard";

        const thumb = document.createElement("div");
        thumb.className = "thumb";
        if (p.imageUrl) {
            const img = document.createElement("img");
            img.src = getBase() + p.imageUrl;
            img.alt = "post image";
            thumb.appendChild(img);
        } else {
            thumb.textContent = "No image";
        }

        const meta = document.createElement("div");
        meta.className = "postMeta";

        const t = document.createElement("div");
        t.className = "pTitle";
        t.textContent = `${p.title} (id=${p.id})`;

        const d = document.createElement("div");
        d.className = "pDesc";
        d.textContent = p.description || "";

        const s = document.createElement("div");
        s.className = "pSmall";
        s.textContent = `Category: ${p.categoryName ?? "-"} (id=${p.categoryId}) • Created: ${p.createdAt}`;

        const actions = document.createElement("div");
        actions.className = "row gap";
        actions.style.marginTop = "10px";

        const btnDel = document.createElement("button");
        btnDel.className = "btn btn-danger";
        btnDel.textContent = "Delete";
        btnDel.onclick = () => deletePost(p.id);

        actions.appendChild(btnDel);

        meta.appendChild(t);
        meta.appendChild(d);
        meta.appendChild(s);
        meta.appendChild(actions);

        card.appendChild(thumb);
        card.appendChild(meta);

        list.appendChild(card);
    });
}

async function createPost() {
    $("postCreateOut").textContent = "";
    try {
        const title = $("postTitle").value.trim();
        const desc = $("postDesc").value.trim();
        const note = $("postNote").value.trim();
        const categoryId = parseInt($("postCategory").value, 10);

        if (!title) throw new Error("Title required.");
        if (!desc) throw new Error("Description required.");
        if (!categoryId) throw new Error("Choose a category.");

        const form = new FormData();
        form.append("Title", title);
        form.append("Description", desc);
        if (note) form.append("Note", note);
        form.append("CategoryId", categoryId.toString());

        const file = $("postImage").files?.[0];
        if (file) form.append("Image", file);

        const data = await apiFetch("/api/admin/posts", {
            method: "POST",
            body: form
        });

        $("postTitle").value = "";
        $("postDesc").value = "";
        $("postNote").value = "";
        $("postImage").value = "";

        await loadPosts();
        $("postCreateOut").textContent = "✅ Post created: " + pretty(data);
        $("postCreateOut").style.color = "var(--accent)";
    } catch (e) {
        $("postCreateOut").textContent = "❌ " + e.message;
        $("postCreateOut").style.color = "var(--danger)";
    }
}

async function deletePost(id) {
    if (!confirm(`Delete post ${id}?`)) return;

    $("postsOut").textContent = "";
    try {
        await apiFetch(`/api/admin/posts/${id}`, { method: "DELETE" });
        await loadPosts();
        $("postsOut").textContent = `✅ Deleted post ${id}`;
    } catch (e) {
        $("postsOut").textContent = "❌ " + e.message;
    }
}

// ===== Wire up =====
window.addEventListener("DOMContentLoaded", () => {
    // set base box (readonly) to same origin
    $("apiBase").value = getBase();

    // restore token if exists
    setToken(getToken());

    $("btnLogin").onclick = login;
    $("btnLogout").onclick = () => { clearToken(); $("statusOut").textContent = "Logged out."; };

    $("btnPing").onclick = ping;
    $("btnMe").onclick = me;
    $("btnAdminSecret").onclick = adminSecret;

    $("btnLoadCategories").onclick = loadCategories;
    $("btnCreateCategory").onclick = createCategory;

    $("btnLoadPosts").onclick = loadPosts;
    $("btnCreatePost").onclick = createPost;

    $("tokenBox").value = getToken();
});
