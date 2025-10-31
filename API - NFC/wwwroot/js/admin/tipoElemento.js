// ========== GESTIÓN DE TIPOS DE ELEMENTO ==========
document.addEventListener('DOMContentLoaded', function () {
    // ========== CONFIGURACIÓN API ==========
    const API = {
        tipoElementos: '/api/TipoElementoes'
    };

    // ========== ELEMENTOS DEL DOM ==========
    const tablaDatos = document.getElementById('tablaDatos');
    const searchInput = document.getElementById('searchInput');
    const requiereNfcFilter = document.getElementById('requiereNfcFilter');
    const estadoFilter = document.getElementById('estadoFilter');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');

    // Modal
    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));
    const modalTitulo = document.getElementById('modalTitulo');
    const idInput = document.getElementById('idInput');
    const tipoInput = document.getElementById('tipoInput');
    const requiereNfcInput = document.getElementById('requiereNfcInput');
    const estadoInput = document.getElementById('estadoInput');

    // ========== ESTADO ==========
    let paginaActual = 1;
    let pageSize = 10;
    let todosLosTipos = [];

    // ========== INICIALIZACIÓN ==========
    init();

    function init() {
        cargarDatosPaginados(1);
        setupEventListeners();
    }

    // ========== EVENT LISTENERS ==========
    function setupEventListeners() {
        // Búsqueda con debounce
        let timeout;
        searchInput.addEventListener('input', () => {
            clearTimeout(timeout);
            timeout = setTimeout(() => {
                aplicarFiltros();
            }, 300);
        });

        // Filtros
        [requiereNfcFilter, estadoFilter].forEach(filter => {
            filter.addEventListener('change', () => {
                aplicarFiltros();
            });
        });
    }

    // ========== CARGAR DATOS PAGINADOS ==========
    async function cargarDatosPaginados(pagina = 1) {
        try {
            paginaActual = pagina;
            tablaDatos.innerHTML = '<tr><td colspan="6" class="text-center py-4"><div class="spinner-border text-success"></div></td></tr>';

            const response = await fetch(`${API.tipoElementos}/paged?pageNumber=${pagina}&pageSize=${pageSize}`);

            if (!response.ok) {
                throw new Error(`Error ${response.status}: ${response.statusText}`);
            }

            const data = await response.json();

            // Soporta ambos formatos
            todosLosTipos = data.items || data.Items || [];
            const totalPaginas = data.totalPages || data.TotalPages || 1;
            const totalRegistros = data.totalCount || data.TotalCount || 0;

            renderizarTabla(todosLosTipos);
            renderizarPaginacionServidor(pagina, totalPaginas, totalRegistros);

        } catch (error) {
            console.error('❌ Error al cargar:', error);
            tablaDatos.innerHTML = `<tr><td colspan="6" class="text-center text-danger py-4">
                    <i class="bi bi-exclamation-triangle me-2"></i>Error: ${error.message}
                </td></tr>`;
            emptyState.classList.add('d-none');
        }
    }

    // ========== APLICAR FILTROS ==========
    function aplicarFiltros() {
        const search = searchInput.value.toLowerCase().trim();
        const requiereNfcFiltro = requiereNfcFilter.value;
        const estadoFiltro = estadoFilter.value;

        // Si NO hay filtros, usar paginación del servidor
        if (!search && !requiereNfcFiltro && !estadoFiltro) {
            document.getElementById('paginacionContainer').style.display = 'flex';
            cargarDatosPaginados(1);
            return;
        }

        // Si hay filtros, aplicar localmente
        document.getElementById('paginacionContainer').style.display = 'none';

        let filtrados = todosLosTipos.filter(t => {
            const coincideBusqueda = !search || (t.tipo && t.tipo.toLowerCase().includes(search));
            const coincideNfc = !requiereNfcFiltro || t.requiereNFC?.toString() === requiereNfcFiltro;
            const coincideEstado = !estadoFiltro || t.estado?.toString() === estadoFiltro;

            return coincideBusqueda && coincideNfc && coincideEstado;
        });

        renderizarTabla(filtrados);
    }

    // ========== RENDERIZAR TABLA ==========
    function renderizarTabla(tipos) {
        resultadosContador.textContent = tipos.length;

        if (tipos.length === 0) {
            tablaDatos.innerHTML = '';
            emptyState.classList.remove('d-none');
            return;
        }

        emptyState.classList.add('d-none');

        tablaDatos.innerHTML = tipos.map(t => {
            const nfcBadge = t.requiereNFC
                ? '<span class="badge bg-success"><i class="bi bi-wifi"></i> Sí</span>'
                : '<span class="badge bg-secondary"><i class="bi bi-wifi-off"></i> No</span>';

            const fecha = t.fechaCreacion ? new Date(t.fechaCreacion).toLocaleDateString() : 'N/A';

            return `
                    <tr>
                        <td class="fw-semibold">${t.idTipoElemento}</td>
                        <td><span class="badge bg-info text-dark" style="font-size: 0.9rem;">${t.tipo || 'N/A'}</span></td>
                        <td>${nfcBadge}</td>
                        <td><small class="text-muted">${fecha}</small></td>
                        <td><span class="badge ${t.estado ? 'bg-success' : 'bg-danger'}">${t.estado ? 'Activo' : 'Inactivo'}</span></td>
                        <td class="text-center">
                            <button class="btn btn-sm btn-warning" onclick="editarTipoGlobal(${t.idTipoElemento})">
                                <i class="bi bi-pencil-fill"></i>
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="eliminarTipoGlobal(${t.idTipoElemento})">
                                <i class="bi bi-trash-fill"></i>
                            </button>
                        </td>
                    </tr>
                `;
        }).join('');
    }

    // ========== PAGINACIÓN DEL SERVIDOR ==========
    function renderizarPaginacionServidor(paginaActual, totalPaginas, totalRegistros) {
        const paginacionNav = document.getElementById('paginacionNav');
        const infoPaginacion = document.getElementById('infoPaginacion');
        const paginacionContainer = document.getElementById('paginacionContainer');

        paginacionContainer.style.display = 'flex';

        const inicio = ((paginaActual - 1) * pageSize) + 1;
        const fin = Math.min(paginaActual * pageSize, totalRegistros);

        infoPaginacion.innerHTML = `<small class="text-muted">Mostrando ${inicio}-${fin} de ${totalRegistros} registros</small>`;

        if (totalPaginas <= 1) {
            paginacionNav.innerHTML = '';
            return;
        }

        let html = '<ul class="pagination pagination-sm mb-0">';

        html += `<li class="page-item ${paginaActual === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="cambiarPaginaServidor(${paginaActual - 1}); return false;">«</a>
            </li>`;

        for (let i = 1; i <= totalPaginas; i++) {
            if (i === 1 || i === totalPaginas || (i >= paginaActual - 2 && i <= paginaActual + 2)) {
                html += `<li class="page-item ${i === paginaActual ? 'active' : ''}">
                        <a class="page-link" href="#" onclick="cambiarPaginaServidor(${i}); return false;">${i}</a>
                    </li>`;
            } else if (i === paginaActual - 3 || i === paginaActual + 3) {
                html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        html += `<li class="page-item ${paginaActual === totalPaginas ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="cambiarPaginaServidor(${paginaActual + 1}); return false;">»</a>
            </li>`;

        html += '</ul>';
        paginacionNav.innerHTML = html;
    }

    window.cambiarPaginaServidor = function (pagina) {
        if (pagina < 1) return;
        cargarDatosPaginados(pagina);
    };

    // ========== ABRIR MODAL CREAR ==========
    window.abrirModalCrear = function () {
        modalTitulo.textContent = 'Nuevo Tipo de Elemento';
        document.getElementById('formularioEditor').reset();
        idInput.value = '';
        estadoInput.checked = true;
        requiereNfcInput.checked = false;
        editorModal.show();
    };

    // ========== EDITAR ==========
    window.editarTipoGlobal = async function (id) {
        try {
            const response = await fetch(`${API.tipoElementos}/${id}`);
            if (!response.ok) throw new Error('Error al cargar tipo de elemento');

            const data = await response.json();

            modalTitulo.textContent = 'Editar Tipo de Elemento';
            idInput.value = data.idTipoElemento;
            tipoInput.value = data.tipo || '';
            requiereNfcInput.checked = data.requiereNFC ?? false;
            estadoInput.checked = data.estado ?? true;

            editorModal.show();

        } catch (error) {
            alert('Error: ' + error.message);
        }
    };

    // ========== GUARDAR ==========
    window.guardarTipo = async function () {
        try {
            const id = parseInt(idInput.value) || 0;

            if (!tipoInput.value.trim()) {
                alert('El campo "Tipo" es obligatorio');
                return;
            }

            const data = {
                idTipoElemento: id,
                tipo: tipoInput.value.trim(),
                requiereNFC: requiereNfcInput.checked,
                estado: estadoInput.checked
            };

            console.log('✅ Enviando:', data);

            const url = id === 0 ? API.tipoElementos : `${API.tipoElementos}/${id}`;
            const method = id === 0 ? 'POST' : 'PUT';

            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('❌ Error del servidor:', errorText);
                throw new Error(errorText || 'Error al guardar');
            }

            editorModal.hide();
            await cargarDatosPaginados(paginaActual);
            alert(id === 0 ? '✅ Tipo creado exitosamente' : '✅ Tipo actualizado exitosamente');

        } catch (error) {
            alert('❌ Error: ' + error.message);
            console.error('Error completo:', error);
        }
    };

    // ========== ELIMINAR ==========
    window.eliminarTipoGlobal = async function (id) {
        if (!confirm('¿Eliminar este tipo de elemento?')) return;

        try {
            const response = await fetch(`${API.tipoElementos}/${id}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al eliminar');

            await cargarDatosPaginados(paginaActual);
            alert('✅ Tipo eliminado exitosamente');

        } catch (error) {
            alert('❌ Error: ' + error.message);
        }
    };

    // ========== LIMPIAR FILTROS ==========
    window.limpiarFiltros = function () {
        searchInput.value = '';
        requiereNfcFilter.value = '';
        estadoFilter.value = '';
        paginaActual = 1;
        cargarDatosPaginados(1);
    };
});