document.addEventListener('DOMContentLoaded', function () {
    const apiUrl = '/api/tipoproceso';
    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));

    // Referencias a los inputs
    const idInput = document.getElementById('idInput');
    const tipoInput = document.getElementById('tipoInput');
    const modalTitulo = document.getElementById('modalTitulo');
    const tablaDatos = document.getElementById('tablaDatos');

    // --- CARGAR DATOS EN LA TABLA ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(apiUrl);
            if (!response.ok) throw new Error('Error al cargar los datos.');
            const data = await response.json();

            tablaDatos.innerHTML = '';
            if (data.length === 0) {
                tablaDatos.innerHTML = '<tr><td colspan="3" class="text-center">No hay datos para mostrar.</td></tr>';
                return;
            }

            data.forEach(item => {
                tablaDatos.innerHTML += `
                    <tr>
                        <td>${item.idTipoProceso}</td>
                        <td>${item.tipo}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="abrirModal(${item.idTipoProceso}, '${item.tipo}')">
                                <i class="bi bi-pencil-fill"></i> Editar
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idTipoProceso})">
                                <i class="bi bi-trash-fill"></i> Borrar
                            </button>
                        </td>
                    </tr>`;
            });
        } catch (error) {
            console.error(error);
            tablaDatos.innerHTML = `<tr><td colspan="3" class="text-center text-danger">${error.message}</td></tr>`;
        }
    };

    // --- ABRIR EL MODAL ---
    window.abrirModal = (id = 0, tipo = '') => {
        idInput.value = id;
        tipoInput.value = tipo;
        modalTitulo.textContent = id === 0 ? 'Crear Nuevo Tipo de Proceso' : 'Editar Tipo de Proceso';
        editorModal.show();
    };

    // --- GUARDAR ---
    window.guardar = async () => {
        const id = idInput.value;
        const esNuevo = id == 0;

        const data = {
            idTipoProceso: parseInt(id) || 0,
            tipo: tipoInput.value
        };

        const url = esNuevo ? apiUrl : `${apiUrl}/${id}`;
        const method = esNuevo ? 'POST' : 'PUT';

        try {
            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) throw new Error('Error al guardar.');

            editorModal.hide();
            cargarDatos();
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // --- DESACTIVAR ---
    window.desactivar = async (id) => {
        if (!confirm('¿Está seguro de que desea borrar (desactivar) este tipo de proceso?')) {
            return;
        }
        try {
            const response = await fetch(`${apiUrl}/${id}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al borrar.');
            cargarDatos();
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // Carga inicial
    cargarDatos();
});