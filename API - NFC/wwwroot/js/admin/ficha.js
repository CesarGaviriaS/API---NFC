document.addEventListener('DOMContentLoaded', function () {
    const fichasApiUrl = '/api/ficha';
    const programasApiUrl = '/api/programa';
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

    let todasLasFichas = []; // Almacenar todas las fichas para filtrar

    // --- CARGAR PROGRAMAS EN EL SELECTOR DEL FORMULARIO ---
    const cargarProgramas = async () => {
        try {
            const response = await fetch(programasApiUrl);
            const programas = await response.json();

            // Limpiar y llenar el select del formulario
            programaIdInput.innerHTML = '<option value="">Seleccione un programa...</option>';
            programas.forEach(prog => {
                programaIdInput.innerHTML += `<option value="${prog.idPrograma}">${prog.nombrePrograma}</option>`;
            });

            // Limpiar y llenar el select de filtros
            programaFilter.innerHTML = '<option value="">Todos los programas</option>';
            programas.forEach(prog => {
                programaFilter.innerHTML += `<option value="${prog.idPrograma}">${prog.nombrePrograma}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar programas:", error);
            programaIdInput.innerHTML = '<option value="">Error al cargar programas</option>';
            programaFilter.innerHTML = '<option value="">Error al cargar programas</option>';
        }
    };

    // --- CARGAR DATOS DE FICHAS EN LA TABLA ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(fichasApiUrl);
            if (!response.ok) throw new Error('Error al cargar los datos de las fichas.');
            const data = await response.json();

            todasLasFichas = data; // Guardar todas las fichas para filtrar
            aplicarFiltros(); // Aplicar filtros iniciales
        } catch (error) {
            console.error(error);
            mostrarErrorEnTabla(error.message);
        }
    };

    // --- APLICAR FILTROS ---
    window.aplicarFiltros = function () {
        const searchTerm = searchInput.value.toLowerCase();
        const programaSeleccionado = programaFilter.value;
        const estadoSeleccionado = estadoFilter.value;

        let fichasFiltradas = todasLasFichas.filter(ficha => {
            // Filtro por búsqueda en código
            const coincideBusqueda = !searchTerm ||
                ficha.codigo.toLowerCase().includes(searchTerm);

            // Filtro por programa
            const coincidePrograma = !programaSeleccionado ||
                ficha.idPrograma.toString() === programaSeleccionado;

            // Filtro por estado
            const coincideEstado = filtrarPorEstado(ficha, estadoSeleccionado);

            return coincideBusqueda && coincidePrograma && coincideEstado;
        });

        mostrarFichasFiltradas(fichasFiltradas);
    };

    // --- FILTRAR POR ESTADO ---
    function filtrarPorEstado(ficha, estado) {
        if (!estado) return true;

        const fechaInicio = ficha.fechaInicio ? new Date(ficha.fechaInicio) : null;
        const fechaFinal = ficha.fechaFinal ? new Date(ficha.fechaFinal) : null;
        const hoy = new Date();

        switch (estado) {
            case 'activa':
                // Ficha activa: tiene fecha de inicio pasada y fecha final futura (o sin fecha final)
                return fechaInicio && fechaInicio <= hoy &&
                    (!fechaFinal || fechaFinal >= hoy);

            case 'finalizada':
                // Ficha finalizada: tiene fecha final pasada
                return fechaFinal && fechaFinal < hoy;

            case 'proxima':
                // Ficha próxima: fecha de inicio futura
                return fechaInicio && fechaInicio > hoy;

            default:
                return true;
        }
    }

    // --- MOSTRAR FICHAS FILTRADAS ---
    function mostrarFichasFiltradas(fichas) {
        tablaDatos.innerHTML = '';

        // Actualizar contador
        resultadosContador.textContent = fichas.length;

        if (fichas.length === 0) {
            emptyState.classList.remove('d-none');
            tablaDatos.innerHTML = '';
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
                    <td>
                        <span class="badge ${estado.clase}">${estado.texto}</span>
                    </td>
                    <td>
                        <button class="btn btn-sm btn-warning" onclick="abrirModal(${item.idFicha}, '${item.codigo}', '${item.idPrograma}', '${item.fechaInicio}', '${item.fechaFinal}')">
                            <i class="bi bi-pencil-fill"></i> Editar
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idFicha})">
                            <i class="bi bi-trash-fill"></i> Borrar
                        </button>
                    </td>
                </tr>`;
        });
    }

    // --- DETERMINAR ESTADO DE LA FICHA ---
    function determinarEstado(ficha) {
        const fechaInicio = ficha.fechaInicio ? new Date(ficha.fechaInicio) : null;
        const fechaFinal = ficha.fechaFinal ? new Date(ficha.fechaFinal) : null;
        const hoy = new Date();

        if (!fechaInicio && !fechaFinal) {
            return { texto: 'Sin fecha', clase: 'bg-secondary' };
        }

        if (fechaInicio && fechaInicio > hoy) {
            return { texto: 'Próxima', clase: 'bg-info' };
        }

        if (fechaFinal && fechaFinal < hoy) {
            return { texto: 'Finalizada', clase: 'bg-danger' };
        }

        if (fechaInicio && fechaInicio <= hoy && (!fechaFinal || fechaFinal >= hoy)) {
            return { texto: 'En curso', clase: 'bg-success' };
        }

        return { texto: 'Indefinido', clase: 'bg-warning' };
    }

    // --- MOSTRAR ERROR EN TABLA ---
    function mostrarErrorEnTabla(mensaje) {
        tablaDatos.innerHTML = `<tr><td colspan="7" class="text-center text-danger">${mensaje}</td></tr>`;
        emptyState.classList.add('d-none');
    }

    // --- ABRIR EL MODAL ---
    window.abrirModal = (id = 0, codigo = '', idPrograma = '', fechaInicio = '', fechaFinal = '') => {
        idInput.value = id;
        codigoInput.value = codigo;
        programaIdInput.value = idPrograma;

        fechaInicioInput.value = fechaInicio ? fechaInicio.split('T')[0] : '';
        fechaFinalInput.value = fechaFinal ? fechaFinal.split('T')[0] : '';

        modalTitulo.textContent = id === 0 ? 'Crear Nueva Ficha' : 'Editar Ficha';
        editorModal.show();
    };

    // --- GUARDAR (CREAR O ACTUALIZAR) ---
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
            await cargarDatos(); // Recargar datos después de guardar
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // --- DESACTIVAR (BORRADO LÓGICO) ---
    window.desactivar = async (id) => {
        if (!confirm('¿Está seguro de que desea borrar (desactivar) esta ficha?')) {
            return;
        }
        try {
            const response = await fetch(`${fichasApiUrl}/${id}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al borrar.');
            await cargarDatos(); // Recargar datos después de borrar
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
        aplicarFiltros();
    };

    // --- Carga inicial de datos ---
    cargarDatos();
    cargarProgramas();
});