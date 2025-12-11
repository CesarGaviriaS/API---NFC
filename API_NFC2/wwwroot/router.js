// Router global CORREGIDO
window.AppRoutes = (function () {
    const page = {
        terminal: '/Terminal',
        elemento: '/Admin/Elemento',
        ficha: '/Admin/Ficha',
        programa: '/Admin/Programa',
        tipoElemento: '/Admin/TipoElemento',
        tipoProceso: '/Admin/TipoProceso',
        usuarios: '/Admin/Usuarios',
        aprendiz: '/Admin/Aprendiz',
        login: '/Login'
    };

    const apiBase = '/api';

    const api = {
        elementos: `${apiBase}/Elementoes`,
        fichas: `${apiBase}/Ficha`,
        programas: `${apiBase}/Programas`,
        tipoElementos: `${apiBase}/TipoElementoes`,
        tipoProcesos: `${apiBase}/TipoProcesoes`,
        usuarios: `${apiBase}/Usuario`,
        aprendiz: `${apiBase}/Aprendiz`,
        procesos: `${apiBase}/Procesoes`
    };

    function pagedUrl(resourceUrl, pageNumber = 1, pageSize = 10) {
        return `${resourceUrl}/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    }

    function navigateTo(path) {
        window.location.href = path;
    }

    return { page, api, pagedUrl, navigateTo };
})();