
document.addEventListener('DOMContentLoaded', function () {
    const fichasApiUrl = '/api/ficha';
    const programasApiUrl = '/api/programa';
    const fichasPaginatedApiUrl = '/api/ficha/paginated';
    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));

    // Referencias a los inputs del formulario
    const idInput = document.getElementById('idInput');
    const codigoInput = document.getElementById('codigoInput');
    const programaIdInput = document.getElementById('programaIdInput');
    const fechaInicioInput = document.getElementById('fechaInicioInput');
    const fechaFinalInput = document.getElementById('fechaFinalInput');

    // Referencias a los filtros
    const searchInput = document.getElementById('searchInput');
    const programaFilter = document.getElementById('programaFilter');
    const estadoFilter = document.getElementById('estadoFilter');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');

    const modalTitulo = document.getElementById('modalTitulo');
    const tablaDatos = document.getElementById('tablaDatos');

    let paginaActual = 1;
    let totalPaginas = 1;
    let totalRegistros = 0;
    let pageSize = 10;

    // --- CARGAR PROGRAMAS ---
    const cargarProgramas = async () => {
        try {
            const response = await fetch(programasApiUrl);
            const programas = await response.json();

            programaIdInput.innerHTML = '<option value="">Seleccione un programa...</option>';
            programas.forEach(prog => {
                programaIdInput.innerHTML += `<option value="${prog.idPrograma}">${prog.nombrePrograma}</option>`;
            });

            programaFilter.innerHTML = '<option value="">Todos los programas</option>';
            programas.forEach(prog => {
                programaFilter.innerHTML += `<option value="${prog.idPrograma}">${prog.nombrePrograma}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar programas:", error);
        }
    };

    // --- CARGAR DATOS CON PAGINACIÓN DEL SERVIDOR ---
    const cargarDatos = async (pagina = 1) => {
        try {
            const params = new URLSearchParams({
                page: pagina,
                pageSize: pageSize,
                search: searchInput.value || ''
            });

            const response = await fetch(`${fichasPaginatedApiUrl}?${params}`);
            if (!response.ok) throw new Error('Error al cargar los datos de la ficha.');

            const data = await response.json();

            // Actualizar variables de estado
            paginaActual = data.page;
            totalPaginas = data.totalPages;
            totalRegistros = data.totalRecords;

            // Aplicar filtros locales sobre los datos paginados
            aplicarFiltrosLocales(data.data);
            actualizarPaginacion();
        } catch (error) {
            console.error(error);
            mostrarErrorEnTabla(error.message);
        }
    };

    // --- APLICAR FILTROS LOCALES (Programa y Estado) ---
    function aplicarFiltrosLocales(fichas) {
        const programaSeleccionado = programaFilter.value;
        const estadoSeleccionado = estadoFilter.value;

        let fichasFiltradas = fichas.filter(ficha => {
            const coincidePrograma = !programaSeleccionado || ficha.idPrograma.toString() === programaSeleccionado;
            const coincideEstado = filtrarPorEstado(ficha, estadoSeleccionado);
            return coincidePrograma && coincideEstado;
        });

        mostrarFichas(fichasFiltradas);
    }

    // --- EJECUTAR BÚSQUEDA (recarga desde servidor) ---
    window.aplicarFiltros = function () {
        paginaActual = 1; // Reiniciar a la primera página al filtrar
        cargarDatos(1);
    };

    // --- FILTRAR POR ESTADO ---
    function filtrarPorEstado(ficha, estado) {
        if (!estado) return true;
        const fechaInicio = ficha.fechaInicio ? new Date(ficha.fechaInicio) : null;
        const fechaFinal = ficha.fechaFinal ? new Date(ficha.fechaFinal) : null;
        const hoy = new Date();

        switch (estado) {
            case 'activa':
                return fechaInicio && fechaInicio <= hoy && (!fechaFinal || fechaFinal >= hoy);
            case 'finalizada':
                return fechaFinal && fechaFinal < hoy;
            case 'proxima':
                return fechaInicio && fechaInicio > hoy;
            default:
                return true;
        }
    }

    // --- MOSTRAR FICHAS EN LA TABLA ---
    function mostrarFichas(fichas) {
        tablaDatos.innerHTML = '';
        resultadosContador.textContent = `${fichas.length} de ${totalRegistros}`;

        if (fichas.length === 0) {
            emptyState.classList.remove('d-none');
            return;
        }

        emptyState.classList.add('d-none');

        fichas.forEach(item => {
            const nombrePrograma = item.programa ? item.programa.nombrePrograma : '<em class="text-muted">N/A</em>';
            const fechaInicio = item.fechaInicio ? new Date(item.fechaInicio).toLocaleDateString() : 'N/A';
            const fechaFinal = item.fechaFinal ? new Date(item.fechaFinal).toLocaleDateString() : 'N/A';
            const estado = determinarEstado(item);

            tablaDatos.innerHTML += `
                <tr>
                    <td>${item.idFicha}</td>
                    <td>${item.codigo}</td>
                    <td>${nombrePrograma}</td>
                    <td>${fechaInicio}</td>
                    <td>${fechaFinal}</td>
                    <td><span class="badge ${estado.clase}">${estado.texto}</span></td>
                    <td>
                        <button class="btn btn-sm btn-warning" onclick="abrirModal(${item.idFicha}, '${item.codigo}', '${item.idPrograma}', '${item.fechaInicio || ''}', '${item.fechaFinal || ''}')">
                            <i class="bi bi-pencil-fill"></i> Editar
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idFicha})">
                            <i class="bi bi-trash-fill"></i> Borrar
                        </button>
                    </td>
                </tr>`;
        });
    }

    // --- ACTUALIZAR CONTROLES DE PAGINACIÓN ---
    function actualizarPaginacion() {
        const paginacion = document.getElementById('paginacion');
        if (!paginacion) return;

        paginacion.innerHTML = '';

        // Botón Anterior
        const anteriorDisabled = paginaActual <= 1 ? 'disabled' : '';
        paginacion.innerHTML += `
            <li class="page-item ${anteriorDisabled}">
                <button class="page-link" onclick="cambiarPagina(${paginaActual - 1})" ${anteriorDisabled}>Anterior</button>
            </li>`;

        // Números de página
        const inicio = Math.max(1, paginaActual - 2);
        const fin = Math.min(totalPaginas, paginaActual + 2);

        for (let i = inicio; i <= fin; i++) {
            const activa = i === paginaActual ? 'active' : '';
            paginacion.innerHTML += `
                <li class="page-item ${activa}">
                    <button class="page-link" onclick="cambiarPagina(${i})">${i}</button>
                </li>`;
        }

        // Botón Siguiente
        const siguienteDisabled = paginaActual >= totalPaginas ? 'disabled' : '';
        paginacion.innerHTML += `
            <li class="page-item ${siguienteDisabled}">
                <button class="page-link" onclick="cambiarPagina(${paginaActual + 1})" ${siguienteDisabled}>Siguiente</button>
            </li>`;

        document.getElementById('infoPagina').textContent = `Página ${paginaActual} de ${totalPaginas}`;
    }

    // --- CAMBIAR DE PÁGINA ---
    window.cambiarPagina = async (pagina) => {
        if (pagina < 1 || pagina > totalPaginas) return;
        await cargarDatos(pagina);
    };

    // --- DETERMINAR ESTADO DE LA FICHA ---
    function determinarEstado(ficha) {
        const fechaInicio = ficha.fechaInicio ? new Date(ficha.fechaInicio) : null;
        const fechaFinal = ficha.fechaFinal ? new Date(ficha.fechaFinal) : null;
        const hoy = new Date();

        if (!fechaInicio && !fechaFinal) return { texto: 'Sin fecha', clase: 'bg-secondary' };
        if (fechaInicio && fechaInicio > hoy) return { texto: 'Próxima', clase: 'bg-info' };
        if (fechaFinal && fechaFinal < hoy) return { texto: 'Finalizada', clase: 'bg-danger' };
        if (fechaInicio && fechaInicio <= hoy && (!fechaFinal || fechaFinal >= hoy)) return { texto: 'En curso', clase: 'bg-success' };
        return { texto: 'Indefinido', clase: 'bg-warning' };
    }

    // --- MOSTRAR ERROR EN TABLA ---
    function mostrarErrorEnTabla(mensaje) {
        tablaDatos.innerHTML = `<tr><td colspan="7" class="text-center text-danger">${mensaje}</td></tr>`;
        emptyState.classList.add('d-none');
    }

    // --- ABRIR MODAL ---
    window.abrirModal = (id = 0, codigo = '', idPrograma = '', fechaInicio = '', fechaFinal = '') => {
        idInput.value = id;
        codigoInput.value = codigo;
        programaIdInput.value = idPrograma;
        fechaInicioInput.value = fechaInicio ? fechaInicio.split('T')[0] : '';
        fechaFinalInput.value = fechaFinal ? fechaFinal.split('T')[0] : '';
        modalTitulo.textContent = id === 0 ? 'Crear Nueva Ficha' : 'Editar Ficha';
        editorModal.show();
    };

    // --- GUARDAR FICHA ---
    window.guardar = async () => {
        const id = idInput.value;
        const esNuevo = id == 0;
        const data = {
            idFicha: parseInt(id) || 0,
            codigo: codigoInput.value,
            idPrograma: parseInt(programaIdInput.value),
            fechaInicio: fechaInicioInput.value || null,
            fechaFinal: fechaFinalInput.value || null
        };

        const url = esNuevo ? fichasApiUrl : `${fichasApiUrl}/${id}`;
        const method = esNuevo ? 'POST' : 'PUT';

        try {
            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Error al guardar.');
            }

            editorModal.hide();
            await cargarDatos(paginaActual); // Mantener en la página actual
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // --- DESACTIVAR FICHA ---
    window.desactivar = async (id) => {
        if (!confirm('¿Está seguro de que desea borrar (desactivar) esta ficha?')) return;
        try {
            const response = await fetch(`${fichasApiUrl}/${id}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al borrar.');
            await cargarDatos(paginaActual);
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // --- LIMPIAR FILTROS ---
    window.limpiarFiltros = function () {
        searchInput.value = '';
        programaFilter.value = '';
        estadoFilter.value = '';
        paginaActual = 1;
        cargarDatos(1);
    };

    // --- EVENTOS DE FILTROS ---
    searchInput.addEventListener('input', () => {
        clearTimeout(window.searchTimeout);
        window.searchTimeout = setTimeout(() => aplicarFiltros(), 500);
    });

    programaFilter.addEventListener('change', () => aplicarFiltrosLocales([]));
    estadoFilter.addEventListener('change', () => aplicarFiltrosLocales([]));

    // --- INICIALIZACIÓN ---
    cargarDatos();
    cargarProgramas();
});