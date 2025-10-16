document.addEventListener('DOMContentLoaded', function () {
    const apiUrl = '/api/programa';
    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));
    // variables globales
    let todosProgramas = [];

    // Referencias a los inputs del formulario
    const idInput = document.getElementById('idInput');
    const nombreInput = document.getElementById('nombreInput');
    const codigoInput = document.getElementById('codigoInput');
    const nivelInput = document.getElementById('nivelInput');

    const modalTitulo = document.getElementById('modalTitulo');
    const tablaDatos = document.getElementById('tablaDatos');
    // referencias a lemntos de filtrado 
    const searchInput = document.getElementById('searchInput');
    const nivelFilter = document.getElementById('nivelFilter');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');



    // --- CARGAR DATOS EN LA TABLA ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(apiUrl);
            if (!response.ok) throw new Error('Error al cargar los datos.');
            // Guardar varible global
            todosProgramas = await response.json();
            // filtrado de niveles 
            llenarFiltroNiveles();
            aplicarFiltros();
        } catch (error) {
            console.error(error);
            tablaDatos.innerHTML = `<tr><td colspan="5" class="text-center text-danger">${error.message}</td></tr>`;
        }
    };
    // llenar filtrado de niveles dinamicos 
    const llenarFiltroNiveles = () => {
        // Obtener niveles únicos 
        const niveles = [...new Set(todosProgramas.
            map(p => p.nivelFormacion)
            .filter(n => n))];// filtrar valores vacios/ null
        nivelFilter.innerHTML = '<option value="">Todos los niveles</option>';
        niveles.forEach(nivel => {
            nivelFilter.innerHTML += `<option value="${nivel}">${nivel}</option>`;
        });
    };

    // funcion de filtrado 
    const aplicarFiltros = () => {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const nivelSeleccionado = nivelFilter.value;

        // filtra programas 
        const programasFiltrados = todosProgramas.filter(programa => {
            // filtrado de busqueda(nombre o codigo)
            const matchSearch = searchTerm === '' ||
                programa.nombrePrograma.toLowerCase().includes(searchTerm) ||
                programa.codigo.toLowerCase().includes(searchTerm);

            // filtrado de nivel
            const matchNivel = nivelSeleccionado === ''||
            programa.nivelFormacion === nivelSeleccionado;

            return matchSearch && matchNivel;
        });
        // renderizar tabla con resultados filtrados 
        renderizarTabla(programasFiltrados);

    };


    // separar la logica de renderizado 
    const renderizarTabla = (programas) => {
        tablaDatos.innerHTML = '';
        resultadosContador.textContent = programas.length;

        if (programas.length === 0) {
            emptyState.classList.remove('d-none');
            return;
        }
        emptyState.classList.add('d-none');

        programas.forEach(item => {
            tablaDatos.innerHTML += `
             <tr>
                    <td class="fw-semibold">${item.idPrograma}</td>
                    <td><span class="badge bg-light text-dark">${item.codigo}</span></td>
                    <td class="fw-semibold">${item.nombrePrograma}</td>
                    <td><span class="badge bg-info">${item.nivelFormacion || 'Sin nivel'}</span></td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-primary" onclick="abrirModal(${item.idPrograma}, '${item.nombrePrograma}', '${item.codigo}', '${item.nivelFormacion || ''}')">
                            <i class="bi bi-pencil-fill"></i>
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idPrograma})">
                            <i class="bi bi-trash-fill"></i>
                        </button>
                    </td>
                </tr>`;
        })
    }

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
    // limpiar filtros
    window.limpiarFiltros = () => {
        searchInput.value = '';
        nivelFilter.value = '';
        aplicarFiltros();

    }
    searchInput?.addEventListener('input', aplicarFiltros);
    nivelFilter?.addEventListener('change', aplicarFiltros);

    // Carga inicial de datos
    cargarDatos();
});