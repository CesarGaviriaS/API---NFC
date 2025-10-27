document.addEventListener('DOMContentLoaded', function () {
    // APIs
    const usuariosApiUrl = '/api/usuario';
    const usuariosPaginatedApiUrl = '/api/usuario/paginated';
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
    const funcionarioContraseñaInput = document.getElementById('funcionarioContraseñaInput');
    const funcionarioDetalleInput = document.getElementById('funcionarioDetalleInput');
    const funcionarioModalTitulo = document.getElementById('funcionarioModalTitulo');

    // Elementos de filtrado
    const searchInput = document.getElementById('searchInput');
    const rolFilter = document.getElementById('rolFilter');
    const fichaFilter = document.getElementById('fichaFilter');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');

    const tablaDatos = document.getElementById('tablaDatos');
    let listaUsuariosCompleta = [];
    let listaFichas = [];

    // Variables globales para paginacion 
    let paginaActual = 1;
    let registrosPorPagina = 10;
    let searchTermActual = '';

    // --- CARGAR DATOS PAGINADOS ---
    const cargarDatosPaginated = async (pagina = 1, searchTerm = '') => {
        try {
            paginaActual = pagina;
            searchTermActual = searchTerm;

            const params = new URLSearchParams({
                page: pagina,
                pageSize: registrosPorPagina
            });

            if (searchTerm) {
                params.append('search', searchTerm);
            }

            const response = await fetch(`${usuariosPaginatedApiUrl}?${params}`);
            if (!response.ok) throw new Error('Error al cargar datos');

            const data = await response.json();
            listaUsuariosCompleta = data.data;
            renderizarTabla(listaUsuariosCompleta);
            generarPaginacion(data.page, data.totalPages, data.totalRecords);

        } catch (error) {
            console.error("Error al cargar usuarios paginados:", error);
            await cargarDatosCompletos();
        }
    };

    // --- GENERAR PAGINACIÓN ---
    const generarPaginacion = (paginaActual, totalPaginas, totalRecords) => {
        let paginacionContainer = document.getElementById('paginacionContainer');

        if (!paginacionContainer) {
            const tablaParent = document.querySelector('.table-responsive').parentNode;
            const nuevoContenedor = document.createElement('div');
            nuevoContenedor.className = 'd-flex justify-content-between align-items-center mt-3';
            nuevoContenedor.id = 'paginacionContainer';
            nuevoContenedor.innerHTML = `
                <div id="infoPaginacion"></div>
                <nav id="paginacionNav"></nav>
            `;
            tablaParent.appendChild(nuevoContenedor);
            paginacionContainer = nuevoContenedor;
        }

        const infoPaginacion = document.getElementById('infoPaginacion');
        const paginacionNav = document.getElementById('paginacionNav');

        const inicio = ((paginaActual - 1) * registrosPorPagina) + 1;
        const fin = Math.min(paginaActual * registrosPorPagina, totalRecords);

        infoPaginacion.innerHTML = `
            <small class="text-muted">
                Mostrando ${inicio}-${fin} de ${totalRecords} registros
            </small>
        `;

        let paginacionHTML = '<ul class="pagination pagination-sm mb-0">';

        paginacionHTML += `
            <li class="page-item ${paginaActual === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="cambiarPagina(${paginaActual - 1}); return false;">«</a>
            </li>
        `;

        const paginasAMostrar = 5;
        let inicioPaginas = Math.max(1, paginaActual - Math.floor(paginasAMostrar / 2));
        let finPaginas = Math.min(totalPaginas, inicioPaginas + paginasAMostrar - 1);

        if (finPaginas - inicioPaginas + 1 < paginasAMostrar) {
            inicioPaginas = Math.max(1, finPaginas - paginasAMostrar + 1);
        }

        for (let i = inicioPaginas; i <= finPaginas; i++) {
            paginacionHTML += `
                <li class="page-item ${i === paginaActual ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="cambiarPagina(${i}); return false;">${i}</a>
                </li>
            `;
        }

        paginacionHTML += `
            <li class="page-item ${paginaActual === totalPaginas ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="cambiarPagina(${paginaActual + 1}); return false;">»</a>
            </li>
        `;

        paginacionHTML += '</ul>';
        paginacionNav.innerHTML = paginacionHTML;
    };

    // --- CAMBIAR PÁGINA ---
    window.cambiarPagina = (pagina) => {
        cargarDatosPaginated(pagina, searchTermActual);
    };

    // --- FUNCIÓN ORIGINAL (PARA COMPATIBILIDAD) ---
    const cargarDatosCompletos = async () => {
        try {
            const response = await fetch(usuariosApiUrl);
            listaUsuariosCompleta = await response.json();
            aplicarFiltrosClientes();
        } catch (error) {
            console.error("Error al cargar usuarios:", error);
        }
    };

    // --- APLICAR FILTROS (MODIFICADA PARA PAGINACIÓN) ---
    const aplicarFiltros = () => {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const rolSeleccionado = rolFilter.value;
        const fichaSeleccionada = fichaFilter.value;

        if (rolSeleccionado || fichaSeleccionada) {
            aplicarFiltrosClientes();
        } else {
            cargarDatosPaginated(1, searchTerm);
        }
    };

    // --- FILTRADO EN CLIENTE (para compatibilidad con rol/ficha) ---
    const aplicarFiltrosClientes = () => {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const rolSeleccionado = rolFilter.value;
        const fichaSeleccionada = fichaFilter.value;

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

            const matchSearch = searchTerm === '' ||
                nombre.toLowerCase().includes(searchTerm) ||
                documento.includes(searchTerm);

            const matchRol = rolSeleccionado === '' || rol === rolSeleccionado;

            const matchFicha = fichaSeleccionada === '' ||
                (fichaId && fichaId.toString() === fichaSeleccionada);

            return matchSearch && matchRol && matchFicha;
        });

        renderizarTabla(usuariosFiltrados);

        const paginacionContainer = document.getElementById('paginacionContainer');
        if (paginacionContainer) {
            paginacionContainer.style.display = 'none';
        }
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

            aprendizFichaIdInput.innerHTML = '<option value="">Seleccione una ficha...</option>';
            listaFichas.forEach(ficha => {
                aprendizFichaIdInput.innerHTML += `<option value="${ficha.idFicha}">${ficha.codigo} - ${ficha.programa.nombrePrograma}</option>`;
            });

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
        const paginacionContainer = document.getElementById('paginacionContainer');
        if (paginacionContainer) {
            paginacionContainer.style.display = 'flex';
        }
        cargarDatosPaginated(1, '');
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
                    funcionarioDetalleInput.value = usuario.funcionario.detalle || '';

                    // En modo edición, la contraseña es opcional
                    funcionarioContraseñaInput.removeAttribute('required');
                    funcionarioContraseñaInput.placeholder = 'Dejar en blanco para mantener la actual';
                }
            } else {
                // Modo creación - contraseña requerida
                funcionarioContraseñaInput.setAttribute('required', 'required');
                funcionarioContraseñaInput.placeholder = '';
            }
            funcionarioModal.show();
        }
    };

    // --- GUARDAR (CON ALERTAS MEJORADAS) ---
    window.guardar = async (tipo) => {
        let url, method, data, modal, id;

        if (tipo === 'aprendiz') {
            id = aprendizIdInput.value;
            url = id == 0 ? aprendizApiUrl : `${aprendizApiUrl}/${id}`;
            method = id == 0 ? 'POST' : 'PUT';
            modal = aprendizModal;
            data = {
                idAprendiz: parseInt(id) || 0,
                nombre: aprendizNombreInput.value,
                documento: aprendizDocumentoInput.value,
                idFicha: parseInt(aprendizFichaIdInput.value),
                estado: true
            };
        } else { // funcionario
            id = funcionarioIdInput.value;
            const password = funcionarioContraseñaInput.value.trim();

            url = id == 0 ? funcionarioApiUrl : `${funcionarioApiUrl}/${id}`;
            method = id == 0 ? 'POST' : 'PUT';
            modal = funcionarioModal;

            data = {
                idFuncionario: parseInt(id) || 0,
                nombre: funcionarioNombreInput.value,
                documento: funcionarioDocumentoInput.value,
                detalle: funcionarioDetalleInput.value || '',
                estado: true
            };

            // Lógica de contraseña
            if (id == 0) {
                // Modo CREAR: contraseña obligatoria
                if (!password) {
                    alert('La contraseña es requerida para crear un funcionario');
                    return;
                }
                data.contraseña = password;
            } else {
                // Modo EDITAR: solo incluir si se proporcionó una nueva
                if (password) {
                    data.contraseña = password;
                }
            }
        }

        try {
            console.log('Enviando datos:', data);

            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error('Error del servidor:', errorText);
                throw new Error(`Error ${response.status}: ${errorText}`);
            }

            modal.hide();
            cargarDatosPaginated(paginaActual, searchTermActual);

            // Mensaje de éxito diferenciado
            alert(id == 0 ? 'Creado exitosamente' : 'Actualizado exitosamente');

        } catch (error) {
            console.error(`Error al guardar ${tipo}:`, error);
            alert(`Error al guardar: ${error.message}`);
        }
    };

    // --- DESACTIVAR ---
    window.desactivar = async (idUsuario) => {
        if (!confirm(`¿Está seguro de que desea borrar (desactivar) al usuario con ID ${idUsuario}?`)) return;

        try {
            const response = await fetch(`${usuariosApiUrl}/${idUsuario}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al borrar el usuario.');

            cargarDatosPaginated(paginaActual, searchTermActual);
        } catch (error) {
            console.error(error);
            alert(error.message);
        }
    };

    // --- Carga inicial de datos ---
    cargarDatosPaginated(1, '');
    cargarFichas();
});