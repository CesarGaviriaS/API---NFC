document.addEventListener('DOMContentLoaded', function () {
    // Referencias al DOM
    const statusIndicator = document.getElementById('statusIndicator');
    const statusText = document.getElementById('statusText');
    const readContent = document.getElementById('readContent').querySelector('code');
    const writeDataInput = document.getElementById('writeDataInput');
    const prepareWriteBtn = document.getElementById('prepareWriteBtn');
    const prepareClearBtn = document.getElementById('prepareClearBtn');

    // --- Conexión SignalR ---
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/nfcHub")
        .withAutomaticReconnect()
        .build();

    // Función para actualizar el estado visual
    const updateStatus = (message, alertClass) => {
        statusText.textContent = message;
        statusIndicator.className = `alert alert-${alertClass} mb-0 py-2`;
    };

    // --- Escuchar eventos del HUB ---

    // El agente nos dice que leyó algo
    connection.on("RecibirDatosTag", (tagData) => {
        readContent.textContent = tagData;
        updateStatus("Tag leído con éxito", "success");
    });

    // El agente nos da una confirmación de una acción
    connection.on("MostrarConfirmacion", (tipoAccion, mensaje) => {
        updateStatus(`${tipoAccion}: ${mensaje}`, "success");
    });

    // El agente nos reporta un error
    connection.on("MostrarError", (mensaje) => {
        updateStatus(`Error: ${mensaje}`, "danger");
    });

    // --- Enviar comandos al HUB ---

    // Botón para preparar la escritura
    prepareWriteBtn.addEventListener('click', async () => {
        const data = writeDataInput.value;
        if (!data) {
            alert('Por favor, ingrese datos para escribir.');
            return;
        }
        try {
            updateStatus("Enviando comando de escritura...", "info");
            await fetch('/api/agente/preparar-escritura', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ datos: data })
            });
            updateStatus("Comando enviado. Esperando tag...", "warning");
        } catch (error) {
            updateStatus("Error al enviar comando", "danger");
        }
    });

    // Botón para preparar la limpieza
    prepareClearBtn.addEventListener('click', async () => {
        if (!confirm('¿Está seguro? Esta acción es irreversible.')) return;
        try {
            updateStatus("Enviando comando de limpieza...", "info");
            await fetch('/api/agente/preparar-limpieza', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }
            });
            updateStatus("Comando enviado. Esperando tag para limpiar...", "warning");
        } catch (error) {
            updateStatus("Error al enviar comando", "danger");
        }
    });

    // Iniciar la conexión de SignalR
    async function startSignalR() {
        try {
            await connection.start();
            updateStatus("Conectado al Hub", "success");
        } catch (err) {
            console.error("Error de conexión con SignalR:", err);
            updateStatus("Desconectado", "danger");
            setTimeout(startSignalR, 5000);
        }
    }

    startSignalR();
});