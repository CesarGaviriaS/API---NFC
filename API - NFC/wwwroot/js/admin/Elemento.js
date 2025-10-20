document.addEventListener('DOMContentLoaded', function () {
    // URLs de las APIs
    const elementoApiUrl = '/api/elemento';
    const elementoPaginatedApiUrl = '/api/elemento/paginated';
    const tipoElementoApiUrl = '/api/tipoelemento';
    const usuarioApiUrl = '/api/usuario';

    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));

    // Referencias a los inputs del formulario
    const idInput = document.getElementById('idInput');
    const nombreInput = document.getElementById('nombreInput');
    const marcaInput = document.getElementById('marcaInput');
    const serialInput = document.getElementById('serialInput');
    const tieneNFCInput = document.getElementById('tieneNFCInput');
    const tipoElementoIdInput = document.getElementById('tipoElementoIdInput');
    const propietarioIdInput = document.getElementById('propietarioIdInput');
    const propietarioDocInput = document.getElementById('propietarioDocInput');
    const caracteristicasTecnicasInput = document.getElementById('caracteristicasTecnicasInput');
    const caracteristicasFisicasInput = document.getElementById('caracteristicasFisicasInput');
    const detallesInput = document.getElementById('detallesInput');
    const imageUrlInput = document.getElementById('imageUrlInput');
    const modalTitulo = document.getElementById('modalTitulo');
    const tablaDatos = document.getElementById('tablaDatos');

    // Elementos de filtrado
    const searchInput = document.getElementById('searchInput');
    const tipoFilter = document.getElementById('tipoFilter');
    const propietarioFilter = document.getElementById('propietarioFilter');
    const nfcFilter = document.getElementById('nfcFilter');
    const resultadosContador = document.getElementById('resultadosContador');
    const emptyState = document.getElementById('emptyState');

    let listaElementosCompleta = [];
    let listaUsuariosCompleta = [];
    let listaTiposElemento = [];

    // Variables para la paginación 
    let paginaActual = 1;
    let registrosPorPagina = 10;
    let searchTermActual = '';

    // --- CARGAR DATOS PAGINADOS ---
    const cargarDatosPaginated = async (pagina = 1, searchTerm = '') => {
        try {
            console.log('Cargando página:', pagina, 'Search:', searchTerm);
            paginaActual = pagina;
            searchTermActual = searchTerm;

            const params = new URLSearchParams({
                page: pagina,
                pageSize: registrosPorPagina
            });

            if (searchTerm) {
                params.append('search', searchTerm);
            }

            const url = `${elementoPaginatedApiUrl}?${params}`;
            console.log('URL de petición:', url);

            const response = await fetch(url);
            if (!response.ok) throw new Error(`Error HTTP: ${response.status}`);

            const data = await response.json();
            console.log('Datos recibidos:', data);

            listaElementosCompleta = data.data || [];
            renderizarTabla(listaElementosCompleta);
            generarPaginacion(data.page, data.totalPages, data.totalRecords);

        } catch (error) {
            console.error("Error al cargar datos paginados:", error);
            // Fallback: cargar sin paginación
            await cargarDatosCompletos();
        }
    };

    // --- GENERAR PAGINACIÓN ---
    const generarPaginacion = (paginaActual, totalPaginas, totalRecords) => {
        console.log('Generando paginación:', { paginaActual, totalPaginas, totalRecords });

        let paginacionContainer = document.getElementById('paginacionContainer');

        if (!paginacionContainer) {
            const tablaParent = document.querySelector('.table-responsive').parentNode;
            paginacionContainer = document.createElement('div');
            paginacionContainer.className = 'd-flex justify-content-between align-items-center mt-3';
            paginacionContainer.id = 'paginacionContainer';
            paginacionContainer.innerHTML = `
                <div id="infoPaginacion"></div>
                <nav id="paginacionNav"></nav>
            `;
            tablaParent.appendChild(paginacionContainer);
        }

        const infoPaginacion = document.getElementById('infoPaginacion');
        const paginacionNav = document.getElementById('paginacionNav');

        // Mostrar contenedor
        paginacionContainer.style.display = 'flex';

        // Información de paginación
        const inicio = ((paginaActual - 1) * registrosPorPagina) + 1;
        const fin = Math.min(paginaActual * registrosPorPagina, totalRecords);

        infoPaginacion.innerHTML = `
            <small class="text-muted">
                Mostrando ${inicio}-${fin} de ${totalRecords} registros
            </small>
        `;

        // Botones de paginación
        let paginacionHTML = '<ul class="pagination pagination-sm mb-0">';

        // Botón anterior
        if (paginaActual > 1) {
            paginacionHTML += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="cambiarPagina(${paginaActual - 1}); return false;">« Anterior</a>
                </li>
            `;
        }

        // Botones de páginas
        for (let i = 1; i <= totalPaginas; i++) {
            if (i === 1 || i === totalPaginas || (i >= paginaActual - 1 && i <= paginaActual + 1)) {
                paginacionHTML += `
                    <li class="page-item ${i === paginaActual ? 'active' : ''}">
                        <a class="page-link" href="#" onclick="cambiarPagina(${i}); return false;">${i}</a>
                    </li>
                `;
            } else if (i === paginaActual - 2 || i === paginaActual + 2) {
                paginacionHTML += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        // Botón siguiente
        if (paginaActual < totalPaginas) {
            paginacionHTML += `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="cambiarPagina(${paginaActual + 1}); return false;">Siguiente »</a>
                </li>
            `;
        }

        paginacionHTML += '</ul>';
        paginacionNav.innerHTML = paginacionHTML;
    };

    // --- CAMBIAR PÁGINA ---
    window.cambiarPagina = (pagina) => {
        console.log('Cambiando a página:', pagina);
        cargarDatosPaginated(pagina, searchTermActual);
    };

    // --- FUNCIÓN ORIGINAL (PARA COMPATIBILIDAD) ---
    const cargarDatosCompletos = async () => {
        try {
            const response = await fetch(elementoApiUrl);
            listaElementosCompleta = await response.json();
            aplicarFiltrosClientes();
        } catch (error) {
            console.error("Error al cargar elementos:", error);
        }
    };

    // --- APLICAR FILTROS (MODIFICADA PARA PAGINACIÓN) ---
    const aplicarFiltros = () => {
        const searchTerm = searchInput.value.trim();
        const tipoSeleccionado = tipoFilter.value;
        const propietarioSeleccionado = propietarioFilter.value;
        const nfcSeleccionado = nfcFilter.value;

        console.log('Aplicando filtros:', { searchTerm, tipoSeleccionado, propietarioSeleccionado, nfcSeleccionado });

        // Si hay filtros de tipo, propietario o NFC, usar filtrado cliente
        if (tipoSeleccionado || propietarioSeleccionado || nfcSeleccionado) {
            aplicarFiltrosClientes();
        } else {
            // Solo búsqueda de texto - usar paginación servidor
            cargarDatosPaginated(1, searchTerm);
        }
    };

    // --- FILTRADO EN CLIENTE (para compatibilidad con tipo/propietario/NFC) ---
    const aplicarFiltrosClientes = () => {
        const searchTerm = searchInput.value.toLowerCase().trim();
        const tipoSeleccionado = tipoFilter.value;
        const propietarioSeleccionado = propietarioFilter.value;
        const nfcSeleccionado = nfcFilter.value;

        const elementosFiltrados = listaElementosCompleta.filter(elemento => {
            // Filtro de búsqueda
            const matchSearch = searchTerm === '' ||
                elemento.nombreElemento.toLowerCase().includes(searchTerm) ||
                (elemento.marca && elemento.marca.toLowerCase().includes(searchTerm)) ||
                (elemento.serial && elemento.serial.toLowerCase().includes(searchTerm));

            // Filtro de tipo
            const matchTipo = tipoSeleccionado === '' ||
                (elemento.idTipoElemento && elemento.idTipoElemento.toString() === tipoSeleccionado);

            // Filtro de propietario
            const matchPropietario = propietarioSeleccionado === '' ||
                (elemento.idPropietario && elemento.idPropietario.toString() === propietarioSeleccionado);

            // Filtro de NFC
            const matchNFC = nfcSeleccionado === '' ||
                (nfcSeleccionado === 'true' && elemento.tieneNFCTag === true) ||
                (nfcSeleccionado === 'false' && elemento.tieneNFCTag === false);

            return matchSearch && matchTipo && matchPropietario && matchNFC;
        });

        renderizarTabla(elementosFiltrados);

        // Ocultar paginación cuando filtramos en cliente
        const paginacionContainer = document.getElementById('paginacionContainer');
        if (paginacionContainer) {
            paginacionContainer.style.display = 'none';
        }
    };

    // --- RENDERIZAR TABLA ---
    const renderizarTabla = (elementos) => {
        console.log('Renderizando tabla con:', elementos.length, 'elementos');
        tablaDatos.innerHTML = '';
        resultadosContador.textContent = elementos.length;

        if (elementos.length === 0) {
            emptyState.classList.remove('d-none');
            return;
        }

        emptyState.classList.add('d-none');

        elementos.forEach(item => {
            const tipo = item.tipoElemento ? item.tipoElemento.nombreTipoElemento : 'N/A';

            let propietarioInfo = '<em class="text-muted">N/A</em>';
            if (item.propietario) {
                const nombre = item.propietario.aprendiz?.nombre || item.propietario.funcionario?.nombre;
                const documento = item.propietario.aprendiz?.documento || item.propietario.funcionario?.documento;
                propietarioInfo = `${nombre || ''} <br><small class="text-muted">${documento || ''}</small>`;
            }

            const nfcBadge = item.tieneNFCTag
                ? '<span class="badge bg-success"><i class="bi bi-wifi me-1"></i>Sí</span>'
                : '<span class="badge bg-secondary"><i class="bi bi-wifi-off me-1"></i>No</span>';

            tablaDatos.innerHTML += `
                <tr>
                    <td>${item.idElemento}</td>
                    <td class="fw-semibold">${item.nombreElemento}</td>
                    <td>
                        ${item.marca || 'N/A'}
                        <br><small class="text-muted">S/N: ${item.serial || 'N/A'}</small>
                    </td>
                    <td><span class="badge bg-info text-dark">${tipo}</span></td>
                    <td>${propietarioInfo}</td>
                    <td>${nfcBadge}</td>
                    <td class="text-center">
                        <button class="btn btn-sm btn-warning" onclick="abrirModal(${item.idElemento})">
                            <i class="bi bi-pencil-fill"></i>
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idElemento})">
                            <i class="bi bi-trash-fill"></i>
                        </button>
                    </td>
                </tr>`;
        });
    };

    // --- CARGAR TIPOS DE ELEMENTO ---
    const cargarTiposElemento = async () => {
        try {
            const response = await fetch(tipoElementoApiUrl);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            listaTiposElemento = await response.json();

            // Llenar select del modal
            tipoElementoIdInput.innerHTML = '<option value="">Seleccione un tipo...</option>';
            listaTiposElemento.forEach(tipo => {
                tipoElementoIdInput.innerHTML += `<option value="${tipo.idTipoElemento}">${tipo.nombreTipoElemento}</option>`;
            });

            // Llenar filtro de tipos
            tipoFilter.innerHTML = '<option value="">Todos los tipos</option>';
            listaTiposElemento.forEach(tipo => {
                tipoFilter.innerHTML += `<option value="${tipo.idTipoElemento}">${tipo.nombreTipoElemento}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar tipos de elemento:", error);
        }
    };

    // --- CARGAR PROPIETARIOS ---
    const cargarPropietarios = async () => {
        try {
            const response = await fetch(usuarioApiUrl);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            listaUsuariosCompleta = await response.json();

            // Llenar select del modal
            propietarioIdInput.innerHTML = '<option value="">Seleccione un propietario...</option>';
            listaUsuariosCompleta.forEach(user => {
                const rol = user.aprendiz ? 'Aprendiz' : 'Funcionario';
                const nombre = user.aprendiz ? user.aprendiz.nombre : user.funcionario.nombre;
                const documento = user.aprendiz ? user.aprendiz.documento : user.funcionario.documento;
                propietarioIdInput.innerHTML += `<option value="${user.idUsuario}">${rol} - ${nombre} - ${documento}</option>`;
            });

            // Llenar filtro de propietarios
            propietarioFilter.innerHTML = '<option value="">Todos los propietarios</option>';
            listaUsuariosCompleta.forEach(user => {
                const nombre = user.aprendiz ? user.aprendiz.nombre : user.funcionario.nombre;
                propietarioFilter.innerHTML += `<option value="${user.idUsuario}">${nombre}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar propietarios:", error);
        }
    };

    // --- BÚSQUEDA POR DOCUMENTO ---
    propietarioDocInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            const doc = propietarioDocInput.value.trim();
            if (!doc) return;

            const usuarioEncontrado = listaUsuariosCompleta.find(user => {
                const documento = user.aprendiz ? user.aprendiz.documento : user.funcionario.documento;
                return documento === doc;
            });

            if (usuarioEncontrado) {
                propietarioIdInput.value = usuarioEncontrado.idUsuario;
                // Cambiar a la pestaña de selección
                const selectTab = document.getElementById('select-tab');
                if (selectTab) {
                    new bootstrap.Tab(selectTab).show();
                }
                propietarioDocInput.value = '';
            } else {
                alert('No se encontró ningún usuario con ese documento.');
            }
        }
    });

    // --- LIMPIAR FILTROS ---
    window.limpiarFiltros = () => {
        searchInput.value = '';
        tipoFilter.value = '';
        propietarioFilter.value = '';
        nfcFilter.value = '';
        // Mostrar paginación nuevamente
        const paginacionContainer = document.getElementById('paginacionContainer');
        if (paginacionContainer) {
            paginacionContainer.style.display = 'flex';
        }
        cargarDatosPaginated(1, '');
    };

    // --- EVENT LISTENERS PARA FILTROS ---
    searchInput.addEventListener('input', aplicarFiltros);
    tipoFilter.addEventListener('change', aplicarFiltros);
    propietarioFilter.addEventListener('change', aplicarFiltros);
    nfcFilter.addEventListener('change', aplicarFiltros);

    // --- ABRIR MODAL ---
    window.abrirModal = (id = 0) => {
        document.getElementById('formularioEditor').reset();
        idInput.value = id;

        if (id === 0) {
            modalTitulo.textContent = 'Crear Nuevo Elemento';
        } else {
            modalTitulo.textContent = 'Editar Elemento';
            const elemento = listaElementosCompleta.find(e => e.idElemento === id);
            if (elemento) {
                nombreInput.value = elemento.nombreElemento || '';
                marcaInput.value = elemento.marca || '';
                serialInput.value = elemento.serial || '';
                tieneNFCInput.value = elemento.tieneNFCTag;
                tipoElementoIdInput.value = elemento.idTipoElemento || '';
                propietarioIdInput.value = elemento.idPropietario || '';
                caracteristicasTecnicasInput.value = elemento.caracteristicasTecnicas || '';
                caracteristicasFisicasInput.value = elemento.caracteristicasFisicas || '';
                detallesInput.value = elemento.detalles || '';
                imageUrlInput.value = elemento.imageUrl || '';
            }
        }
        editorModal.show();
    };

    // --- GUARDAR (ACTUALIZADO PARA PAGINACIÓN) ---
    window.guardar = async () => {
        const id = idInput.value;
        const esNuevo = id == 0;

        const data = {
            idElemento: parseInt(id) || 0,
            nombreElemento: nombreInput.value,
            marca: marcaInput.value,
            serial: serialInput.value,
            tieneNFCTag: tieneNFCInput.value === 'true',
            idTipoElemento: parseInt(tipoElementoIdInput.value) || null,
            idPropietario: parseInt(propietarioIdInput.value) || null,
            caracteristicasTecnicas: caracteristicasTecnicasInput.value,
            caracteristicasFisicas: caracteristicasFisicasInput.value,
            detalles: detallesInput.value,
            imageUrl: imageUrlInput.value
        };

        const url = esNuevo ? elementoApiUrl : `${elementoApiUrl}/${id}`;
        const method = esNuevo ? 'POST' : 'PUT';

        try {
            const response = await fetch(url, {
                method: method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            if (!response.ok) throw new Error(await response.text());

            editorModal.hide();
            // Recargar manteniendo la paginación actual
            cargarDatosPaginated(paginaActual, searchTermActual);
        } catch (error) {
            alert(`Error: ${error.message}`);
        }
    };

    // --- DESACTIVAR (ACTUALIZADO PARA PAGINACIÓN) ---
    window.desactivar = async (id) => {
        if (!confirm('¿Está seguro de que desea borrar (desactivar) este elemento?')) return;
        try {
            const response = await fetch(`${elementoApiUrl}/${id}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al borrar.');
            // Recargar manteniendo la paginación actual
            cargarDatosPaginated(paginaActual, searchTermActual);
        } catch (error) {
            alert(error.message);
        }
    };

    // --- CARGA INICIAL ---
    cargarDatosPaginated(1, ''); // Usar paginación por defecto
    cargarTiposElemento();
    cargarPropietarios();
});