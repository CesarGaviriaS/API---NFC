document.addEventListener('DOMContentLoaded', function () {
    const apiUrl = '/api/programa';
    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));

    // Referencias a los inputs del formulario
    const idInput = document.getElementById('idInput');
    const nombreInput = document.getElementById('nombreInput');
    const codigoInput = document.getElementById('codigoInput');
    const nivelInput = document.getElementById('nivelInput');

    const modalTitulo = document.getElementById('modalTitulo');
    const tablaDatos = document.getElementById('tablaDatos');

    // --- CARGAR DATOS EN LA TABLA ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(apiUrl);
            if (!response.ok) throw new Error('Error al cargar los datos.');
            const data = await response.json();

            tablaDatos.innerHTML = ''; // Limpiar tabla
            if (data.length === 0) {
                tablaDatos.innerHTML = '<tr><td colspan="5" class="text-center">No hay programas para mostrar.</td></tr>';
                return;
            }

            data.forEach(item => {
                tablaDatos.innerHTML += `
                    <tr>
                        <td>${item.idPrograma}</td>
                        <td>${item.codigo}</td>
                        <td>${item.nombrePrograma}</td>
                        <td>${item.nivelFormacion}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="abrirModal(${item.idPrograma}, '${item.nombrePrograma}', '${item.codigo}', '${item.nivelFormacion}')">
                                <i class="bi bi-pencil-fill"></i> Editar
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idPrograma})">
                                <i class="bi bi-trash-fill"></i> Borrar
                            </button>
                        </td>
                    </tr>`;
            });
        } catch (error) {
            console.error(error);
            tablaDatos.innerHTML = `<tr><td colspan="5" class="text-center text-danger">${error.message}</td></tr>`;
        }
    };

    // --- ABRIR EL MODAL (PARA CREAR O EDITAR) ---
    window.abrirModal = (id = 0, nombre = '', codigo = '', nivel = '') => {
        idInput.value = id;
        nombreInput.value = nombre;
        codigoInput.value = codigo;
        nivelInput.value = nivel;
        modalTitulo.textContent = id === 0 ? 'Crear Nuevo Programa' : 'Editar Programa';
        editorModal.show();
    };

    // --- GUARDAR (CREAR O ACTUALIZAR) ---
    window.guardar = async () => {
        const id = idInput.value;
        const esNuevo = id == 0;

        const data = {
            idPrograma: parseInt(id) || 0,
            nombrePrograma: nombreInput.value,
            codigo: codigoInput.value,
            nivelFormacion: nivelInput.value
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

    // --- DESACTIVAR (BORRADO LÓGICO) ---
    window.desactivar = async (id) => {
        if (!confirm('¿Está seguro de que desea borrar (desactivar) este programa?')) {
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

    // Carga inicial de datos
    cargarDatos();
});