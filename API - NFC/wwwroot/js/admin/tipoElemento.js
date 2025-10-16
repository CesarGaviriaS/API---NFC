document.addEventListener('DOMContentLoaded', function () {
    const apiUrl = '/api/tipoelemento';
    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));
    const idInput = document.getElementById('idInput');
    const nombreInput = document.getElementById('nombreInput');
    const modalTitulo = document.getElementById('modalTitulo');
    const tablaDatos = document.getElementById('tablaDatos');
    const searchInput = document.getElementById('searchInput');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');

    let datosOriginales = [];

    // --- CARGAR DATOS EN LA TABLA ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(apiUrl);
            if (!response.ok) throw new Error('Error al cargar los datos.');
            const data = await response.json();

            datosOriginales = data;
            aplicarFiltros();

            // Configurar evento de búsqueda en tiempo real
            searchInput.addEventListener('input', aplicarFiltros);

        } catch (error) {
            console.error(error);
            mostrarError('Error al cargar los datos: ' + error.message);
        }
    };

    // --- APLICAR FILTROS DE BÚSQUEDA ---
    const aplicarFiltros = () => {
        const searchTerm = searchInput.value.toLowerCase().trim();

        let datosFiltrados = datosOriginales;

        // Aplicar filtro de búsqueda
        if (searchTerm) {
            datosFiltrados = datosOriginales.filter(item =>
                item.nombreTipoElemento.toLowerCase().includes(searchTerm) ||
                item.idTipoElemento.toString().includes(searchTerm)
            );
        }

        actualizarTabla(datosFiltrados);
        actualizarContador(datosFiltrados.length);
    };

    // --- ACTUALIZAR TABLA CON DATOS FILTRADOS ---
    const actualizarTabla = (datos) => {
        tablaDatos.innerHTML = '';

        if (datos.length === 0) {
            emptyState.classList.remove('d-none');
            return;
        }

        emptyState.classList.add('d-none');

        datos.forEach(item => {
            const fila = document.createElement('tr');
            fila.innerHTML = `
                        <td class="fw-semibold">${item.idTipoElemento}</td>
                        <td>
                            <span class="badge bg-light text-dark fs-6">${item.nombreTipoElemento}</span>
                        </td>
                        <td class="text-center">
                            <button class="btn btn-sm btn-warning me-1" onclick="abrirModal(${item.idTipoElemento}, '${item.nombreTipoElemento.replace(/'/g, "\\'")}')">
                                <i class="bi bi-pencil-fill"></i> Editar
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idTipoElemento})">
                                <i class="bi bi-trash-fill"></i> Borrar
                            </button>
                        </td>
                    `;
            tablaDatos.appendChild(fila);
        });
    };

    // --- ACTUALIZAR CONTADOR DE RESULTADOS ---
    const actualizarContador = (cantidad) => {
        resultadosContador.textContent = cantidad;
        resultadosContador.className = `badge py-2 px-3 ${cantidad > 0 ? 'bg-success' : 'bg-secondary'}`;
    };

    // --- LIMPIAR FILTROS ---
    window.limpiarFiltros = () => {
        searchInput.value = '';
        aplicarFiltros();
    };

    // --- MOSTRAR ERROR ---
    const mostrarError = (mensaje) => {
        tablaDatos.innerHTML = `
                    <tr>
                        <td colspan="4" class="text-center text-danger py-4">
                            <i class="bi bi-exclamation-triangle-fill me-2"></i>
                            ${mensaje}
                        </td>
                    </tr>`;
        emptyState.classList.add('d-none');
    };

    // --- ABRIR EL MODAL (PARA CREAR O EDITAR) ---
    window.abrirModal = (id = 0, nombre = '') => {
        idInput.value = id;
        nombreInput.value = nombre;
        modalTitulo.textContent = id === 0 ? 'Crear Nuevo Tipo de Elemento' : 'Editar Tipo de Elemento';

        // Limpiar validación
        nombreInput.classList.remove('is-invalid');

        editorModal.show();

        // Enfocar el input al abrir el modal
        setTimeout(() => nombreInput.focus(), 500);
    };

    // --- VALIDAR FORMULARIO ---
    const validarFormulario = () => {
        const nombre = nombreInput.value.trim();

        if (!nombre) {
            nombreInput.classList.add('is-invalid');
            nombreInput.focus();
            return false;
        }

        nombreInput.classList.remove('is-invalid');
        return true;
    };

    // --- GUARDAR (CREAR O ACTUALIZAR) ---
    window.guardar = async () => {
        if (!validarFormulario()) {
            return;
        }

        const id = idInput.value;
        const esNuevo = id == 0;

        const data = {
            idTipoElemento: parseInt(id) || 0,
            nombreTipoElemento: nombreInput.value.trim()
        };

        const url = esNuevo ? apiUrl : `${apiUrl}/${id}`;
        const method = esNuevo ? 'POST' : 'PUT';

        try {
            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(errorData || 'Error al guardar');
            }

            editorModal.hide();

            // Mostrar mensaje de éxito
            mostrarMensajeExito(esNuevo ? 'Tipo de elemento creado exitosamente' : 'Tipo de elemento actualizado exitosamente');

            cargarDatos();
        } catch (error) {
            console.error(error);
            mostrarError('Error al guardar: ' + error.message);
        }
    };

    // --- MOSTRAR MENSAJE DE ÉXITO ---
    const mostrarMensajeExito = (mensaje) => {
        // Puedes implementar un toast o alerta aquí
        console.log(mensaje);
        // Ejemplo con alerta simple:
        alert(mensaje);
    };

    // --- DESACTIVAR (BORRADO LÓGICO) ---
    window.desactivar = async (id) => {
        const elemento = datosOriginales.find(item => item.idTipoElemento === id);
        const nombre = elemento ? elemento.nombreTipoElemento : 'este elemento';

        if (!confirm(`¿Está seguro de que desea borrar (desactivar) "${nombre}"?`)) {
            return;
        }

        try {
            const response = await fetch(`${apiUrl}/${id}`, {
                method: 'DELETE'
            });

            if (!response.ok) throw new Error('Error al borrar.');

            mostrarMensajeExito('Tipo de elemento borrado exitosamente');
            cargarDatos();
        } catch (error) {
            console.error(error);
            mostrarError('Error al borrar: ' + error.message);
        }
    };

    // Carga inicial de datos
    cargarDatos();
});