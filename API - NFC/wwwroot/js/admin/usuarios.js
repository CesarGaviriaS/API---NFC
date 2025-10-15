document.addEventListener('DOMContentLoaded', function () {
    // APIs
    const usuariosApiUrl = '/api/usuario';
    const aprendizApiUrl = '/api/aprendiz';
    const funcionarioApiUrl = '/api/funcionario';
    const fichasApiUrl = '/api/ficha';

    // Modales
    const aprendizModal = new bootstrap.Modal(document.getElementById('aprendizModal'));
    const funcionarioModal = new bootstrap.Modal(document.getElementById('funcionarioModal'));

    // Formulario Aprendiz
    const aprendizIdInput = document.getElementById('aprendizIdInput');
    const aprendizNombreInput = document.getElementById('aprendizNombreInput');
    const aprendizDocumentoInput = document.getElementById('aprendizDocumentoInput');
    const aprendizFichaIdInput = document.getElementById('aprendizFichaIdInput');
    const aprendizModalTitulo = document.getElementById('aprendizModalTitulo');

    // Formulario Funcionario
    const funcionarioIdInput = document.getElementById('funcionarioIdInput');
    const funcionarioNombreInput = document.getElementById('funcionarioNombreInput');
    const funcionarioDocumentoInput = document.getElementById('funcionarioDocumentoInput');
    const funcionarioDetalleInput = document.getElementById('funcionarioDetalleInput');
    const funcionarioModalTitulo = document.getElementById('funcionarioModalTitulo');

    // Elementos de filtrado
    const searchInput = document.getElementById('searchInput');
    const rolFilter = document.getElementById('rolFilter');
    const fichaFilter = document.getElementById('fichaFilter');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');

    const tablaDatos = document.getElementById('tablaDatos');
    let listaUsuariosCompleta = []; // Almacenamos los datos completos
    let listaFichas = []; // Almacenamos las fichas para el filtro

    // --- CARGAR DATOS EN LA TABLA PRINCIPAL ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(usuariosApiUrl);
            listaUsuariosCompleta = await response.json();
            aplicarFiltros(); // Aplicar filtros después de cargar
        } catch (error) {
            console.error("Error al cargar usuarios:", error);
        }
    };

    // --- APLICAR FILTROS ---
    const aplicarFiltros = () => {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const rolSeleccionado = rolFilter.value;
        const fichaSeleccionada = fichaFilter.value;

        // Filtrar usuarios
        const usuariosFiltrados = listaUsuariosCompleta.filter(user => {
            let rol, nombre, documento, fichaId;

            if (user.aprendiz) {
                rol = 'Aprendiz';
                nombre = user.aprendiz.nombre;
                documento = user.aprendiz.documento;
                fichaId = user.aprendiz.idFicha;
            } else if (user.funcionario) {
                rol = 'Funcionario';
                nombre = user.funcionario.nombre;
                documento = user.funcionario.documento;
                fichaId = null;
            }

            // Filtro de búsqueda
            const matchSearch = searchTerm === '' ||
                nombre.toLowerCase().includes(searchTerm) ||
                documento.includes(searchTerm);

            // Filtro de rol
            const matchRol = rolSeleccionado === '' || rol === rolSeleccionado;

            // Filtro de ficha
            const matchFicha = fichaSeleccionada === '' ||
                (fichaId && fichaId.toString() === fichaSeleccionada);

            return matchSearch && matchRol && matchFicha;
        });

        renderizarTabla(usuariosFiltrados);
    };

    // --- RENDERIZAR TABLA ---
    const renderizarTabla = (usuarios) => {
        tablaDatos.innerHTML = '';
        resultadosContador.textContent = usuarios.length;

        if (usuarios.length === 0) {
            emptyState.classList.remove('d-none');
            return;
        }

        emptyState.classList.add('d-none');

        usuarios.forEach(user => {
            let rol, nombre, documento, detalle, editId, badgeClass;

            if (user.aprendiz) {
                rol = 'Aprendiz';
                nombre = user.aprendiz.nombre;
                documento = user.aprendiz.documento;
                detalle = user.aprendiz.ficha ? user.aprendiz.ficha.codigo : 'Sin Ficha';
                editId = user.aprendiz.idAprendiz;
                badgeClass = 'bg-primary';
            } else if (user.funcionario) {
                rol = 'Funcionario';
                nombre = user.funcionario.nombre;
                documento = user.funcionario.documento;
                detalle = user.funcionario.detalle || 'N/A';
                editId = user.funcionario.idFuncionario;
                badgeClass = 'bg-warning text-dark';
            }

            const rolBadge = rol === 'Aprendiz'
                ? `<span class="badge ${badgeClass} badge-role"><i class="bi bi-mortarboard-fill me-1"></i>${rol}</span>`
                : `<span class="badge ${badgeClass} badge-role"><i class="bi bi-person-badge-fill me-1"></i>${rol}</span>`;

            const infoAdicional = rol === 'Aprendiz'
                ? `<span class="badge bg-success bg-opacity-10 text-success">${detalle}</span>`
                : `<span class="text-muted">${detalle}</span>`;

            tablaDatos.innerHTML += `
                <tr>
                    <td class="fw-semibold">${user.idUsuario}</td>
                    <td>${rolBadge}</td>
                    <td class="fw-semibold">${nombre}</td>
                    <td><span class="badge bg-light text-dark">${documento}</span></td>
                    <td>${infoAdicional}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-primary" onclick="abrirModal('${rol.toLowerCase()}', ${editId})">
                            <i class="bi bi-pencil-fill"></i>
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="desactivar(${user.idUsuario})">
                            <i class="bi bi-trash-fill"></i>
                        </button>
                    </td>
                </tr>`;
        });
    };

    // --- CARGAR FICHAS PARA EL SELECTOR Y FILTRO ---
    const cargarFichas = async () => {
        try {
            const response = await fetch(fichasApiUrl);
            listaFichas = await response.json();

            // Llenar select del modal de aprendiz
            aprendizFichaIdInput.innerHTML = '<option value="">Seleccione una ficha...</option>';
            listaFichas.forEach(ficha => {
                aprendizFichaIdInput.innerHTML += `<option value="${ficha.idFicha}">${ficha.codigo} - ${ficha.programa.nombrePrograma}</option>`;
            });

            // Llenar filtro de fichas
            fichaFilter.innerHTML = '<option value="">Todas las fichas</option>';
            listaFichas.forEach(ficha => {
                fichaFilter.innerHTML += `<option value="${ficha.idFicha}">${ficha.codigo}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar fichas:", error);
        }
    };

    // --- LIMPIAR FILTROS ---
    window.limpiarFiltros = () => {
        searchInput.value = '';
        rolFilter.value = '';
        fichaFilter.value = '';
        aplicarFiltros();
    };

    // --- EVENT LISTENERS PARA FILTROS EN TIEMPO REAL ---
    searchInput.addEventListener('input', aplicarFiltros);
    rolFilter.addEventListener('change', aplicarFiltros);
    fichaFilter.addEventListener('change', aplicarFiltros);

    // --- ABRIR EL MODAL CORRESPONDIENTE ---
    window.abrirModal = (tipo, id = 0) => {
        if (tipo === 'aprendiz') {
            aprendizModalTitulo.textContent = id === 0 ? 'Crear Nuevo Aprendiz' : 'Editar Aprendiz';
            document.getElementById('aprendizForm').reset();
            aprendizIdInput.value = id;

            if (id !== 0) {
                const usuario = listaUsuariosCompleta.find(u => u.aprendiz && u.aprendiz.idAprendiz === id);
                if (usuario && usuario.aprendiz) {
                    aprendizNombreInput.value = usuario.aprendiz.nombre;
                    aprendizDocumentoInput.value = usuario.aprendiz.documento;
                    aprendizFichaIdInput.value = usuario.aprendiz.idFicha;
                }
            }
            aprendizModal.show();
        } else if (tipo === 'funcionario') {
            funcionarioModalTitulo.textContent = id === 0 ? 'Crear Nuevo Funcionario' : 'Editar Funcionario';
            document.getElementById('funcionarioForm').reset();
            funcionarioIdInput.value = id;

            if (id !== 0) {
                const usuario = listaUsuariosCompleta.find(u => u.funcionario && u.funcionario.idFuncionario === id);
                if (usuario && usuario.funcionario) {
                    funcionarioNombreInput.value = usuario.funcionario.nombre;
                    funcionarioDocumentoInput.value = usuario.funcionario.documento;
                    funcionarioDetalleInput.value = usuario.funcionario.detalle;
                }
            }
            funcionarioModal.show();
        }
    };

    // --- GUARDAR (CREA O ACTUALIZA APRENDIZ/FUNCIONARIO) ---
    window.guardar = async (tipo) => {
        let url, method, data, modal;

        if (tipo === 'aprendiz') {
            const id = aprendizIdInput.value;
            url = id == 0 ? aprendizApiUrl : `${aprendizApiUrl}/${id}`;
            method = id == 0 ? 'POST' : 'PUT';
            modal = aprendizModal;
            data = {
                idAprendiz: parseInt(id) || 0,
                nombre: aprendizNombreInput.value,
                documento: aprendizDocumentoInput.value,
                idFicha: parseInt(aprendizFichaIdInput.value)
            };
        } else { // funcionario
            const id = funcionarioIdInput.value;
            url = id == 0 ? funcionarioApiUrl : `${funcionarioApiUrl}/${id}`;
            method = id == 0 ? 'POST' : 'PUT';
            modal = funcionarioModal;
            data = {
                idFuncionario: parseInt(id) || 0,
                nombre: funcionarioNombreInput.value,
                documento: funcionarioDocumentoInput.value,
                detalle: funcionarioDetalleInput.value
            };
        }

        try {
            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            if (!response.ok) throw new Error(await response.text());

            modal.hide();
            cargarDatos();
        } catch (error) {
            console.error(`Error al guardar ${tipo}:`, error);
            alert(`Error: ${error.message}`);
        }
    };

    // --- DESACTIVAR (BORRADO LÓGICO DE USUARIO) ---
    window.desactivar = async (idUsuario) => {
        if (!confirm(`¿Está seguro de que desea borrar (desactivar) al usuario con ID ${idUsuario}?`)) return;

        try {
            const response = await fetch(`${usuariosApiUrl}/${idUsuario}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al borrar el usuario.');
            cargarDatos();
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // --- Carga inicial de datos ---
    cargarDatos();
    cargarFichas();
});