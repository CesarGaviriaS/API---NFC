// usuarios.js - dinámico y robusto para nuevos campos de DB
document.addEventListener('DOMContentLoaded', function () {
    // ---------- CONFIG (adapta las rutas si tu API es singular/plural) ----------
    const api = (window.AppRoutes && window.AppRoutes.api) ? window.AppRoutes.api : {
        usuarios: '/api/usuarios',
        aprendiz: '/api/aprendices',
        funcionario: '/api/funcionario',
        fichas: '/api/ficha'
    };
    const usuariosApiBase = api.usuarios || '/api/usuarios';
    const fichasApiUrl = api.fichas || '/api/ficha';
    const aprendizApiUrl = api.aprendiz || '/api/aprendices';
    const funcionarioApiUrl = api.funcionario || '/api/funcionario';

    // ---------- ELEMENTOS DEL DOM ----------
    const tablaDatos = document.getElementById('tablaDatos');
    const searchInput = document.getElementById('searchInput');
    const rolFilter = document.getElementById('rolFilter');
    const fichaFilter = document.getElementById('fichaFilter');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');

    // modal unificado init tolerante
    const userModalEl = document.getElementById('userModal');
    const userModal = userModalEl ? (typeof bootstrap !== 'undefined' ? new bootstrap.Modal(userModalEl) : null) : null;

    const userModalTitle = document.getElementById('userModalTitle');
    const userIdInput = document.getElementById('userIdInput');
    const userRoleSelect = document.getElementById('userRoleSelect');
    const userNombreInput = document.getElementById('userNombreInput');
    const userDocumentoInput = document.getElementById('userDocumentoInput');

    const aprendizFields = document.getElementById('aprendizFields');
    const aprendizFichaSelect = document.getElementById('aprendizFichaSelect');
    const funcionarioFields = document.getElementById('funcionarioFields');
    const funcionarioCargoInput = document.getElementById('funcionarioCargoInput');
    const funcionarioPasswordInput = document.getElementById('funcionarioPasswordInput');

    const userSaveBtn = document.getElementById('userSaveBtn');

    // ---------- ESTADO ----------
    let paginaActual = 1;
    let registrosPorPagina = 10;
    let searchTermActual = '';
    let listaUsuariosCompleta = [];
    let listaUsuariosPagina = [];
    let listaFichas = [];

    // ---------- HELPERS ----------
    function get(obj, ...paths) {
        for (const p of paths) {
            if (!p) continue;
            const parts = p.split('.');
            let cur = obj;
            let ok = true;
            for (const part of parts) {
                if (cur == null) { ok = false; break; }
                if (part in cur) { cur = cur[part]; continue; }
                const lower = part.charAt(0).toLowerCase() + part.slice(1);
                if (lower in cur) { cur = cur[lower]; continue; }
                const upper = part.charAt(0).toUpperCase() + part.slice(1);
                if (upper in cur) { cur = cur[upper]; continue; }
                ok = false; break;
            }
            if (ok && cur !== undefined) return cur;
        }
        return undefined;
    }

    function normalizeUser(raw) {
        const user = {};
        user.idUsuario = get(raw, 'idUsuario', 'IdUsuario', 'id', 'Id');
        user.role = get(raw, 'rol', 'Rol', 'role', 'Role') || (raw.aprendiz ? 'Aprendiz' : (raw.funcionario ? 'Funcionario' : undefined));

        const rawApr = get(raw, 'aprendiz', 'Aprendiz');
        if (rawApr) {
            user.aprendiz = {
                idAprendiz: get(rawApr, 'idAprendiz', 'IdAprendiz', 'id', 'Id'),
                nombre: get(rawApr, 'nombre', 'Nombre'),
                documento: get(rawApr, 'documento', 'NumeroDocumento', 'numeroDocumento'),
                idFicha: get(rawApr, 'idFicha', 'IdFicha')
            };
            user.nombre = user.aprendiz.nombre;
            user.documento = user.aprendiz.documento;
            user.detalle = get(rawApr, 'ficha.codigo', 'ficha.Codigo') || null;
            return user;
        }

        const rawFunc = get(raw, 'funcionario', 'Funcionario');
        if (rawFunc) {
            user.funcionario = {
                idFuncionario: get(rawFunc, 'idFuncionario', 'IdFuncionario', 'id', 'Id'),
                nombre: get(rawFunc, 'nombre', 'Nombre'),
                documento: get(rawFunc, 'documento', 'NumeroDocumento', 'numeroDocumento'),
                detalle: get(rawFunc, 'detalle', 'Detalle', 'cargo', 'Cargo')
            };
            user.nombre = user.funcionario.nombre;
            user.documento = user.funcionario.documento;
            user.detalle = user.funcionario.detalle;
            return user;
        }

        user.nombre = get(raw, 'nombre', 'Nombre', 'fullName', 'FullName') || `${get(raw, 'nombre', 'Nombre') || ''} ${get(raw, 'apellido', 'Apellido') || ''}`.trim();
        user.documento = get(raw, 'numeroDocumento', 'NumeroDocumento', 'documento', 'Documento') || '';
        user.detalle = get(raw, 'cargo', 'Cargo', 'detalle', 'Detalle') || (get(raw, 'ficha.codigo', 'ficha.Codigo') || null);
        user.raw = raw;
        return user;
    }

    // ---------- PAGINADO ----------
    async function cargarDatosPaginated(pagina = 1, searchTerm = '') {
        paginaActual = pagina;
        searchTermActual = searchTerm;
        const params = new URLSearchParams({ page: pagina, pageSize: registrosPorPagina });
        if (searchTerm) params.append('search', searchTerm);

        try {
            let url;
            if (window.AppRoutes && window.AppRoutes.paginatedUrl && window.AppRoutes.api && window.AppRoutes.api.usuarios) {
                url = window.AppRoutes.paginatedUrl(window.AppRoutes.api.usuarios, pagina, registrosPorPagina, searchTerm);
            } else {
                url = `${usuariosApiBase}/paginated?${params.toString()}`;
            }

            const res = await fetch(url);
            if (!res.ok) throw new Error('Error al obtener paginado');
            const raw = await res.json();
            const arr = raw.data || raw || [];
            listaUsuariosPagina = (arr || []).map(u => normalizeUser(u));
            listaUsuariosCompleta = listaUsuariosPagina.slice();
            renderizarTabla(listaUsuariosPagina);

            const pageVal = get(raw, 'page', 'Page') || pagina;
            const pageSizeVal = get(raw, 'pageSize', 'PageSize') || registrosPorPagina;
            const totalPages = get(raw, 'totalPages', 'TotalPages') || Math.ceil((get(raw, 'totalRecords', 'TotalRecords') || listaUsuariosPagina.length) / pageSizeVal);
            const totalRecords = get(raw, 'totalRecords', 'TotalRecords') || listaUsuariosPagina.length;
            generarPaginacion(pageVal, totalPages, totalRecords);
        } catch (err) {
            console.error('paginado falla, fallback:', err);
            await cargarDatosCompletos();
        }
    }

    async function cargarDatosCompletos() {
        try {
            const res = await fetch(usuariosApiBase);
            if (!res.ok) throw new Error('Error al cargar lista');
            const raw = await res.json();
            const arr = raw.data || raw || [];
            listaUsuariosCompleta = (arr || []).map(u => normalizeUser(u));
            aplicarFiltrosClientes();
        } catch (err) {
            console.error(err);
            tablaDatos.innerHTML = '';
            resultadosContador.textContent = 0;
            emptyState.classList.remove('d-none');
        }
    }

    // ---------- FILTRADO ----------
    function aplicarFiltros() {
        const searchTerm = (searchInput.value || '').toLowerCase().trim();
        const rolSeleccionado = (rolFilter.value || '').trim();
        const fichaSeleccionada = (fichaFilter.value || '').trim();

        if (rolSeleccionado || fichaSeleccionada) {
            if (!listaUsuariosCompleta || listaUsuariosCompleta.length === 0) { cargarDatosCompletos(); return; }
            aplicarFiltrosClientes();
        } else {
            cargarDatosPaginated(1, searchTerm);
        }
    }

    function aplicarFiltrosClientes() {
        const searchTerm = (searchInput.value || '').toLowerCase().trim();
        const rolSeleccionado = (rolFilter.value || '').trim();
        const fichaSeleccionada = (fichaFilter.value || '').trim();

        const filtered = (listaUsuariosCompleta || []).filter(user => {
            const nombre = (user.nombre || '').toLowerCase();
            const documento = (user.documento || '') + '';
            const rol = (user.role || '') + '';
            const fichaId = (user.aprendiz && user.aprendiz.idFicha) || get(user.raw || {}, 'idFicha', 'IdFicha');

            const matchSearch = !searchTerm || nombre.includes(searchTerm) || (documento && documento.includes(searchTerm));
            const matchRol = !rolSeleccionado || rol === rolSeleccionado;
            const matchFicha = !fichaSeleccionada || (fichaId && fichaId.toString() === fichaSeleccionada);
            return matchSearch && matchRol && matchFicha;
        });

        renderizarTabla(filtered);
        const pagContainer = document.getElementById('paginacionContainer');
        if (pagContainer) pagContainer.style.display = 'none';
    }

    // ---------- RENDER TABLA ----------
    function renderizarTabla(users) {
        tablaDatos.innerHTML = '';
        resultadosContador.textContent = users.length || 0;
        if (!users || users.length === 0) { emptyState.classList.remove('d-none'); return; }
        emptyState.classList.add('d-none');

        users.forEach(user => {
            const rol = user.role || (user.aprendiz ? 'Aprendiz' : (user.funcionario ? 'Funcionario' : 'SinRol'));
            const nombre = user.nombre || '';
            const documento = user.documento || '';
            const detalle = user.detalle || '';
            const editId = (user.aprendiz && user.aprendiz.idAprendiz) || (user.funcionario && user.funcionario.idFuncionario) || user.idUsuario || 0;
            const badgeClass = rol === 'Aprendiz' ? 'bg-primary' : 'bg-warning text-dark';
            const rolBadge = rol === 'Aprendiz'
                ? `<span class="badge ${badgeClass} badge-role"><i class="bi bi-mortarboard-fill me-1"></i>${rol}</span>`
                : `<span class="badge ${badgeClass} badge-role"><i class="bi bi-person-badge-fill me-1"></i>${rol}</span>`;
            const infoAdicional = rol === 'Aprendiz'
                ? `<span class="badge bg-success bg-opacity-10 text-success">${detalle || 'Sin ficha'}</span>`
                : `<span class="text-muted">${detalle || 'N/A'}</span>`;

            tablaDatos.innerHTML += `
                <tr>
                    <td class="fw-semibold">${user.idUsuario || ''}</td>
                    <td>${rolBadge}</td>
                    <td class="fw-semibold">${nombre}</td>
                    <td><span class="badge bg-light text-dark">${documento}</span></td>
                    <td>${infoAdicional}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-primary" onclick="abrirModal('${(rol || '').toLowerCase()}', ${editId})"><i class="bi bi-pencil-fill"></i></button>
                        <button class="btn btn-sm btn-danger" onclick="desactivar(${user.idUsuario || 0})"><i class="bi bi-trash-fill"></i></button>
                    </td>
                </tr>`;
        });
    }

    // ---------- PAGINACIÓN ----------
    function generarPaginacion(page, totalPages, totalRecords) {
        let paginacionContainer = document.getElementById('paginacionContainer');
        if (!paginacionContainer) {
            const tablaParent = document.querySelector('.table-responsive').parentNode;
            const nuevoContenedor = document.createElement('div');
            nuevoContenedor.className = 'd-flex justify-content-between align-items-center mt-3';
            nuevoContenedor.id = 'paginacionContainer';
            nuevoContenedor.innerHTML = `<div id="infoPaginacion"></div><nav id="paginacionNav"></nav>`;
            tablaParent.appendChild(nuevoContenedor);
            paginacionContainer = nuevoContenedor;
        }
        const infoPaginacion = document.getElementById('infoPaginacion');
        const paginacionNav = document.getElementById('paginacionNav');

        const inicio = ((paginaActual - 1) * registrosPorPagina) + 1;
        const fin = Math.min(paginaActual * registrosPorPagina, totalRecords || 0);
        infoPaginacion.innerHTML = `<small class="text-muted">Mostrando ${isNaN(inicio) ? 0 : inicio}-${isNaN(fin) ? 0 : fin} de ${totalRecords || 0} registros</small>`;

        let pagHTML = '<ul class="pagination pagination-sm mb-0">';
        pagHTML += `<li class="page-item ${paginaActual === 1 ? 'disabled' : ''}"><a class="page-link" href="#" onclick="cambiarPagina(${paginaActual - 1}); return false;">«</a></li>`;
        const paginasAMostrar = 5;
        let inicioPag = Math.max(1, paginaActual - Math.floor(paginasAMostrar / 2));
        let finPag = Math.min(totalPages, inicioPag + paginasAMostrar - 1);
        if (finPag - inicioPag + 1 < paginasAMostrar) inicioPag = Math.max(1, finPag - paginasAMostrar + 1);

        for (let i = inicioPag; i <= finPag; i++) {
            pagHTML += `<li class="page-item ${i === paginaActual ? 'active' : ''}"><a class="page-link" href="#" onclick="cambiarPagina(${i}); return false;">${i}</a></li>`;
        }
        pagHTML += `<li class="page-item ${paginaActual === totalPages ? 'disabled' : ''}"><a class="page-link" href="#" onclick="cambiarPagina(${paginaActual + 1}); return false;">»</a></li>`;
        pagHTML += '</ul>';
        paginacionNav.innerHTML = pagHTML;
    }

    window.cambiarPagina = (p) => cargarDatosPaginated(p, searchTermActual);

    // ---------- CARGAR FICHAS ----------
    async function cargarFichas() {
        try {
            const res = await fetch(fichasApiUrl);
            if (!res.ok) throw new Error('Error cargando fichas');
            const raw = await res.json();
            const arr = raw.data || raw || [];
            listaFichas = arr;
            aprendizFichaSelect.innerHTML = '<option value="">Seleccione una ficha...</option>';
            fichaFilter.innerHTML = '<option value="">Todas las fichas</option>';
            arr.forEach(f => {
                const id = get(f, 'idFicha', 'IdFicha', 'id', 'Id');
                const codigo = get(f, 'codigo', 'Codigo') || '';
                const prog = get(f, 'programa.nombrePrograma', 'programa.NombrePrograma') || '';
                aprendizFichaSelect.innerHTML += `<option value="${id}">${codigo} - ${prog}</option>`;
                fichaFilter.innerHTML += `<option value="${id}">${codigo}</option>`;
            });
        } catch (err) { console.error(err); }
    }

    // ---------- MODAL: mostrar/ocultar campos ----------
    function mostrarCamposPorRol(rol) {
        if (!rol) { aprendizFields.classList.add('d-none'); funcionarioFields.classList.add('d-none'); return; }
        if (rol === 'Aprendiz') { aprendizFields.classList.remove('d-none'); funcionarioFields.classList.add('d-none'); }
        else if (rol === 'Funcionario') { funcionarioFields.classList.remove('d-none'); aprendizFields.classList.add('d-none'); }
        else { aprendizFields.classList.add('d-none'); funcionarioFields.classList.add('d-none'); }
    }

    // ---------- GLOBALS: abrirModal, guardar, desactivar ----------
    window.abrirModal = function (tipo, id = 0) {
        try {
            userModalTitle.textContent = id === 0 ? 'Crear Usuario' : 'Editar Usuario';
            userIdInput.value = id || 0;
            // reset
            userRoleSelect.value = (tipo === 'aprendiz') ? 'Aprendiz' : (tipo === 'funcionario') ? 'Funcionario' : '';
            userNombreInput.value = '';
            userDocumentoInput.value = '';
            funcionarioCargoInput.value = '';
            funcionarioPasswordInput.value = '';
            aprendizFichaSelect.value = '';
            mostrarCamposPorRol(userRoleSelect.value);

            if (id !== 0) {
                const u = (listaUsuariosCompleta || []).find(x => (x.aprendiz && get(x, 'aprendiz.idAprendiz') == id) || (x.funcionario && get(x, 'funcionario.idFuncionario') == id) || x.idUsuario == id)
                    || (listaUsuariosPagina || []).find(x => x.idUsuario == id);
                if (u) {
                    userRoleSelect.value = u.role || userRoleSelect.value;
                    mostrarCamposPorRol(userRoleSelect.value);
                    userNombreInput.value = u.nombre || '';
                    userDocumentoInput.value = u.documento || '';
                    if (u.aprendiz) aprendizFichaSelect.value = u.aprendiz.idFicha || '';
                    if (u.funcionario) funcionarioCargoInput.value = u.funcionario.detalle || '';
                }
            }
            if (userModal) userModal.show();
            else console.warn('userModal no inicializado, no se puede mostrar dialog');
        } catch (err) {
            console.error('abrirModal error', err);
        }
    };

    userRoleSelect && userRoleSelect.addEventListener('change', (e) => mostrarCamposPorRol(e.target.value));

    userSaveBtn && userSaveBtn.addEventListener('click', async () => {
        const id = parseInt(userIdInput.value || 0);
        const role = userRoleSelect.value;
        const nombre = (userNombreInput.value || '').trim();
        const documento = (userDocumentoInput.value || '').trim();
        if (!role || !nombre || !documento) { alert('Complete los campos obligatorios.'); return; }

        let url, method = id === 0 ? 'POST' : 'PUT', payload = null;
        if (role === 'Aprendiz') {
            const idFicha = aprendizFichaSelect.value || null;
            url = id === 0 ? aprendizApiUrl : `${aprendizApiUrl}/${id}`;
            payload = { idAprendiz: id || 0, nombre, documento, idFicha: idFicha ? parseInt(idFicha) : null, estado: true };
        } else {
            const cargo = funcionarioCargoInput.value.trim();
            const password = funcionarioPasswordInput.value.trim();
            url = id === 0 ? funcionarioApiUrl : `${funcionarioApiUrl}/${id}`;
            payload = { idFuncionario: id || 0, nombre, documento, detalle: cargo, estado: true };
            if (password) payload.contrasena = password;
        }

        try {
            const res = await fetch(url, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            if (!res.ok) { const txt = await res.text(); throw new Error(txt || `Error ${res.status}`); }
            if (userModal) userModal.hide();
            await cargarDatosPaginated(paginaActual, searchTermActual);
            alert(id === 0 ? 'Creado correctamente' : 'Actualizado correctamente');
        } catch (err) {
            console.error('Error guardar usuario:', err);
            alert('Error al guardar: ' + (err.message || err));
        }
    });

    window.desactivar = async function (idUsuario) {
        if (!confirm(`¿Desea desactivar al usuario ID ${idUsuario}?`)) return;
        try {
            const res = await fetch(`${usuariosApiBase}/${idUsuario}`, { method: 'DELETE' });
            if (!res.ok) throw new Error('Error al desactivar');
            await cargarDatosPaginated(paginaActual, searchTermActual);
        } catch (err) { console.error(err); alert('Error: ' + (err.message || err)); }
    };

    // ---------- Listeners filtros ----------
    searchInput && searchInput.addEventListener('input', aplicarFiltros);
    rolFilter && rolFilter.addEventListener('change', aplicarFiltros);
    fichaFilter && fichaFilter.addEventListener('change', aplicarFiltros);

    // ---------- Init ----------
    cargarFichas();
    cargarDatosPaginated(1, '');
});