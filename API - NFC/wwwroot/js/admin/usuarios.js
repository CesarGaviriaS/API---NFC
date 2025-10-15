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

    const tablaDatos = document.getElementById('tablaDatos');
    let listaUsuariosCompleta = []; // Almacenamos los datos para la edición

    // --- CARGAR DATOS EN LA TABLA PRINCIPAL ---
    const cargarDatos = async () => {
        try {
            const response = await fetch(usuariosApiUrl);
            listaUsuariosCompleta = await response.json();

            tablaDatos.innerHTML = '';
            if (listaUsuariosCompleta.length === 0) {
                tablaDatos.innerHTML = '<tr><td colspan="6" class="text-center">No hay usuarios para mostrar.</td></tr>';
                return;
            }

            listaUsuariosCompleta.forEach(user => {
                let rol, nombre, documento, detalle, editId;
                if (user.aprendiz) {
                    rol = 'Aprendiz';
                    nombre = user.aprendiz.nombre;
                    documento = user.aprendiz.documento;
                    detalle = user.aprendiz.ficha ? user.aprendiz.ficha.codigo : 'Sin Ficha';
                    editId = user.aprendiz.idAprendiz;
                } else if (user.funcionario) {
                    rol = 'Funcionario';
                    nombre = user.funcionario.nombre;
                    documento = user.funcionario.documento;
                    detalle = user.funcionario.detalle || 'N/A';
                    editId = user.funcionario.idFuncionario;
                }

                tablaDatos.innerHTML += `
                    <tr>
                        <td>${user.idUsuario}</td>
                        <td><span class="badge bg-${rol === 'Aprendiz' ? 'success' : 'info'}">${rol}</span></td>
                        <td>${nombre}</td>
                        <td>${documento}</td>
                        <td>${detalle}</td>
                        <td>
                            <button class="btn btn-sm btn-warning" onclick="abrirModal('${rol.toLowerCase()}', ${editId})">
                                <i class="bi bi-pencil-fill"></i> Editar
                            </button>
                            <button class="btn btn-sm btn-danger" onclick="desactivar(${user.idUsuario})">
                                <i class="bi bi-trash-fill"></i> Borrar
                            </button>
                        </td>
                    </tr>`;
            });
        } catch (error) {
            console.error("Error al cargar usuarios:", error);
        }
    };

    // --- CARGAR FICHAS PARA EL SELECTOR DEL MODAL DE APRENDIZ ---
    const cargarFichas = async () => {
        try {
            const response = await fetch(fichasApiUrl);
            const fichas = await response.json();
            aprendizFichaIdInput.innerHTML = '<option value="">Seleccione una ficha...</option>';
            fichas.forEach(ficha => {
                aprendizFichaIdInput.innerHTML += `<option value="${ficha.idFicha}">${ficha.codigo} - ${ficha.programa.nombrePrograma}</option>`;
            });
        } catch (error) {
            console.error("Error al cargar fichas:", error);
        }
    };

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