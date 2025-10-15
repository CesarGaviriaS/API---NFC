document.addEventListener('DOMContentLoaded', function () {
    const fichasApiUrl = '/api/ficha';
    const programasApiUrl = '/api/programa'; // API para llenar el dropdown
    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));

    // Referencias a los inputs del formulario
    const idInput = document.getElementById('idInput');
    const codigoInput = document.getElementById('codigoInput');
    const programaIdInput = document.getElementById('programaIdInput');
    const fechaInicioInput = document.getElementById('fechaInicioInput');
    const fechaFinalInput = document.getElementById('fechaFinalInput');

    const modalTitulo = document.getElementById('modalTitulo');
    const tablaDatos = document.getElementById('tablaDatos');

    // --- CARGAR PROGRAMAS EN EL SELECTOR ---
    const cargarProgramas = async () => {
        try {
            const response = await fetch(programasApiUrl);
            const programas = await response.json();
            programaIdInput.innerHTML = '<option value="">Seleccione un programa...</option>';
            programas.forEach(prog => {
                programaIdInput.innerHTML += `<option value="${prog.idPrograma}">${prog.nombrePrograma}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar programas:", error);
            programaIdInput.innerHTML = '<option value="">Error al cargar programas</option>';
        }
    };

    // --- CARGAR DATOS DE FICHAS EN LA TABLA ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(fichasApiUrl);
            if (!response.ok) throw new Error('Error al cargar los datos de las fichas.');
            const data = await response.json();

            tablaDatos.innerHTML = '';
            if (data.length === 0) {
                tablaDatos.innerHTML = '<tr><td colspan="6" class="text-center">No hay fichas para mostrar.</td></tr>';
                return;
            }

            data.forEach(item => {
                const nombrePrograma = item.programa ? item.programa.nombrePrograma : '<em class="text-muted">N/A</em>';
                const fechaInicio = item.fechaInicio ? new Date(item.fechaInicio).toLocaleDateString() : 'N/A';
                const fechaFinal = item.fechaFinal ? new Date(item.fechaFinal).toLocaleDateString() : 'N/A';

                tablaDatos.innerHTML += `
                    <tr>
                        <td>${item.idFicha}</td>
                        <td>${item.codigo}</td>
                        <td>${nombrePrograma}</td>
                        <td>${fechaInicio}</td>
                        <td>${fechaFinal}</td>
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
        } catch (error) {
            console.error(error);
            tablaDatos.innerHTML = `<tr><td colspan="6" class="text-center text-danger">${error.message}</td></tr>`;
        }
    };

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
            cargarDatos();
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
            cargarDatos();
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // --- Carga inicial de datos ---
    cargarDatos();
    cargarProgramas();
});