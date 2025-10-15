document.addEventListener('DOMContentLoaded', function () {
    // URLs de las APIs
    const elementoApiUrl = '/api/elemento';
    const tipoElementoApiUrl = '/api/tipoelemento';
    const usuarioApiUrl = '/api/usuario';

    const editorModal = new bootstrap.Modal(document.getElementById('editorModal'));

    // --- Referencias a los inputs del formulario ---
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

    let listaElementosCompleta = []; // Cache para editar
    let listaUsuariosCompleta = []; // Cache para buscar por documento

    // --- CARGAR DATOS PARA LOS SELECTORES (DROPDOWNS) ---
    const cargarTiposElemento = async () => {
        try {
            const response = await fetch(tipoElementoApiUrl);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const tipos = await response.json();

            tipoElementoIdInput.innerHTML = '<option value="">Seleccione un tipo...</option>';
            tipos.forEach(tipo => {
                tipoElementoIdInput.innerHTML += `<option value="${tipo.idTipoElemento}">${tipo.nombreTipoElemento}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar tipos de elemento:", error);
            tipoElementoIdInput.innerHTML = '<option value="">Error al cargar</option>';
        }
    };

    const cargarPropietarios = async () => {
        try {
            const response = await fetch(usuarioApiUrl);
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            listaUsuariosCompleta = await response.json(); // Guardamos en cache

            propietarioIdInput.innerHTML = '<option value="">Seleccione un propietario...</option>';
            listaUsuariosCompleta.forEach(user => {
                const rol = user.aprendiz ? 'Aprendiz' : 'Funcionario';
                const nombre = user.aprendiz ? user.aprendiz.nombre : user.funcionario.nombre;
                const documento = user.aprendiz ? user.aprendiz.documento : user.funcionario.documento;
                propietarioIdInput.innerHTML += `<option value="${user.idUsuario}">${rol} - ${nombre} - ${documento}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar propietarios:", error);
            propietarioIdInput.innerHTML = '<option value="">Error al cargar</option>';
        }
    };

    // --- LÓGICA DE BÚSQUEDA POR DOCUMENTO ---
    propietarioDocInput.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault(); // Evita que el formulario se envíe
            const doc = propietarioDocInput.value.trim();
            if (!doc) return;

            const usuarioEncontrado = listaUsuariosCompleta.find(user => {
                const documento = user.aprendiz ? user.aprendiz.documento : user.funcionario.documento;
                return documento === doc;
            });

            if (usuarioEncontrado) {
                // Si lo encontramos, seleccionamos la opción en el dropdown
                propietarioIdInput.value = usuarioEncontrado.idUsuario;
                // Y cambiamos a la otra pestaña para que el usuario vea su selección
                new bootstrap.Tab(document.getElementById('select-tab')).show();
                propietarioDocInput.value = ''; // Limpiamos el campo de búsqueda
            } else {
                alert('No se encontró ningún usuario con ese documento.');
            }
        }
    });

    // --- CARGAR DATOS EN LA TABLA PRINCIPAL ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(elementoApiUrl);
            if (!response.ok) throw new Error('Error al cargar elementos');
            listaElementosCompleta = await response.json();

            tablaDatos.innerHTML = '';
            if (listaElementosCompleta.length === 0) {
                tablaDatos.innerHTML = '<tr><td colspan="7" class="text-center">No hay elementos para mostrar.</td></tr>';
                return;
            }

            listaElementosCompleta.forEach(item => {
                const tipo = item.tipoElemento ? item.tipoElemento.nombreTipoElemento : 'N/A';

                // --- LÓGICA MEJORADA PARA MOSTRAR EL PROPIETARIO ---
                let propietarioInfo = '<em class="text-muted">N/A</em>';
                if (item.propietario) {
                    const nombre = item.propietario.aprendiz?.nombre || item.propietario.funcionario?.nombre;
                    const documento = item.propietario.aprendiz?.documento || item.propietario.funcionario?.documento;
                    propietarioInfo = `${nombre || ''} <br><small class="text-muted">${documento || ''}</small>`;
                }

                // --- LÓGICA PARA MOSTRAR LAS CARACTERÍSTICAS ---
                const caracteristicas = `
                <small><strong>Téc:</strong> ${item.caracteristicasTecnicas || 'N/A'}</small><br>
                <small><strong>Fís:</strong> ${item.caracteristicasFisicas || 'N/A'}</small><br>
                <small><strong>Det:</strong> ${item.detalles || 'N/A'}</small>
            `;

                tablaDatos.innerHTML += `
                <tr>
                    <td>${item.idElemento}</td>
                    <td>${item.nombreElemento}</td>
                    <td>
                        ${item.marca || 'N/A'}
                        <br><small class="text-muted">S/N: ${item.serial || 'N/A'}</small>
                    </td>
                    <td>${tipo}</td>
                    <td>${propietarioInfo}</td>
                    <td>${caracteristicas}</td>
                    <td>
                        <button class="btn btn-sm btn-warning" onclick="abrirModal(${item.idElemento})">
                            <i class="bi bi-pencil-fill"></i>
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="desactivar(${item.idElemento})">
                            <i class="bi bi-trash-fill"></i>
                        </button>
                    </td>
                </tr>`;
            });
        } catch (error) {
            console.error("Error al cargar elementos:", error);
            tablaDatos.innerHTML = `<tr><td colspan="7" class="text-center text-danger">${error.message}</td></tr>`;
        }
    };

    // --- ABRIR EL MODAL ---
    window.abrirModal = (id = 0) => {
        document.getElementById('formularioEditor').reset();
        idInput.value = id;

        if (id === 0) { // Crear nuevo
            modalTitulo.textContent = 'Crear Nuevo Elemento';
        }
        else { // Editar existente
            modalTitulo.textContent = 'Editar Elemento';
            const elemento = listaElementosCompleta.find(e => e.idElemento === id);
            if (elemento) {
                nombreInput.value = elemento.nombreElemento;
                marcaInput.value = elemento.marca;
                serialInput.value = elemento.serial;
                tieneNFCInput.value = elemento.tieneNFCTag;
                tipoElementoIdInput.value = elemento.idTipoElemento;
                propietarioIdInput.value = elemento.idPropietario;
                caracteristicasTecnicasInput.value = elemento.caracteristicasTecnicas;
                caracteristicasFisicasInput.value = elemento.caracteristicasFisicas;
                detallesInput.value = elemento.detalles;
                imageUrlInput.value = elemento.imageUrl;
            }
        }
        editorModal.show();
    };

    // --- GUARDAR ---
    window.guardar = async () => {
        const id = idInput.value;
        const esNuevo = id == 0;

        const data = {
            idElemento: parseInt(id) || 0,
            nombreElemento: nombreInput.value,
            marca: marcaInput.value,
            serial: serialInput.value,
            tieneNFCTag: tieneNFCInput.value === 'true',
            idTipoElemento: parseInt(tipoElementoIdInput.value),
            idPropietario: parseInt(propietarioIdInput.value),
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
            cargarDatos();
        }
        catch (error) {
            alert(`Error: ${error.message}`);
        }
    };

    // --- DESACTIVAR ---
    window.desactivar = async (id) => {
        if (!confirm('¿Está seguro de que desea borrar (desactivar) este elemento?')) return;
        try {
            const response = await fetch(`${elementoApiUrl}/${id}`, { method: 'DELETE' });
            if (!response.ok) throw new Error('Error al borrar.');
            cargarDatos();
        }
        catch (error) {
            alert(error.message);
        }
    };




    // --- Carga inicial de todos los datos ---
    cargarDatos();
    cargarTiposElemento();
    cargarPropietarios();
});