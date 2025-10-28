// Router global (accesible como window.AppRoutes)
window.AppRoutes = (function () {
    const page = {
        terminal: '/Terminal',
        elemento: '/Admin/Elemento',
        ficha: '/Admin/Ficha',
        programa: '/Admin/Programa',
        tipoElemento: '/Admin/TipoElemento',
        tipoProceso: '/Admin/TipoProceso',
        usuarios: '/Admin/Usuarios',
        login: '/Login'
    };

    const apiBase = '/api';

    const api = {
        elementos: `${apiBase}/elementos`,
        fichas: `${apiBase}/fichas`,
        programas: `${apiBase}/programas`,
        tipoElementos: `${apiBase}/tipoelementos`,
        tipoProcesos: `${apiBase}/tipoprocesos`,
        usuarios: `${apiBase}/usuarios`,
        registrosNfc: `${apiBase}/registrosnfc`,
        procesos: `${apiBase}/procesos`
    };

    function paginatedUrl(resourceUrl, page = 1, pageSize = 10, search = '') {
        const params = new URLSearchParams({ page, pageSize });
        if (search) params.append('search', search);
        return `${resourceUrl}/paginated?${params.toString()}`;
    }

    function navigateTo(path) { window.location.href = path; }

    return { page, api, paginatedUrl, navigateTo };
})();